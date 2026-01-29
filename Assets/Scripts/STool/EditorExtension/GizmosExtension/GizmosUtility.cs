using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace STool.GizmosExtension
{
    public static class GizmoExtensions
    {
        /// <summary>
        /// 根据点列表绘制折线
        /// </summary>
        public static void DrawPolyline(List<Vector3> points)
        {
            if (points == null || points.Count < 2)
                return;

            for (var i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }
        public static void DrawCircle(Vector3 center, float radius, int segments = 64)
        {
            var angleStep = 360f / segments;
            var prevPoint = center + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;

            for (var i = 1; i <= segments; i++)
            {
                var angle = angleStep * i * Mathf.Deg2Rad;
                var nextPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
        public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, float f)
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
        public static void DrawBoxYRot(in Vector3 center, in Vector3 size, float rotationY)
        {
            var halfX = size.x * 0.5f;
            var halfZ = size.z * 0.5f;
            var angle = -rotationY * Mathf.Deg2Rad;
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);
            var y0 = center.y;
            var y1 = center.y + size.y;

            // 手动计算每个角的世界坐标（底部）
            var p0 = WorldCorner(-halfX, -halfZ, center, cos, sin, y0);
            var p1 = WorldCorner( halfX, -halfZ, center, cos, sin, y0);
            var p2 = WorldCorner( halfX,  halfZ, center, cos, sin, y0);
            var p3 = WorldCorner(-halfX,  halfZ, center, cos, sin, y0);

            var p0Top = new Vector3(p0.x, y1, p0.z);
            var p1Top = new Vector3(p1.x, y1, p1.z);
            var p2Top = new Vector3(p2.x, y1, p2.z);
            var p3Top = new Vector3(p3.x, y1, p3.z);

            // 绘制底面
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);

            // 绘制顶面
            Gizmos.DrawLine(p0Top, p1Top);
            Gizmos.DrawLine(p1Top, p2Top);
            Gizmos.DrawLine(p2Top, p3Top);
            Gizmos.DrawLine(p3Top, p0Top);

            // 绘制竖边
            Gizmos.DrawLine(p0, p0Top);
            Gizmos.DrawLine(p1, p1Top);
            Gizmos.DrawLine(p2, p2Top);
            Gizmos.DrawLine(p3, p3Top);
        }
        private static Vector3 WorldCorner(float localX, float localZ, Vector3 center, float cos, float sin, float y)
        {
            var x = localX * cos - localZ * sin;
            var z = localX * sin + localZ * cos;
            return new Vector3(center.x + x, y, center.z + z);
        }
        public static void DrawCylinder(in Vector3 center, float radius, float height, int segments = 12)
        {
            var angleStep = Mathf.PI * 2f / segments;
            var y0 = center.y - height * 0.5f;
            var y1 = center.y + height * 0.5f;

            var prevX = Mathf.Cos(0f) * radius;
            var prevZ = Mathf.Sin(0f) * radius;

            var prev0 = new Vector3(center.x + prevX, y0, center.z + prevZ);
            var prev1 = new Vector3(center.x + prevX, y1, center.z + prevZ);

            for (var i = 1; i <= segments; i++)
            {
                var angle = i * angleStep;
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
                var p0 = new Vector3(center.x + x, y0, center.z + z);
                var p1 = new Vector3(center.x + x, y1, center.z + z);

                Gizmos.DrawLine(prev0, p0);
                Gizmos.DrawLine(prev1, p1);
                Gizmos.DrawLine(prev0, prev1);

                prev0 = p0;
                prev1 = p1;
            }
        }
        
        
        /// <summary>
        /// 绘制一个以底部居中为 pivot 的立方体线框。
        /// </summary>
        /// <param name="pivot">底部中心点（世界坐标）</param>
        /// <param name="size">立方体的尺寸 (x=宽, y=高, z=深)</param>
        /// <param name="color">线框颜色</param>
        public static void DrawWireCubeBottomPivot(Vector3 pivot, Vector3 size)
        {

            // 计算顶点坐标
            var half = new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);

            // 底部四个点
            var bFL = pivot + new Vector3(-half.x, 0f, half.z); // Front-Left
            var bFR = pivot + new Vector3(half.x, 0f, half.z); // Front-Right
            var bBR = pivot + new Vector3(half.x, 0f, -half.z); // Back-Right
            var bBL = pivot + new Vector3(-half.x, 0f, -half.z); // Back-Left

            // 顶部四个点
            var tFL = bFL + Vector3.up * size.y;
            var tFR = bFR + Vector3.up * size.y;
            var tBR = bBR + Vector3.up * size.y;
            var tBL = bBL + Vector3.up * size.y;

            // 绘制底面
            Gizmos.DrawLine(bFL, bFR);
            Gizmos.DrawLine(bFR, bBR);
            Gizmos.DrawLine(bBR, bBL);
            Gizmos.DrawLine(bBL, bFL);

            // 绘制顶面
            Gizmos.DrawLine(tFL, tFR);
            Gizmos.DrawLine(tFR, tBR);
            Gizmos.DrawLine(tBR, tBL);
            Gizmos.DrawLine(tBL, tFL);

            // 绘制竖线
            Gizmos.DrawLine(bFL, tFL);
            Gizmos.DrawLine(bFR, tFR);
            Gizmos.DrawLine(bBR, tBR);
            Gizmos.DrawLine(bBL, tBL);
        }
        
        /// <summary>
        /// 绘制一个由三条线组成的箭头（主线 + 两条斜线）
        /// </summary>
        /// <param name="origin">箭头起点（世界坐标）</param>
        /// <param name="direction">箭头方向向量（决定长度与方向）</param>
        /// <param name="headLength">箭头头部长度（相对于箭头总长）</param>
        /// <param name="headAngle">箭头头部张角（度）</param>
        public static void DrawArrow(Vector3 origin, Vector3 direction,
            float headLength = 0.25f, float headAngle = 45)
        {
            if (direction == Vector3.zero)
                return;
            
            var end = origin + direction;
            Gizmos.DrawLine(origin, end);

            // 箭头长度与角度控制
            var len = direction.magnitude;
            var dirNorm = direction.normalized;

            // 计算箭头两侧的向量
            var right = Quaternion.LookRotation(dirNorm) * Quaternion.Euler(0, 180 + headAngle, 0) * Vector3.forward;
            var left = Quaternion.LookRotation(dirNorm) * Quaternion.Euler(0, 180 - headAngle, 0) * Vector3.forward;

            Gizmos.DrawLine(end, end + right * headLength * len);
            Gizmos.DrawLine(end, end + left  * headLength * len);
        }
        
        /// <summary>
        /// 绘制标尺线：主线 + 间隔刻度（垂直短线）
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="step">刻度间隔（默认1）</param>
        /// <param name="tickSize">刻度短线长度（默认0.1）</param>
        /// <param name="widerTick">每x刻度双倍长</param>
        public static void DrawLineRuler(Vector3 start, Vector3 end, float step = 1f, float tickSize = 1f, int widerTick = 5)
        {
            if (step <= 0f) step = 1f;

            // 主线
            Gizmos.DrawLine(start, end);

            var dir = end - start;
            var totalLength = dir.magnitude;
            if (totalLength < Mathf.Epsilon) return;

            var dirNorm = dir.normalized;

            // 垂直方向（尽量避开与 up 共线）
            Vector3 perp;
            if (Mathf.Abs(Vector3.Dot(dirNorm, Vector3.up)) > 0.99f)
                perp = Vector3.Cross(dirNorm, Vector3.right);
            else
                perp = Vector3.Cross(dirNorm, Vector3.up);
            perp.Normalize();

            // 刻度
            var tickCount = Mathf.FloorToInt(totalLength / step);
            for (var i = 1; i < tickCount; i++)
            {
                var wider = i % widerTick == 0;
                var p = start + dirNorm * (i * step);
                var a = p - perp * (tickSize * 0.5f * (wider ? 2 : 1));
                var b = p + perp * (tickSize * 0.5f * (wider ? 2 : 1));
                Gizmos.DrawLine(a, b);
            }

            // —— 在中点附近显示长度数字 —— //
            var mid = (start + end) * 0.5f;
            // 往垂直方向稍微偏一点，避免压住主线
            var labelPos = mid + perp * (tickSize * 1.2f);

            // 长度显示（你也可以换成 Mathf.Round(totalLength) 或自定义格式）
            var text = totalLength.ToString("0.###");

            // 简单样式（可按需自定义）
            var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

            Handles.Label(labelPos, text, style);
        }

        /// <summary>
        /// 以此绘制指向性线条
        /// </summary>
        /// <param name="points"></param>
        /// <param name="headLength"></param>
        /// <param name="headAngle"></param>
        public static void DrawSequentialArrows(IReadOnlyList<Vector3> points, float headLength = 1f, float headAngle = 45)
        {
            if (points.Count < 2) return;
            for (var i = 1; i < points.Count; i++)
            {
                var origin = points[i - 1];
                var end = points[i];
                Gizmos.DrawLine(origin, end);

                // 箭头长度与角度控制
                var dir = end - origin;
                var dirNorm = dir.normalized;

                // 计算箭头两侧的向量
                var right = Quaternion.LookRotation(dirNorm) * Quaternion.Euler(0, 180 + headAngle, 0) * Vector3.forward;
                var left = Quaternion.LookRotation(dirNorm) * Quaternion.Euler(0, 180 - headAngle, 0) * Vector3.forward;

                Gizmos.DrawLine(end, end + right * headLength);
                Gizmos.DrawLine(end, end + left  * headLength);
            }
        }
    }

}
