using System;

namespace STool.ValueExtension
{
    [Serializable] public abstract class BoxValue { public abstract Type ValueType { get; } }
    [Serializable] public class BoxValue<T> : BoxValue
    { 
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if(_value.Equals(value)) return;
                _value = value;
                onChange.Invoke(_value);
            }
        }
        public override Type ValueType => typeof(T);
        public UnityEngine.Events.UnityEvent<T> onChange = new UnityEngine.Events.UnityEvent<T>();
    }
}
