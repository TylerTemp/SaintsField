#if SAINTSFIELD_SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE

using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsArrowAttribute), true)]
    public class SaintsArrowAttributeDrawer: OneDirectionHandleBase
    {
        private static Texture2D _icon;

        protected override Texture2D GetIcon()
        {
            if (_icon is null)
            {
                return _icon = EditorGUIUtility.IconContent("UpArrow").image as Texture2D;
            }

            return _icon;
        }

        protected override bool OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
            if (!base.OnSceneDraw(sceneView, oneDirectionInfo, worldPosStart, worldPosEnd))
            {
                return false;
            }

            float sqrMagnitude = (worldPosStart - worldPosEnd).sqrMagnitude;

            SaintsArrowAttribute saintsArrowAttribute =
                (SaintsArrowAttribute)oneDirectionInfo.OneDirectionAttribute;

            float headLength = saintsArrowAttribute.HeadLength;
            if(headLength * 2f * headLength * 2f > sqrMagnitude)
            {
                headLength = Mathf.Sqrt(sqrMagnitude) * 0.5f;
            }

            (Vector3 tail, Vector3 head, Vector3 arrowheadLeft, Vector3 arrowheadRight) = SaintsDraw.Arrow.GetPoints(
                worldPosStart,
                worldPosEnd,
                arrowHeadLength: headLength,
                arrowHeadAngle: saintsArrowAttribute.HeadAngle);

            using (new HandleColorScoop(oneDirectionInfo.Color))
            {
                DrawLine(head, tail, oneDirectionInfo.OneDirectionAttribute.Dotted);
                DrawLine(head, arrowheadLeft, oneDirectionInfo.OneDirectionAttribute.Dotted);
                DrawLine(head, arrowheadRight, oneDirectionInfo.OneDirectionAttribute.Dotted);
            }

            return true;
        }

        private static void DrawLine(Vector3 start, Vector3 end, float dotted)
        {
            if(dotted > 0f)
            {
                Handles.DrawDottedLine(start, end, dotted);
            }
            else
            {
                Handles.DrawLine(start, end);
            }
        }
    }
}
#endif
