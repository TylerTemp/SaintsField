using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ArrowHandleCapAttribute), true)]
    public class ArrowHandleCapAttributeDrawer: OneDirectionHandleBase
    {
        protected override void OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
            using (new HandleColorScoop(oneDirectionInfo.Color))
            {
                // Handles.DrawLine(worldPosStart, worldPosEnd);
                float size = HandleUtility.GetHandleSize(worldPosEnd);
                float distance = (worldPosStart - worldPosEnd).magnitude;
                float useSize = Mathf.Min(size, distance);
                Vector3 arrowPos = Vector3.Lerp(worldPosEnd, worldPosStart, useSize / distance);
                Handles.ArrowHandleCap(
                    0,
                    arrowPos,
                    // transform.rotation * Quaternion.LookRotation(Vector3.right),
                    Quaternion.LookRotation(worldPosEnd - worldPosStart),
                    // (worldPosStart - worldPosEnd).magnitude,
                    useSize,
                    // HandleUtility.GetHandleSize(worldPosStart - worldPosEnd) * 2,
                    EventType.Repaint
                );

                if (distance > size)
                {
                    if (oneDirectionInfo.OneDirectionAttribute.Dotted > 0f)
                    {
                        Handles.DrawDottedLine(worldPosStart, worldPosEnd, oneDirectionInfo.OneDirectionAttribute.Dotted);
                    }
                    else
                    {
                        Handles.DrawLine(worldPosStart, worldPosEnd, 2f);
                    }
                }
            }
        }
    }
}
