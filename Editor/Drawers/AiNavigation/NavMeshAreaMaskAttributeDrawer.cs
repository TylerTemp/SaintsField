#if SAINTSFIELD_AI_NAVIGATION
using SaintsField.Editor.Core;
using UnityEditor;
using SaintsField.AiNavigation;

#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif  // UNITY_2021_3_OR_NEWER

#endif  // SAINTSFIELD_AI_NAVIGATION

namespace SaintsField.Editor.Drawers.AiNavigation
{
#if SAINTSFIELD_AI_NAVIGATION
    [CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute))]
    public class NavMeshAreaMaskAttributeDrawer: SaintsPropertyDrawer
    {

    }
#endif  // SAINTSFIELD_AI_NAVIGATION
}
