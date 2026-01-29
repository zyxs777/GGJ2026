using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.STag
{
    [DisallowMultipleComponent]
    public class STagMono : SerializedMonoBehaviour
    {
        #if UNITY_EDITOR
        private List<ValueDropdownItem> GetDropdownItems()
        {
            var list = new List<ValueDropdownItem>();
            var dic = DropdownRegistry.GetAllDropDowns();
            foreach (var (groupName, value) in dic)
            {
                if (value == null || value.Count == 0) continue;
                if (value[0].value is not STagData) continue;

                foreach (var d in value)
                {
                    if (d.value is not STagData sd) continue;
                    list.Add(new ValueDropdownItem($"{groupName}/{d.label}",sd));
                }
            }
            return list;
        }
        [ValueDropdown("GetDropdownItems")]
        #endif
        [SerializeField] private HashSet<STagData> tags = new();
    }
}
