using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawLabel
{
    [CustomPropertyDrawer(typeof(DrawLabelAttribute))]
    public partial class DrawLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private class LabelInfo
        {
            public Space Space;

            public string Content;
            public bool IsCallback;
            public string ActualContent;
            public EColor EColor;

            public Util.TargetWorldPosInfo TargetWorldPosInfo;

            public GUIStyle GUIStyle;
        }



        private static void OnSceneGUIInternal(SceneView _, LabelInfo labelInfo)
        {
            // ReSharper disable once ReplaceWithStringIsNullOrEmpty
            // ReSharper disable once MergeIntoLogicalPattern
            if (labelInfo.ActualContent == null || labelInfo.ActualContent == "")
            {
                return;
            }

            if (!string.IsNullOrEmpty(labelInfo.TargetWorldPosInfo.Error))
            {
                return;
            }

            if(labelInfo.GUIStyle == null)
            {
                if (labelInfo.EColor == EColor.White)
                {
                    labelInfo.GUIStyle = GUI.skin.label;
                }
                else
                {
                    labelInfo.GUIStyle = new GUIStyle
                    {
                        normal = { textColor = labelInfo.EColor.GetColor() },
                    };
                }
            }

            Vector3 pos = labelInfo.TargetWorldPosInfo.IsTransform
                ? labelInfo.TargetWorldPosInfo.Transform.position
                : labelInfo.TargetWorldPosInfo.WorldPos;
            Handles.Label(pos, labelInfo.ActualContent, labelInfo.GUIStyle);
        }

        ~DrawLabelAttributeDrawer()
        {
            // SceneView.duringSceneGui -= OnSceneGUIIMGUI;
#if UNITY_2021_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
#endif
        }
    }
}
