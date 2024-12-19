using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(DrawLineAttribute))]
    [CustomPropertyDrawer(typeof(DrawLineFromAttribute))]
    [CustomPropertyDrawer(typeof(DrawLineToAttribute))]
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
