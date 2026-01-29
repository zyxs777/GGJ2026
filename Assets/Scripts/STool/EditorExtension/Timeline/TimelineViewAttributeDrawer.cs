#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using STool.EditorExtension.SEditorExtension.Attributes;
using UnityEngine;

namespace STool.EditorExtension.Timeline
{
    // 仅对 TimelineModel 生效；你也可以扩展成接口/泛型
    public class TimelineViewAttributeDrawer : OdinAttributeDrawer<TimelineViewAttribute, TimelineModel>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 先画字段本体（如果你想完全只读，可以注释掉这行）
            CallNextDrawer(label);

            var model = ValueEntry.SmartValue;
            if (model == null) return;
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            GUILayout.Space(6);

            var rect = TimelineGUI.ReserveRect(Attribute.Height);

            if (Event.current.type != EventType.Repaint) return;
            var opt = new TimelineDrawOptions(
                height: Attribute.Height,
                drawTicks: Attribute.DrawTicks,
                drawLabels: Attribute.DrawLabels
            );

            TimelineGUI.Draw(rect, model.duration, model.points, model.spans, opt);
        }
    }
}
#endif