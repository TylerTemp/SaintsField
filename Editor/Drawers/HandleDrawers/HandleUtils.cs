using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    public static class HandleUtils
    {
        public static (string error, Transform result) GetSpaceTransform(Util.TargetWorldPosInfo targetWorldPosInfo, string parentSpace, SerializedProperty serializedProperty, MemberInfo memberInfo, object parent)
        {
            if (targetWorldPosInfo.Transform != null)
            {
                return ("", targetWorldPosInfo.Transform);
            }

            Debug.Assert(parentSpace != null);

            Transform spaceTrans;

            Object spaceTarget;
            if (parentSpace == "this")
            {
                spaceTarget = serializedProperty.serializedObject.targetObject;
            }
            else
            {
                (string error, Object result) = Util.GetOf<Object>(parentSpace, null,
                    serializedProperty, memberInfo, parent);
                if (error != "")
                {
                    return (error, null);
                }
                spaceTarget = result;
            }

            switch (spaceTarget)
            {
                case GameObject go:
                    spaceTrans = go.transform;
                    break;
                case Component comp:
                    spaceTrans = comp.transform;
                    break;
                default:
                    return ($"Space {parentSpace} is not a GameObject or Component", null);
            }

            // wireDiscInfo.SpaceTransform = spaceTrans;
            return ("", spaceTrans);
        }

        public static Vector3 GetLocalToWorldScale(Transform transform)
        {
            Vector3 worldScale = transform.localScale;
            Transform parent = transform.parent;

            while (parent != null)
            {
                worldScale = Vector3.Scale(worldScale,parent.localScale);
                parent = parent.parent;
            }

            return worldScale;
        }
    }
}
