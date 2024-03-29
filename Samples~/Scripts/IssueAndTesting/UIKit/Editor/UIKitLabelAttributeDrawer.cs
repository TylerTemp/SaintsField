#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using SaintsField.Samples.Scripts.UIKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Samples.Scripts.IssueAndTesting.UIKit.Editor
{
    [CustomPropertyDrawer(typeof(UIKitLabelAttribute))]
    public class UIKitLabelAttributeDrawer: PropertyDrawer
    {

        private Texture2D _texture;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement aboveContainer = new VisualElement();

            aboveContainer.style.flexDirection = FlexDirection.Row;
            // aboveContainer.style.flexWrap = Wrap.NoWrap;

            var oneHelpBox =
                new HelpBox(
                    "Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips Some Tips ",
                    HelpBoxMessageType.Info);
            // oneHelpBox.style.width = 50;
            // oneHelpBox.style.flexGrow = 1;
            // oneHelpBox.style.width = Length.Percent(50);
            aboveContainer.Add(oneHelpBox);
            var anotherHelpBox = new HelpBox("Another top", HelpBoxMessageType.Info);
            // anotherHelpBox.style.width = 50;
            // anotherHelpBox.style.flexGrow = 1;
            // anotherHelpBox.style.width = Length.Percent(50);
            aboveContainer.Add(anotherHelpBox);

            VisualElement fieldContainer = new VisualElement();
            aboveContainer.Add(fieldContainer);

            var prop = new PropertyField(property, " ");
            // prop.style.


            VisualElement overlayLabelContainer = new VisualElement();

            var label = new Label("Text<space=10>");
            label.style.flexShrink = 0;
            overlayLabelContainer.Add(label);

            _texture = Util.LoadResource<Texture2D>("eye.png");
            var img = new Image()
            {
                image = _texture,
                scaleMode = ScaleMode.ScaleToFit,
                tintColor = Color.red,
            };
            img.style.width = img.style.height = EditorGUIUtility.singleLineHeight - 2;
            img.style.flexShrink = 0;
            overlayLabelContainer.Add(img);

            // Debug.Log(EditorGUIUtility.labelWidth);

            var label2 = new Label("g property property property ");
            label2.style.flexShrink = 0;
            overlayLabelContainer.Add(label2);

            overlayLabelContainer.style.position = Position.Absolute;
            overlayLabelContainer.style.left = 3;
            overlayLabelContainer.style.top = 0;
            overlayLabelContainer.style.height = EditorGUIUtility.singleLineHeight;
            overlayLabelContainer.style.flexDirection = FlexDirection.Row;
            overlayLabelContainer.style.flexWrap = Wrap.NoWrap;
            overlayLabelContainer.style.alignItems = Align.Center;  // vertical
            // overlayLabelContainer.style.justifyContent = Justify.FlexStart;  // horizontal
            overlayLabelContainer.style.overflow = Overflow.Hidden;
            overlayLabelContainer.pickingMode = PickingMode.Ignore;

            fieldContainer.Add(overlayLabelContainer);
            fieldContainer.Add(prop);


            // GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            // {
            //     richText = true,
            // };
            // Debug.Log(textStyle.CalcSize(new GUIContent("property<space=10>.displayName")).x);
            // container.Add(label);

            VisualElement root = new VisualElement();
            root.Add(aboveContainer);
            root.Add(fieldContainer);

            return root;
        }
    }
}
#endif
