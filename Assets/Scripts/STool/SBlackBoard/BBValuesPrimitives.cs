#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.SBlackBoard
{
    [Serializable]
    public sealed class BBInt : BBValue<int>
    {
        [SerializeField, HideLabel] private int _value;
        public override int Value { get => _value; set => _value = value; }
    }

    [Serializable]
    public sealed class BBFloat : BBValue<float>
    {
        [SerializeField, HideLabel] private float _value;
        public override float Value { get => _value; set => _value = value; }
    }

    [Serializable]
    public sealed class BBBool : BBValue<bool>
    {
        [SerializeField, HideLabel] private bool _value;
        public override bool Value { get => _value; set => _value = value; }
    }

    [Serializable]
    public sealed class BBString : BBValue<string>
    {
        [SerializeField, HideLabel] private string _value = "";
        public override string Value { get => _value; set => _value = value ?? ""; }
    }

    [Serializable]
    public sealed class BBVector3 : BBValue<Vector3>
    {
        [SerializeField, HideLabel] private Vector3 _value;
        public override Vector3 Value { get => _value; set => _value = value; }
    }

    /// <summary>
    /// For UnityEngine.Object references (GameObject, Transform, ScriptableObject, etc.)
    /// Stored as UnityEngine.Object (base type). If you want stronger constraints, add derived wrappers.
    /// </summary>
    [Serializable]
    public sealed class BBUnityObject : BBValue<UnityEngine.Object?>
    {
        [SerializeField, HideLabel] private UnityEngine.Object? _value;
        public override UnityEngine.Object? Value { get => _value; set => _value = value; }
    }
}