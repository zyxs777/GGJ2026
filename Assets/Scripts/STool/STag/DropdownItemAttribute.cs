using System;

namespace STool.STag
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class DropdownItemAttribute : Attribute
    {
        public readonly string Group;
        public readonly string Label;

        public DropdownItemAttribute(string group, string label = null)
        {
            Group = group;
            Label = label;
        }
    }
}