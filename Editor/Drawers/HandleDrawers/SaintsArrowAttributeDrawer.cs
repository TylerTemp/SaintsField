#if SAINTSFIELD_SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE

using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsArrowAttribute), true)]
    public class SaintsArrowAttributeDrawer: OneDirectionHandleBase
    {
        protected override void OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
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
                Handles.DrawLine(head, tail);
                Handles.DrawLine(head, arrowheadLeft);
                Handles.DrawLine(head, arrowheadRight);
            }
        }
    }
}
#endif
