#if SAINTSFIELD_SAINTSDRAW || SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE

using SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(SaintsArrowAttribute))]
    public class SaintsArrowAttributeDrawer: OneDirectionHandleBase
    {
        protected override void OnSceneDraw(SceneView sceneView, OneDirectionInfo oneDirectionInfo, Vector3 worldPosStart, Vector3 worldPosEnd)
        {
            float sqrMagnitude = (worldPosStart - worldPosEnd).sqrMagnitude;

            SaintsArrowAttribute saintsArrowAttribute =
                (SaintsArrowAttribute)oneDirectionInfo.OneDirectionConstInfo.OneDirectionAttribute;

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

            using (new HandleColorScoop(oneDirectionInfo.OneDirectionConstInfo.OneDirectionAttribute.EColor.GetColor() * new Color(1, 1, 1, oneDirectionInfo.OneDirectionConstInfo.OneDirectionAttribute.ColorAlpha)))
            {
                Handles.DrawLine(head, tail);
                Handles.DrawLine(head, arrowheadLeft);
                Handles.DrawLine(head, arrowheadRight);
            }
        }
    }
}

#else
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    [CustomPropertyDrawer(typeof(SaintsArrowAttribute))]
    public class SaintsArrowAttributeDrawer : SaintsPropertyDrawer
    {
        private const string Url = "https://github.com/TylerTemp/SaintsDraw";
        private const string ErrorMessage = "Requires SaintsDraw (>= 1.0.4): " + Url;

        #region IMGUI
        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.GetHeight(ErrorMessage, width, MessageType.Error) + SingleLineHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            Rect leftRect = ImGuiHelpBox.Draw(position, ErrorMessage, MessageType.Error);
            (Rect buttonRect, Rect emptyRect) = RectUtils.SplitHeightRect(leftRect, SingleLineHeight);
            if (GUI.Button(buttonRect, "Open"))
            {
                Application.OpenURL(Url);
            }
            return emptyRect;
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit


        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            root.Add(new HelpBox
            {
                text = ErrorMessage,
                messageType = HelpBoxMessageType.Error,
                style =
                {
                    flexGrow = 1,
                },
            });
            root.Add(new Button(() => Application.OpenURL(Url))
            {
                text = "Open",
            });

            return root;
        }
        #endregion

#endif
    }
}
#endif
