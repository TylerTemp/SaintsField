using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DrawLineAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineFromAttribute), true)]
    [CustomPropertyDrawer(typeof(DrawLineToAttribute), true)]
    public class DrawLineAttributeDrawer: OneDirectionHandleBase
    {
        private static Texture2D _icon;

        protected override Texture2D GetIcon()
        {
            if (_icon is null)
            {
                return _icon = Util.LoadResource<Texture2D>("line.png");
            }

            return _icon;
        }

        protected override bool OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
            if (!base.OnSceneDraw(sceneView, oneDirectionInfo, worldPosStart, worldPosEnd))
            {
                return false;
            }

            using (new HandleColorScoop(oneDirectionInfo.Color))
            {
                if (oneDirectionInfo.OneDirectionAttribute.Dotted > 0f)
                {
                    Handles.DrawDottedLine(worldPosStart, worldPosEnd, oneDirectionInfo.OneDirectionAttribute.Dotted);
                }
                else
                {
                    Handles.DrawLine(worldPosStart, worldPosEnd);
                }
            }

            return true;
        }
    }
}
