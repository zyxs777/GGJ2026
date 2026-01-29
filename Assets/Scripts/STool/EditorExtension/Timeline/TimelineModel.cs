using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace STool.EditorExtension.Timeline
{
    public static class TimelineDrawConst
    {
        public static readonly Color PointColor = new Color(1f, 0.7f, 0.2f, 0.85f);
        public static readonly Color SpanStartColor = new(0.2f, 0.4f, .1f, 0.55f);
        public static readonly Color SpanEndColor = new(0.2f, 0.8f, 1f, 0.55f);
        public static readonly Color SpanDurationColor =  new Color(0.2f, 0.8f, 1f, 0.12f);
    }
    
    [Serializable]
    public struct TimelinePoint
    {
        [Min(0)] public float time;
        public string label;
        public Color color;
        
        public TimelinePoint(float time)
        {
            this.time = time;
            label = "";
            color = TimelineDrawConst.PointColor;
        }
    }

    [Serializable]
    public struct TimelineSpan
    {
        [Min(0)] public float start;
        [Min(0)] public float end;
        public string label;
        public Color startColor;
        public Color endColor;
        public Color durationColor;
        
        public TimelineSpan(float start, float end)
        {
            this.start = start;
            this.end = end;
            label = "";
            startColor = TimelineDrawConst.SpanStartColor;
            endColor = TimelineDrawConst.SpanEndColor;
            durationColor = TimelineDrawConst.SpanDurationColor;
        }
    }

    /// <summary>只负责承载数据：时长、点、区间。</summary>
    [Serializable]
    public class TimelineModel
    {
        [Min(0.0001f)] [ShowIf("showContainers")] public float duration = 10f;
        [HideInInspector] public bool showContainers = true;
        
        [ShowIf("showContainers")] public List<TimelinePoint> points = new();
        [ShowIf("showContainers")] public List<TimelineSpan> spans = new();
    }
}