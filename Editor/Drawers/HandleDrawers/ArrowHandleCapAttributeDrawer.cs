using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ArrowHandleCapAttribute), true)]
    public class ArrowHandleCapAttributeDrawer: OneDirectionHandleBase
    {
        protected override void OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
            using (new HandleColorScoop(oneDirectionInfo.Color))
            {
                // Handles.DrawLine(worldPosStart, worldPosEnd);
                Handles.ArrowHandleCap(
                    0,
                    worldPosStart,
                    // transform.rotation * Quaternion.LookRotation(Vector3.right),
                    Quaternion.LookRotation(worldPosEnd - worldPosStart),
                    (worldPosStart - worldPosEnd).magnitude,
                    // HandleUtility.GetHandleSize(worldPosStart - worldPosEnd) * 2,
                    EventType.Repaint
                );
            }
        }
    }
}
