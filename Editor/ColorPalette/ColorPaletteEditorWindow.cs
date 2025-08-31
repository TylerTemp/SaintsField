#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.ColorPalette.UIToolkit;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette
{
    public class ColorPaletteEditorWindow: SaintsEditorWindow
    {

// #if SAINTSFIELD_DEBUG
//         [MenuItem("Saints/Color Palette...")]
// #else
//         [MenuItem("Window/Saints/Color Palette...")]
// #endif
        public static ColorPaletteEditorWindow OpenColorPaletteEditorWindow()
        {
            ColorPaletteEditorWindow window = GetWindow<ColorPaletteEditorWindow>(false, "Color Palette");
            // window.Show();
            return window;
        }

// #if SAINTSFIELD_DEBUG
//         [InitializeOnLoadMethod]
//         private static void ReOpen()
//         {
//             ColorPaletteEditorWindow window = GetWindow<ColorPaletteEditorWindow>(false, "Color Palette");
//             window.Close();
//             OpenColorPaletteEditorWindow();
//         }
// #endif

        [GetScriptableObject(EXP.NoAutoResignToNull), NoLabel, OnValueChanged(nameof(ColorPaletteArrayChanged))]
        public ColorPaletteArray colorPaletteArray;

        private SerializedObject _so;

        [Button("Add")]
        [PlayaBelowSeparator(5), PlayaBelowSeparator(EColor.Gray), PlayaBelowSeparator(5)]
        private void AddNew()
        {
            SerializedProperty prop = _so?.FindProperty(nameof(ColorPaletteArray.colorInfoArray));
            if (prop == null)
            {
                return;
            }

            int index = prop.arraySize;
            prop.arraySize += 1;
            SerializedProperty eleProp = prop.GetArrayElementAtIndex(index);
            eleProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.color)).colorValue = Color.black;
            eleProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels)).arraySize = 0;
            prop.serializedObject.ApplyModifiedProperties();
        }

        public void ColorPaletteArrayChanged(ColorPaletteArray cpa)
        {
            _so?.ApplyModifiedProperties();
            _so?.Dispose();
            _so = new SerializedObject(cpa);
            EditorRelinkRootUIToolkit();
        }

        public override void OnEditorDestroy()
        {
            _so?.ApplyModifiedProperties();
            _so?.Dispose();
        }

        public override void OnEditorEnable()
        {
            if (colorPaletteArray)
            {
                ColorPaletteArrayChanged(colorPaletteArray);
            }
        }

        // private readonly List<ColorPaletteLabels> _colorPaletteLabels = new List<ColorPaletteLabels>();

        protected override void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            ScrollView rootScoller = EditorCreatInspectingTarget();
            rootScoller.style.position = Position.Relative;
            root.Add(rootScoller);

            if (_so == null)
            {
                return;
            }

            // VisualTreeAsset containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/ColorPalette/Container.uxml");

            // VisualElement containerLayout = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = FlexDirection.Row,
            //         flexWrap = Wrap.Wrap,
            //     },
            // };
            // rootScoller.Add(containerLayout);

            // StyleSheet style = Util.LoadResource<StyleSheet>("UIToolkit/Chip/ChipStyle.uss");

            // ReSharper disable once PossibleNullReferenceException
            // foreach (ColorPaletteArray.ColorInfo colorInfo in colorPaletteArray)

            SerializedProperty colorInfoArrayProp = _so.FindProperty(nameof(ColorPaletteArray.colorInfoArray));
            ColorInfoArray result = new ColorInfoArray(rootScoller, colorInfoArrayProp)
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                },
            };
            rootScoller.Add(result);

            // List<ColorPaletteLabels> allColorPaletteLabels = new List<ColorPaletteLabels>();
            // for (int index = 0; index < colorInfoArrayProp.arraySize; index++)
            // {
            //     SerializedProperty colorInfoProp = colorInfoArrayProp.GetArrayElementAtIndex(index);
            //
            //     TemplateContainer container = containerTree.CloneTree();
            //     VisualElement containerRoot = container.Q<VisualElement>("container-root");
            //
            //     VisualElement colorContainer = containerRoot.Q<VisualElement>("color-container");
            //
            //     SerializedProperty colorInfoColorProp = colorInfoProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.color));
            //
            //     ColorField colorField = colorContainer.Q<ColorField>("color");
            //     colorField.BindProperty(colorInfoColorProp);
            //
            //     Button delete = colorContainer.Q<Button>("delete");
            //     int thisIndex = index;
            //     delete.clicked += () =>
            //     {
            //         colorInfoArrayProp.DeleteArrayElementAtIndex(thisIndex);
            //         colorInfoArrayProp.serializedObject.ApplyModifiedProperties();
            //     };
            //
            //     SerializedProperty colorInfoLabelsProp = colorInfoProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels));
            //
            //     ColorPaletteLabels colorPaletteLabels = new ColorPaletteLabels(containerRoot, colorInfoLabelsProp);
            //     allColorPaletteLabels.Add(colorPaletteLabels);
            //     containerRoot.Add(colorPaletteLabels);
            //
            //     colorPaletteLabels.Add(new CleanableTextInputTypeAhead(colorInfoLabelsProp, rootScoller, colorInfoArrayProp));
            //
            //     containerLayout.Add(container);
            // }
            //
            // foreach (ColorPaletteLabels colorPaletteLabels in allColorPaletteLabels)
            // {
            //     colorPaletteLabels.BindAllColorPaletteLabels(rootVisualElement, allColorPaletteLabels);
            //     foreach (ColorPaletteLabel colorPaletteLabel in colorPaletteLabels.Labels)
            //     {
            //         LabelPointerManipulator _ = new LabelPointerManipulator(colorPaletteLabel, colorPaletteLabels, allColorPaletteLabels);
            //     }
            // }
        }
    }
}
#endif
