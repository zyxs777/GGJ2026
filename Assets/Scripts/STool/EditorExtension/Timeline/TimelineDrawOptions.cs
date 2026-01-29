#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace STool.EditorExtension.Timeline
{
    public readonly struct TimelineDrawOptions
    {
        public readonly float Height;
        public readonly float Padding;
        public readonly int MaxTicks;
        public readonly bool DrawTicks;
        public readonly bool DrawLabels;

        public TimelineDrawOptions(
            float height = 64f,
            float padding = 8f,
            int maxTicks = 50,
            bool drawTicks = true,
            bool drawLabels = true)
        {
            Height = height;
            Padding = padding;
            MaxTicks = Mathf.Max(1, maxTicks);
            DrawTicks = drawTicks;
            DrawLabels = drawLabels;
        }
    }

    /// <summary>纯绘制：给一个 rect + 数据，画时间条、点、区间。</summary>
    public static class TimelineGUI
    {
        public static void Draw(
            Rect rect,
            float duration,
            IReadOnlyList<TimelinePoint> points,
            IReadOnlyList<TimelineSpan> spans,
            TimelineDrawOptions opt = default)
        {
            if (opt.Height <= 0) opt = new TimelineDrawOptions();
            duration = Mathf.Max(0.0001f, duration);

            // 背景
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.15f));

            var pad = opt.Padding;
            var bar = new Rect(rect.x + pad, rect.y + pad, rect.width - pad * 2, rect.height - pad * 2 - 12);

            // 主条
            var band = new Rect(bar.x, bar.center.y - 6, bar.width, 12);
            EditorGUI.DrawRect(band, new Color(1, 1, 1, 0.12f));

            // 刻度
            if (opt.DrawTicks)
            {
                var tickCount = Mathf.Clamp(Mathf.CeilToInt(duration), 1, opt.MaxTicks);
                for (var i = 0; i <= tickCount; i++)
                {
                    var time = duration * i / tickCount;
                    var x = X(time);
                    EditorGUI.DrawRect(new Rect(x, bar.y, 1, bar.height), new Color(1, 1, 1, 0.08f));
                }
            }

            // 区间（填充 + 边线）
            if (spans != null)
                for (var i = 0; i < spans.Count; i++)
                {
                    var s = spans[i];
                    var x1 = X(s.start);
                    var x2 = X(s.end);
                    if (x2 < x1) (x1, x2) = (x2, x1);
                    var startColor = s.startColor;
                    var endColor = s.endColor;
                    var durationColor = s.durationColor;

                    EditorGUI.DrawRect(new Rect(x1, bar.y, Mathf.Max(1, x2 - x1), bar.height), durationColor);

                    EditorGUI.DrawRect(new Rect(x1, bar.y, 2, bar.height), startColor);
                    EditorGUI.DrawRect(new Rect(x2, bar.y, 2, bar.height), endColor);

                    if (opt.DrawLabels && !string.IsNullOrEmpty(s.label))
                        GUI.Label(new Rect(x1 + 4, bar.y + 2, bar.width, bar.height - 6), s.label, EditorStyles.miniLabel);
                }

            // 点（竖线）
            if (points != null)
                for (var i = 0; i < points.Count; i++)
                {
                    var p = points[i];
                    var x = X(p.time);
                    var color = p.color;

                    EditorGUI.DrawRect(new Rect(x, bar.y, 2, bar.height), color);

                    if (opt.DrawLabels && !string.IsNullOrEmpty(p.label))
                        GUI.Label(new Rect(x + 4, bar.center.y - 18, bar.width, bar.height - 2), p.label, EditorStyles.miniLabel);
                }

            // 起止
            if (!opt.DrawLabels) return;
            GUI.Label(new Rect(bar.x, bar.yMax + 2, 80, 16), "0", EditorStyles.miniLabel);
            GUI.Label(new Rect(bar.xMax - 60, bar.yMax + 2, 80, 16), $"{duration:0.##}s", EditorStyles.miniLabel);
            return;

            float X(float t)
            {
                return bar.x + Mathf.Clamp01(t / duration) * bar.width;
            }
        }

        public static Rect ReserveRect(float height)
        {
            return GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
        }
    }
}
#endif