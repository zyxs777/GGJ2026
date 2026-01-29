using System.Collections.Generic;
using UnityEngine;

namespace STool
{
    public static class SelectTools
    {
        /// <summary>
        /// 基于屏幕空间做最近选择
        /// </summary>
        /// <param name="src">筛选数据</param>
        /// <param name="outData">idx为-1表示没有结果</param>
        /// <param name="mousePos">鼠标在屏幕位置</param>
        /// <param name="camera">屏幕相机</param>
        /// <param name="threshold">筛选阈值</param>
        public static void AbsoluteSelectOnScreen(this ICollection<Vector3> src, out SelectOutData outData,
            Vector2 mousePos, Camera camera, float threshold = float.MaxValue)
        {
            var dis = float.MaxValue;
            var idx = -1;
            var selectedIdx = -1;
            var selectedPos = Vector3.zero;
            var selectedUIPos = Vector2.zero;
            foreach (var variable in src)
            {
                var screenPos = RectTransformUtility.WorldToScreenPoint(camera, variable);
                var distance = Vector2.Distance(screenPos, mousePos);
                idx++;
                if (distance >= dis || distance > threshold) continue;
                dis = distance;
                selectedIdx = idx;
                selectedPos = variable;
                selectedUIPos = screenPos;
            }

            outData = new SelectOutData()
            {
                WorldPos = selectedPos,
                ScreenPos = selectedUIPos,
                IdxInArray = selectedIdx
            };
        }

        /// <summary>
        /// 基于UI上位置和方向
        /// </summary>
        /// <param name="src"></param>
        /// <param name="outData"></param>
        /// <param name="mousePos">屏幕位置</param>
        /// <param name="direction">筛选朝向</param>
        /// <param name="camera"></param>
        /// <param name="angleDotThreshold">筛选角度阈值</param>
        /// <param name="disWeight">筛选评分的距离项权重</param>
        /// <param name="ignoreNear">剔除离所选目标过近的选项，为负时不启用</param>
        public static void NearSwitchSelectOnUI(this ICollection<Vector3> src, out SelectOutData outData, 
            Vector2 mousePos, Vector2 direction, Camera camera, float angleDotThreshold = 0, float disWeight = 1, float ignoreNear = -1f)
        {
            if (direction.sqrMagnitude < .2f)
            {
                src.AbsoluteSelectOnScreen(out outData, mousePos, camera);
                return;    
            }
            
            //初始化
            var idx = -1;
            var selectedIdx = -1;
            var selectedPos = Vector3.zero;
            var selectedUIPos = Vector2.zero;
            var score = float.MinValue;
            var directionNormalized = direction.normalized;
            foreach (var variable in src)
            {
                //计算筛选评分
                var screenPos = RectTransformUtility.WorldToScreenPoint(camera, variable);
                var tarDirection = screenPos - mousePos;
                var tarDirNormalized = tarDirection.normalized;
                var tarSqrDis = tarDirection.magnitude;

                var angleDot = Vector2.Dot(directionNormalized, tarDirNormalized);
                var tarScore = angleDot + disWeight / tarSqrDis;
                
                idx++;
                // Debug.Log($"Scoring: {idx} => {tarScore:F5} with " +
                //           $"{screenPos}\t{mousePos}>{direction}\t{angleDot:F5} and {disWeight/tarSqrDis:F5}" +
                //           $"\n Is too near? {mousePos} {screenPos} {Vector2.Distance(mousePos, screenPos)} < {ignoreNear} is {(Vector2.Distance(mousePos, screenPos) < ignoreNear)}");
                //过近保护
                if (Vector2.Distance(mousePos, screenPos) < ignoreNear) continue;
                //判断是否选择
                if (angleDot < angleDotThreshold) continue;
                if (score > tarScore) continue;

                score = tarScore;
                selectedPos = variable;
                selectedUIPos = screenPos;
                selectedIdx = idx;
            }
            
            // if (selectedIdx == -1)
            // {
            //     src.AbsoluteSelectOnScreen(out outData, mousePos, camera);
            //     return;  
            // }            
            
            outData = new SelectOutData()
            {
                WorldPos = selectedPos,
                ScreenPos = selectedUIPos,
                IdxInArray = selectedIdx
            };
        }

        public struct SelectOutData
        {
            public Vector3 WorldPos;
            public Vector2 ScreenPos;
            public int IdxInArray;
        }
    }
}
