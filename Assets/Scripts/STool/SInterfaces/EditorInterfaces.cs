using STool.CollectionUtility;
using UnityEngine;

namespace STool.SInterfaces
{
    public interface IGizmosDrawing
    {
        #if UNITY_EDITOR
        public void DrawGizmos(BucketDictionary buffer);
        #endif
    }

    public interface IHandlesDrawing
    {
        #if UNITY_EDITOR
        public void DrawHandles(BucketDictionary buffer);
        #endif
    }

    public interface IBakeTarget
    {
        #if UNITY_EDITOR
        public void DoBake();
        #endif
    }

    #if UNITY_EDITOR
    public static class EditorDrawing
    {
        public const string Position = "position";
        public const string Rotation = "rotation";
        public const string Scale = "scale";
        public const string Time = "time";

        public static readonly GUIStyle RightAlignStyle = new(UnityEditor.EditorStyles.label) { alignment = TextAnchor.MiddleRight};
        public static readonly GUIStyle LeftAlignStyle = new(UnityEditor.EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
        public static readonly GUIStyle UpMiddleStyle = new(UnityEditor.EditorStyles.label) { alignment = TextAnchor.UpperCenter };
        public static readonly GUIStyle DownMiddleStyle = new(UnityEditor.EditorStyles.label) { alignment = TextAnchor.LowerCenter };

        public static readonly UnityEditor.Handles.CapFunction SphereButton = UnityEditor.Handles.SphereHandleCap;
        
        public static string ToBinary(ulong v, int bits = 64)
        {
            var s = new char[bits];
            for (var i = bits - 1; i >= 0; i--)
                s[bits - 1 - i] = ((v >> i) & 1UL) != 0 ? '1' : '0';
            return new string(s);
        }
    }
    #endif
}
