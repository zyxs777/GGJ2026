using System.Linq;
using UnityEngine;

namespace STool.RewiredExtension
{
    public static class RewiredEditorUtility
    {
        public const string GetActionName = "@RewiredEditorUtility.ActionNames()";
#if UNITY_EDITOR

        public static string[] ActionNames()
        {
            var manager = Object.FindObjectOfType<Rewired.InputManager>();
            var list = manager.userData.GetActions_Copy();
            return list.Select(variable => variable.name).ToArray();
        }
        
#endif
    }
}