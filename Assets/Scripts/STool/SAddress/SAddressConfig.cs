using System.Collections.Generic;
using Sirenix.OdinInspector;
using STool.EditorExtension.SEditorExtension.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using STool.EditorExtension.Timeline;
#endif

namespace STool.SAddress
{
    [CreateAssetMenu(menuName = "STool/SAddress/SAddress Config")]
    public class SAddressConfig : ScriptableObject
    {
        #if UNITY_EDITOR
        private void OnValidate()
        {
            TryValidate();
            ReCalculateView();
        }

        #region Validation & Verify

        private void TryValidate()
        {
            var changed = false;
            
            //Avoid overlay
            for (var i = 0; i < segments.Count - 1; i++)
            {
                var segment = segments[i];
                var nextSegment = segments[i + 1];
                var nextStart = segment.offset + segment.width;

                if (nextSegment.offset >= nextStart) continue;

                changed = true;
                nextSegment.offset = nextStart;
            }
            
            if (!changed) return;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endregion
        
        #region lineSegmentView
        [HideReferenceObjectPicker] [ShowInInspector] [TimelineView]
        private TimelineModel _timelineModel = new() { duration = 64, showContainers = false };
        private void ReCalculateView()
        {
            _timelineModel.duration = 64;
            _timelineModel.showContainers = false;
            _timelineModel.points.Clear();
            _timelineModel.spans.Clear();

            foreach (var segment in segments)
            {
                _timelineModel.spans.Add(new TimelineSpan(start: 64 - segment.offset,
                    end: 64 - segment.offset - segment.width));
            }
        }
        #endregion

        #region ValueDropDown Provider
        private readonly List<ValueDropdownItem> _dropdownItems = new();
        public List<ValueDropdownItem> SegmentsProvider()
        {
            _dropdownItems.Clear();
            foreach (var segment in segments)
            {
                _dropdownItems.Add(new ValueDropdownItem() { Text = segment.name, Value = segment });
            }
            return _dropdownItems;
        }
        #endregion
        
        #endif

        [OnCollectionChanged("ReCalculateView")]
        public List<SAddrSegment> segments = new();
    }
}
