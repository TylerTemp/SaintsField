// using System.Reflection;
// using SaintsField.Editor.Utils;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// #if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
//
// #if UNITY_2021_3_OR_NEWER
//
// namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
// {
//     public partial class AddressableResourceAttributeDrawer
//     {
//         private static string ButtonName(SerializedProperty property) =>
//             $"{property.propertyPath}__AddressableResource_Button";
//         private static string HelpBoxName(SerializedProperty property) =>
//             $"{property.propertyPath}__AddressableResource_HelpBox";
//
//         private static string GroupDownName(SerializedProperty property) =>
//             $"{property.propertyPath}__AddressableResource_Group";
//         private static string LabelDownName(SerializedProperty property) =>
//             $"{property.propertyPath}__AddressableResource_Label";
//
//         protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
//             VisualElement container, FieldInfo info, object parent)
//         {
//             Button button = new Button
//             {
//                 style =
//                 {
//                     backgroundImage = Util.LoadResource<Texture2D>("folder.png"),
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(14, 14),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                     paddingLeft = 8,
//                     paddingRight = 8,
//                 },
//                 name = ButtonName(property),
//             };
//             return button;
//         }
//
//         protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
//             ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
//         {
//             VisualElement root = new VisualElement
//             {
//                 style =
//                 {
//                     backgroundColor = EColor.EditorEmphasized.GetColor(),
//                     paddingTop = 4,
//                     paddingBottom = 4,
//                     paddingLeft = 4,
//                     paddingRight = 8,
//                 },
//             };
//
//             VisualElement actionArea = new VisualElement();
//             root.Add(actionArea);
//
//             ObjectField objField = new ObjectField("Resource");
//             objField.AddToClassList(ClassAllowDisable);
//             objField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
//             actionArea.Add(objField);
//
//             UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit("Group");
//             dropdownButton.style.flexGrow = 1;
//             dropdownButton.name = GroupDownName(property);
//             dropdownButton.AddToClassList(ClassAllowDisable);
//             actionArea.Add(dropdownButton);
//             // dropdownButton.ButtonLabelElement.text = GetSelectedNames(metaInfo.BitValueToName, property.intValue);
//
//             UIToolkitUtils.DropdownButtonField dropdownLabel = UIToolkitUtils.MakeDropdownButtonUIToolkit("Label");
//             dropdownLabel.style.flexGrow = 1;
//             dropdownLabel.name = LabelDownName(property);
//             dropdownLabel.AddToClassList(ClassAllowDisable);
//             actionArea.Add(dropdownLabel);
//
//             VisualElement buttons = new VisualElement
//             {
//                 style =
//                 {
//                     flexDirection = FlexDirection.Row,
//                     flexGrow = 1,
//                     height = SingleLineHeight,
//                 },
//             };
//             buttons.Add(new Button
//             {
//                 style =
//                 {
//                     flexGrow = 1,
//                     backgroundImage = Util.LoadResource<Texture2D>("check.png"),
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                 },
//             });
//             buttons.Add(new Button
//             {
//                 style =
//                 {
//                     flexGrow = 1,
//                     backgroundImage = Util.LoadResource<Texture2D>("trash.png"),
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                 },
//             });
//             buttons.Add(new Button
//             {
//                 style =
//                 {
//                     flexGrow = 1,
//                     backgroundImage = Util.LoadResource<Texture2D>("close.png"),
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                 },
//             });
//             actionArea.Add(buttons);
//
//             HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
//             {
//                 style =
//                 {
//                     display = DisplayStyle.None,
//                 },
//                 name = HelpBoxName(property),
//             };
//
//             root.Add(helpBox);
//             return root;
//         }
//     }
// }
//
// #endif
//
// #endif
