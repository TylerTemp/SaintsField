using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DrawLineAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineFromAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineToAttribute), true)]
    public class DrawLineAttributeDrawer: OneDirectionHandleBase
    {
        protected override void OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
            using (new HandleColorScoop(oneDirectionInfo.OneDirectionConstInfo.OneDirectionAttribute.EColor.GetColor() * new Color(1, 1, 1, oneDirectionInfo.OneDirectionConstInfo.OneDirectionAttribute.ColorAlpha)))
            {
                Handles.DrawLine(worldPosStart, worldPosEnd);
            }
        }
    }
}
