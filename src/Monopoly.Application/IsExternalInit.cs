// File: IsExternalInit.cs
// Dành cho build netstandard2.1 — để hỗ trợ record/init/required property

#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    using System;

    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name) { }
        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
    }
}
#endif
