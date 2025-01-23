using System.Reflection;
using SaintsField.Editor.Utils;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine
{
    public static class SpineUtils
    {
        public static (string error, SkeletonDataAsset skeletonDataAsset) GetSkeletonDataAsset(string callback, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(callback))  // find on target
            {
                Object obj = property.serializedObject.targetObject;
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (obj)
                {
                    case Component comp:
                        return SkeletonDataOrNull(comp.GetComponent<SkeletonRenderer>());
                    case GameObject go:
                        return SkeletonDataOrNull(go.GetComponent<SkeletonRenderer>());
                    default:
                        return ($"{obj} is not a valid target", null);
                }
            }

            (string error, Object uObj) = Util.GetOf<Object>(callback, null, property, info, parent);
            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return (error, null);
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (uObj)
            {
                case SkeletonDataAsset skeletonDataAsset:
                    return ("", skeletonDataAsset);
                case SkeletonRenderer skeletonRenderer:
                    return ("", skeletonRenderer.skeletonDataAsset);
                case Component comp:
                    return SkeletonDataOrNull(comp.GetComponent<SkeletonRenderer>());
                case GameObject go:
                    return SkeletonDataOrNull(go.GetComponent<SkeletonRenderer>());
                default:
                    return ($"Target `{callback}` is not a valid target: {uObj} ({uObj.GetType()})", null);
            }
        }

        private static (string error, SkeletonDataAsset skeletonDataAsset) SkeletonDataOrNull(
            SkeletonRenderer skeletonRenderer)
        {
            return skeletonRenderer == null
                ? ($"SkeletonRenderer not found", null)
                : ("", skeletonRenderer.skeletonDataAsset);
        }
    }
}
