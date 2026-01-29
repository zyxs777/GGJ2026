using UnityEditor;
using UnityEngine;

namespace STool.EditorExtension
{
    public static class SEditorUtility
    {
        /// <summary>
        /// 将世界坐标转换为 Scene 视图的屏幕坐标（用于 Handles、Editor 工具等）
        /// </summary>
        public static Vector2 WorldToSceneScreenPoint(Vector3 worldPos)
        {
            var sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null || sceneView.camera == null) return Vector2.zero;

            var cam = sceneView.camera;
            // Scene 视图屏幕坐标，以左上角为 (0,0) 向下增长
            var screenPos = cam.WorldToScreenPoint(worldPos);
            // Unity GUI 是左上角 (0,0)，但 SceneView 的屏幕坐标 Y 是从下往上// 所以要翻转 Y
            screenPos.y = sceneView.position.height - screenPos.y;

            return new Vector2(screenPos.x, screenPos.y);
        }
    }
}
