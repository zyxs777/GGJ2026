using UnityEngine;

namespace STool.MathUtility
{
    public static class GeometryUtility
    {
        public static class Circle
        {
            /// <summary>
            ///     求两圆交点
            ///     返回值：
            ///     0 个交点 -> return 0
            ///     1 个交点（相切）-> return 1，out p1 有效
            ///     2 个交点 -> return 2，out p1、p2 有效
            /// </summary>
            public static int Intersect(Vector2 c0, float r0, Vector2 c1, float r1,
                out Vector2 p1, out Vector2 p2)
            {
                p1 = p2 = default;

                var dVec = c1 - c0;
                var d = dVec.magnitude;

                // 重合或太近时处理（可按你需求特殊处理）
                if (d < 1e-6f)
                {
                    // 圆心几乎重合
                    return 0;
                }

                // 相离 / 内含（不相交）
                if (d > r0 + r1 || d < Mathf.Abs(r0 - r1)) return 0;

                // 计算 a 和 h
                var a = (r0 * r0 - r1 * r1 + d * d) / (2f * d);
                var hSq = r0 * r0 - a * a;

                // 浮点误差保护
                if (hSq < 0f) hSq = 0f;
                var h = Mathf.Sqrt(hSq);

                // 基点 P2：两圆连线上的点
                var dir = dVec / d;
                var p2Base = c0 + dir * a;

                // 垂直方向
                var perp = new Vector2(-dir.y, dir.x);

                // 相切（只有一个交点）
                if (h < 1e-6f)
                {
                    p1 = p2Base;
                    return 1;
                }

                // 两个交点
                p1 = p2Base + perp * h;
                p2 = p2Base - perp * h;
                return 2;
            }
        }
    }
}