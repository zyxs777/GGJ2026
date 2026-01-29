using STool.SBlackBoard;
using STool.SBlackBoard.Unity;
using STool.STag;
using UnityEngine;

namespace STool.SConditions.BlackboardAdapter
{
    public static class SBlackboardCondUtility
    {
        public static void EmbedBlackBoardValueSource(this SBlackboardDefinition sBlackboard, ICondition target)
        {
            #if UNITY_EDITOR
            target.Recursion(condition =>
            {
                if (condition is IEntryHolder ie) ie.ValueProviders = sBlackboard.GetDropdownItems();
            });
            #endif
        }

        /// <summary>
        /// Register new entry to blackboard SO. Match by tag (of the sample)
        /// </summary>
        /// <param name="sBlackboard"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static bool TryRegisterWithTags(this SBlackboardDefinition sBlackboard, ref Entry sample)
        {
            //Find target tags with entry tag
            var entries = sBlackboard.Entries;

            //Try Matching
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];

                //Do tag matching
                var tagMatch = true;
                foreach (var tag in sample.TagData)
                {
                    if (entry.TagData.Contains(tag)) continue;
                    tagMatch = false;
                    break;
                }
                if (!tagMatch) continue;
                
                //Do Value type matching
                if (sample.defaultValue == null || entry.defaultValue == null) continue;
                if (sample.defaultValue.GetType() != entry.defaultValue.GetType()) continue;

                //Match Success and return
                sample = entry.GetCopy();
                return true;
            }
            
            //if not found, then use this entry to insert one
            var newEntry = sample.GetCopy();
            entries.Add(newEntry);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(sBlackboard);
            #endif
            
            return true;
        }

        /// <summary>
        /// Register new entry to blackboard SO. Match by Guid
        /// </summary>
        /// <param name="sBlackboard"></param>
        /// <param name="sample"></param>
        public static void TryRegisterWithGuid(this SBlackboardDefinition sBlackboard, ref Entry sample)
        {
            //Find target tags with entry tag
            var entries = sBlackboard.Entries;

            // var matchSuccess = false;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!entry.Id.Equals(sample.Id)) continue;

                // matchSuccess = true;
                entry.Copy(sample);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(sBlackboard);
#endif
                return;
            }
            
            var newEntry = sample.GetCopy();
            entries.Add(newEntry);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(sBlackboard);
#endif
        }
        
        /// <summary>
        /// Register new entry to blackboard SO. Match by Guid
        /// </summary>
        /// <param name="sBlackboard"></param>
        /// <param name="sample"></param>
        public static void TryUnregisterWithGuid(this SBlackboardDefinition sBlackboard, ref Entry sample)
        {
            //Find target tags with entry tag
            var entries = sBlackboard.Entries;

            Debug.Log($"111");
            
            // var matchSuccess = false;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!entry.Id.Equals(sample.Id)) continue;

                // matchSuccess = true;
                entries.RemoveAt(i);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(sBlackboard);
#endif
                return;
            }
        }

        
        public static void TryFixWithGuid(this SBlackboardDefinition sBlackboard, ref Entry target)
        {
            if (target == null) return;
            var entries = sBlackboard.Entries;
            foreach (var entry in entries)
            {
                if (!entry.Id.Equals(target.Id)) continue;
                target.Copy(entry);
                break;
            }
        }
    }
}
