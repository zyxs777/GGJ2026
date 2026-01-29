using System;
using UnityEngine;

namespace STool.EditorExtension.SEditorExtension.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TimelineViewAttribute : PropertyAttribute
    {
        public float Height = 64f;
        public bool DrawTicks = true;
        public bool DrawLabels = true;

        public TimelineViewAttribute(float height = 64f)
        {
            Height = height;
        }
    }
}