#nullable enable
using System;
using Sirenix.OdinInspector;

namespace STool.SBlackBoard
{
    /// <summary>
    /// Strongly-typed value for authoring (default/override).
    /// Serialized polymorphically by Odin.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IBBValue
    {
        Type ValueType { get; }
        object? BoxedValue { get; }
        public void InsertKey(Blackboard blackboard, string key);
        public BBKey GetKey();
    }

    /// <summary>
    /// Generic base for strongly typed values.
    /// </summary>
    [Serializable]
    public abstract class BBValue<T> : IBBValue
    {
        public abstract T Value { get; set; }
        public Type ValueType => typeof(T);
        public object? BoxedValue => Value;
        public virtual BBKey GetKey() => BBKeyShare.GetSharedKey<T>(string.Empty);
        public void InsertKey(Blackboard? blackboard, string key)
        {
            blackboard?.Create(new BBKey<T>(key), Value);
        }
    }
}