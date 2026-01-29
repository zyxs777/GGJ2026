using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace STool
{
    #region 计数器
    [Serializable] public class ConsumeCounter
    {
        [ShowInInspector, ReadOnly] private int _remain;
        public int refreshTo = 1;
        public void Refresh() { _remain = refreshTo; }
        public void Refresh(int count) { _remain = count; }
        public void SetRefreshTo(int count) { refreshTo = count; }
        public bool CanUse() => _remain > 0;
        /// <summary>
        /// 剩余使用机制，够用则返回true
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Use(int count = 1)
        {
            if (_remain < count) return false;
            _remain -= count;
            return true;
        }
        public float GetPercent() { return 1f - Mathf.Clamp01(1f * _remain / refreshTo) ; }
        public float GetValue() => _remain;
    }
    [Serializable] public class ConsumeCounterFloat
    {
        [ShowInInspector, ReadOnly] private float _remain;
        public float refreshTo = 1;
        public void Refresh() { _remain = refreshTo; }
        public void Refresh(float count) { _remain = count; }
        public void SetRefreshTo(float count) { refreshTo = count; }
        /// <summary>
        /// 累计使用机制，本次累计后达标，则可使用
        /// </summary>
        /// <param name="count">累计值</param>
        /// <returns></returns>
        public bool Use(float count)
        {
            if (_remain <= 0) return true;
            _remain -= count;
            return _remain <= 0;
        }
        public bool CanUse(){return _remain <= 0;}
        public float GetPercent()
        {
            return 1f - Mathf.Clamp01(_remain / refreshTo) ;
        }

        public float GetValue() => _remain;
    }
    [Serializable] public class LoopCounter
    {
        [ShowInInspector, ReadOnly, HorizontalGroup("0"), HideLabel] private int _curCount;
        [HorizontalGroup("0"), HideLabel] public int loopCount = 1;
        public void Reset() { _curCount = 0; }
        public int GetCurrent() => _curCount;
        public void DoCount(int count) { _curCount = (_curCount + count + loopCount) % loopCount; }
    }
    [Serializable] public class ConsumeCounterFloat_RefreshAdd
    {
        [ShowInInspector, ReadOnly] private float _remain;
        public float refreshTo = 1;
        public void Refresh() { _remain = refreshTo; }
        public void Refresh(float count) { _remain += count; }
        public void SetRefreshTo(float count) { refreshTo = count; }
        /// <summary>
        /// 无上限累计使用机制，会不断累计。每次刷新时从总累计值中扣除
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Use(float count)
        {
            var result = _remain <= 0;
            _remain -= count;
            return result;
        }
        public bool CanUse(){return _remain <= 0;}
        public float GetPercent()
        {
            return 1f - Mathf.Clamp01(_remain / refreshTo) ;
        }
    }

    [Serializable]
    public class RangeCounterFloat
    {
        public Vector2 limit;
        [ShowInInspector] private float _value;
        public float Value
        {
            get => _value;
            set => _value = Mathf.Clamp(value, limit.x, limit.y);
        }
        
        /// <summary>
        /// 扣除使用机制，本次够用则返回true
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool UseCheck(float count)
        {
            return Value >= count;
        }
        
        /// <summary>
        /// 扣除使用机制，本次够用则返回true
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Use(float count)
        {
            if (Value < count) return false;
            Value -= count;
            return true;
        }
        public float GetPercent()
        {
            return (Value - limit.x) / (limit.y - limit.x);
        }
        public void Fill(float percentage = 1)
        {
            Value = limit.x + (limit.y - limit.x) * percentage;
        }
        public void FillAbs(float value)
        {
            Value = value;
        }
    }
    #endregion
    
    #region 扩展计算
    public static class Vector2Extensions
    {
        /// <summary>
        /// 将 Vector2 绕原点顺时针旋转指定角度（单位：度）。
        /// </summary>
        /// <param name="v">原始向量</param>
        /// <param name="angleDegrees">旋转角度，单位为度。正值为顺时针</param>
        /// <returns>旋转后的新向量</returns>
        public static Vector2 Rotate(this Vector2 v, float angleDegrees)
        {
            var rad = -angleDegrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }
        /// <summary>
        /// 获取从 (1, 0) 顺时针旋转到 dir 向量所对应的角度，范围 [0, 360) 。
        /// </summary>
        public static float ClockAngle(this Vector2 v)
        {
            if (v == Vector2.zero) return 0;
            var rad = System.MathF.Atan2(v.y, v.x);
            var deg = rad * UnityEngine.Mathf.Rad2Deg;
            deg = -deg;
            if (deg < 0) deg += 360f;
            return deg;
        }
        /// <summary>
        /// 计算一个点到线段的最近距离
        /// </summary>
        /// <param name="point">目标点</param>
        /// <param name="lineStart">线段起点</param>
        /// <param name="lineEnd">线段终点</param>
        /// <returns>点到线段的最短距离</returns>
        public static float DistanceToSegment(this Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            var line = lineEnd - lineStart;
            var toPoint = point - lineStart;

            var lineLengthSqr = line.sqrMagnitude;
            if (lineLengthSqr == 0f) return Vector2.Distance(point, lineStart); // 退化为点

            var t = Mathf.Clamp01(Vector2.Dot(toPoint, line) / lineLengthSqr);
            var projection = lineStart + t * line;

            return Vector2.Distance(point, projection);
        }
        public static Vector2 ConvertXZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
        /// <summary>
        /// 判断点在边的哪一侧
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static float Sign(this Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - 
                   (p2.x - p3.x) * (p1.y - p3.y);
        }

        public static Vector3 ExtendZ(this Vector2 v, float z) => new Vector3(v.x, v.y, z);
        public static Vector3 ExtendY(this Vector2 v, float y) => new Vector3(v.x, y, v.y);

        public static bool InRange(this Vector2 v, float value) => value >= v.x && value <= v.y;
    }
    public static class Vector3Extensions
    {
        public static Vector3 MulChannels(this Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
        public static Vector3 ConvertXZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }
        public static Vector3 SetXZ(this Vector3 v, Vector2 vector2)
        {
            return new Vector3(vector2.x, v.y, vector2.y);
        }
        public static Vector3 SetY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }
        /// <summary>
        /// 计算一个Vector3绕指定轴旋转指定角度后所得到的向量。
        /// </summary>
        /// <param name="source">旋转前的源Vector3</param>
        /// <param name="axis">旋转轴</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>旋转后得到的新Vector3</returns>
        public static Vector3 Rotate(this Vector3 source, Vector3 axis, float angle)
        {
            var q = Quaternion.AngleAxis(angle, axis);// 旋转系数
            return q * source;// 返回目标点
        }
        /// <summary>
        /// 将Vec3扩展W位为Vec4
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Vector4 ExtendW(this Vector3 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, w);
        }
    }
    public static class ColorExtensions
    {
        public static Color SetAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    public static class Geometry3DUtility
    {
        /// <summary>
        /// 判断两个球是否相交（包含相切）
        /// </summary>
        public static bool SphereIntersectsSphere(Vector3 centerA, float radiusA, Vector3 centerB, float radiusB)
        {
            var rSum = radiusA + radiusB;
            var sqrDistance = (centerA - centerB).sqrMagnitude;
            return sqrDistance <= rSum * rSum;
        }
        /// <summary>
        /// 判断线段AB是否与球体（center，radius）相交（包含相切）
        /// </summary>
        public static bool LineSegmentIntersectsSphere(Vector3 a, Vector3 b, Vector3 sphereCenter, float radius)
        {
            var ab = b - a;
            var ac = sphereCenter - a;
            var abLengthSqr = ab.sqrMagnitude;
            if (abLengthSqr < Mathf.Epsilon)
            {
                // A和B几乎重合，退化为点对球判断
                return (sphereCenter - a).sqrMagnitude <= radius * radius;
            }
            // 向量投影：t 为 ac 在 ab 上的投影比例（非归一化）
            var t = Vector3.Dot(ac, ab) / abLengthSqr;
            // Clamp 到线段 [0,1] 区间
            t = Mathf.Clamp01(t);
            // 最近点 = a + ab * t
            var closest = a + ab * t;
            var sqrDist = (closest - sphereCenter).sqrMagnitude;
            return sqrDist <= radius * radius;
        }
        public static bool InHeight(float height, float minHeight, float maxHeight)
        {
            return height >= minHeight && height <= maxHeight;
        }
        public static bool InSphere(Vector3 position, Vector3 center, float radius)
        {
            return (position - center).sqrMagnitude <= radius * radius;
        }
        public static bool InAngle(Vector3 direction, Vector3 aiming, float angle)
        {
            return Vector3.Angle(direction, aiming) < angle;
        }
    }

    #endregion
    
    #region Unity扩展工具
    public static class UIUtility
    {
        /// <summary>
        /// 将世界坐标转换为 Canvas 上某个 RectTransform 下的局部坐标
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="camera">用于渲染该 Canvas 的摄像机</param>
        /// <param name="canvas">目标 Canvas</param>
        /// <param name="parent">Canvas 下的目标父 RectTransform</param>
        /// <returns>父节点下的本地坐标</returns>
        public static Vector2 WorldToUILocalPosition(Vector3 worldPos, Camera camera, Canvas canvas, RectTransform parent)
        {
            var screenPos = RectTransformUtility.WorldToScreenPoint(camera, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera,
                out var localPos);

            return localPos;
        }
        
    }
    public static class TransformExtensions
    {
        public static void SelectChild(this Transform src, Func<Transform, bool> func,
            Action<Transform> action)
        {
            for (var i = 0; i < src.childCount; i++)
                src.GetChild(i).SelectChild(func, action);
            if (!func(src)) return;
            action(src);
        }
    }
    
    [Serializable] public sealed class TagFilter
    {
        [ValueDropdown("@UnityEditorInternal.InternalEditorUtility.tags")] [SerializeReference]
        private string[] tags;

        public bool HasTag(string tag)
        {
            for (var index = tags.Length - 1; index >= 0; index--)
            {
                var t = tags[index];
                if (t.Equals(tag)) return true;
            }
            return false;
        }
        
        public TagFilter (string[] tags){
            this.tags = tags;
        }
    }
    #endregion

    #region 容器工具
    public static class ListExtensions
    {
        /// <summary>
        /// 将 source 中满足 predicate 的元素添加到 target 中（不会清空 target）
        /// </summary>
        public static void AddWhere<T>(this List<T> target, List<T> source, Predicate<T> predicate)
        {
            foreach (var item in source)
                if (predicate(item)) target.Add(item);
        }
        /// <summary>
        /// 对目标List进行原地插入排序 List-int (a,b)=>(a-b)为降序
        /// </summary>
        public static void InsertionSort<T>(this List<T> list, Func<T,T,int> comparer)
        {
            var count = list.Count;
            for (var i = 1; i < count; i++)
            {
                var current = list[i];
                var j = i - 1;
                while (j >= 0 && comparer(current, list[j]) > 0)
                {
                    list[j + 1] = list[j]; // 后移
                    j--;
                }
                list[j + 1] = current;
            }
        }
        public static void InsertSortedFrom<T>(this List<T> target, ICollection<T> source, Comparer<T> comparer)
        {
            foreach (var item in source)
            {
                var index = target.BinarySearch(item, comparer);
                if (index < 0)
                    index = ~index;
                target.Insert(index, item);
            }
        }
        /// <summary>
        /// 将单个元素插入到指定的有序List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="T"></typeparam>
        public static void InsertSorted<T>(this List<T> list, T item, Func<T, T, int> comparer)
        {
            var low = 0;
            var high = list.Count - 1;

            while (low <= high)
            {
                var mid = (low + high) >> 1;
                var cmp = comparer(item, list[mid]);
                if (cmp > 0) high = mid - 1;
                else low = mid + 1;
            }
            list.Insert(low, item);
        }

        public static bool RangeValid<T>(this List<T> container, int idx)
        {
            return idx >= 0 && idx < container.Count;
        }
    }
    public static class ArrayTools
    {
        /// <summary>
        /// 创建并填充一个数组，每个元素通过构造函数生成
        /// </summary>
        public static T[] CreateFilled<T>(int count, Func<T> generator)
        {
            var arr = new T[count];
            for (var i = 0; i < count; i++) arr[i] = generator();
            return arr;
        }
    }
    
    #endregion
    
    
    public static class FastMath
    {
        /// <summary>
        /// return value clamp to [min,max]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
    public static class InterfaceFinder
    {
        /// <summary>
        /// 查找场景中所有实现了接口 T 的组件
        /// </summary>
        public static void FindObjectsOfInterface<T>(ref List<T> results) where T : class
        {
            // 遍历场景中所有 MonoBehaviour
            foreach (var mono in Object.FindObjectsOfType<MonoBehaviour>(true)) // true = 包含 inactive
            {
                if (mono is T t) // 判断是否实现接口
                {
                    results.Add(t);
                }
            }
        }
    }
#if UNITY_EDITOR
    public static class EditorExtensions
    {
        public static bool IsMouseLeftDown(){return Event.current.type == UnityEngine.EventType.MouseDown && Event.current.button == 0;}
        public static bool IsMouseRightDown(){return Event.current.type == UnityEngine.EventType.MouseDown && Event.current.button == 1;}
        public static bool IsMouseLeftUp(){return Event.current.type == UnityEngine.EventType.MouseUp && Event.current.button == 0;}
        public static bool IsMouseRightUp(){return Event.current.type == UnityEngine.EventType.MouseUp && Event.current.button == 1;}
        public static Vector3 GetMouseWorldPoint(float maxDistance)
        {
            // 获取鼠标射线
            var ray = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // 如果射线命中场景物体，返回碰撞点
            return Physics.Raycast(ray, out var hitInfo, maxDistance) ? hitInfo.point :
                // 否则，返回射线最大距离处的点
                ray.GetPoint(maxDistance);
        } 
    }
    public static class MAssetTool
    {
        public static List<T> FindAllAssetsOfType<T>() where T : ScriptableObject
        {
            var results = new List<T>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                T so = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (so != null)
                    results.Add(so);
            }
            return results;
        }

    }
#endif
}
