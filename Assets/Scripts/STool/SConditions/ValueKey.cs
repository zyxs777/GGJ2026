using System;

namespace STool.SConditions
{
    [Serializable]
    public sealed class ValueKey<T>
    {
        
        public string Name { get; }
        public T DefaultValue { get; }

        public ValueKey(string name, T defaultValue = default!)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public override string ToString() => $"ValueKey<{typeof(T).Name}>({Name})";
    }
}