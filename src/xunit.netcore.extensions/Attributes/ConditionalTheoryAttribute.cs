// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit.Sdk;

namespace Xunit
{
    [XunitTestCaseDiscoverer("Xunit.NetCore.Extensions.ConditionalTheoryDiscoverer", "Xunit.NetCore.Extensions")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ConditionalTheoryAttribute : TheoryAttribute
    {
        public string ConditionMemberName { get; private set; }

        public ConditionalTheoryAttribute(string conditionMemberName)
        {
            ConditionMemberName = conditionMemberName;
        }
    }
}
