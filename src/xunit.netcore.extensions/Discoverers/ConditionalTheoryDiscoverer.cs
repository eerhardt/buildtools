// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.NetCore.Extensions
{
    public class ConditionalTheoryDiscoverer : TheoryDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public ConditionalTheoryDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public override IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            MethodInfo testMethodInfo = testMethod.Method.ToRuntimeMethod();

            string conditionMemberName = theoryAttribute.GetConstructorArguments().FirstOrDefault() as string;
            Type declaringType = testMethodInfo.DeclaringType;
            string[] symbols = conditionMemberName.Split('.');

            if (symbols.Length == 2)
            {
                conditionMemberName = symbols[1];
                ITypeInfo type = testMethod.TestClass.Class.Assembly.GetTypes(false).Where(t => t.Name.Contains(symbols[0])).FirstOrDefault();
                if (type != null)
                {
                    declaringType = type.ToRuntimeType();
                }
            }

            MethodInfo conditionMethodInfo;
            if (conditionMemberName == null ||
                (conditionMethodInfo = ConditionalFactDiscoverer.LookupConditionalMethod(declaringType, conditionMemberName)) == null)
            {
                return new[] {
                    new ExecutionErrorTestCase(
                        _diagnosticMessageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        testMethod,
                        ConditionalFactDiscoverer.GetFailedLookupString(conditionMemberName))
                };
            }

            IEnumerable<IXunitTestCase> testCases = base.Discover(discoveryOptions, testMethod, theoryAttribute);
            if ((bool)conditionMethodInfo.Invoke(null, null))
            {
                return testCases;
            }
            else
            {
                string skippedReason = "\"" + conditionMemberName + "\" returned false.";
                return testCases.Select(tc => new SkippedTestCase(tc, skippedReason));
            }
        }
    }
}
