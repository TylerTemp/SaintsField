#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using SaintsField.Editor.ColorPalette.UIToolkit;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette
{
    public class ColorPaletteEditorWindow: SaintsEditorWindow
    {

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Color Palette...")]
#else
        [MenuItem("Window/Saints/Color Palette...")]
#endif
        public static void OpenColorPaletteEditorWindow()
        {
            ColorPaletteEditorWindow window = GetWindow<ColorPaletteEditorWindow>(false, "Color Palette");
            window.Show();
        }

#if SAINTSFIELD_DEBUG
        [InitializeOnLoadMethod]
        private static void ReOpen()
        {
            ColorPaletteEditorWindow window = GetWindow<ColorPaletteEditorWindow>(false, "Color Palette");
            window.Close();
            OpenColorPaletteEditorWindow();
        }
#endif

        [GetScriptableObject, NoLabel, OnValueChanged(nameof(ColorPaletteArrayChanged))]
        [BelowSeparator(5), BelowSeparator(EColor.Gray), BelowSeparator(5)]
        public ColorPaletteArray colorPaletteArray;

        private SerializedObject _so;

        private void ColorPaletteArrayChanged(ColorPaletteArray cpa)
        {
            _so?.ApplyModifiedProperties();
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
            _so?.Update();
        }

        // private readonly List<ColorPaletteLabels> _colorPaletteLabels = new List<ColorPaletteLabels>();

        protected override void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            ScrollView rootScoller = EditorCreatInspectingTarget();
            root.Add(rootScoller);

            if (_so == null)
            {
                return;
            }

            VisualTreeAsset containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/Container.uxml");

            VisualElement containerLayout = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                },
            };
            rootScoller.Add(containerLayout);

            // StyleSheet style = Util.LoadResource<StyleSheet>("UIToolkit/Chip/ChipStyle.uss");

            // ReSharper disable once PossibleNullReferenceException
            // foreach (ColorPaletteArray.ColorInfo colorInfo in colorPaletteArray)

            SerializedProperty prop = _so.FindProperty(nameof(ColorPaletteArray.colorInfoArray));

            List<ColorPaletteLabels> allColorPaletteLabels = new List<ColorPaletteLabels>();
            for (int index = 0; index < prop.arraySize; index++)
            {
                SerializedProperty colorInfoProp = prop.GetArrayElementAtIndex(index);

                TemplateContainer container = containerTree.CloneTree();
                VisualElement containerRoot = container.Q<VisualElement>("container-root");

                // foreach (VisualElement child in containerRoot.Query<VisualElement>("chip-root").ToList())
                // {
                //     child.RemoveFromHierarchy();
                // }

                VisualElement colorContainer = new VisualElement
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = SaintsPropertyDrawer.SingleLineHeight,
                        marginBottom = 4,
                    },
                };

                SerializedProperty colorInfoColorProp = colorInfoProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.color));

                ColorField colorField = new ColorField
                {
                    value = colorInfoColorProp.colorValue,
                    style =
                    {
                        width = Length.Percent(100),
                        height = SaintsPropertyDrawer.SingleLineHeight - 2,
                    },
                };
                colorField.BindProperty(colorInfoColorProp);
                colorContainer.Add(colorField);

                containerRoot.Insert(0, colorContainer);

                SerializedProperty colorInfoLabelsProp = colorInfoProp.FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels));

                ColorPaletteLabels colorPaletteLabels = new ColorPaletteLabels(containerRoot, colorInfoLabelsProp);
                allColorPaletteLabels.Add(colorPaletteLabels);
                containerRoot.Add(colorPaletteLabels);

                colorPaletteLabels.Add(new CleanableTextInput());

                containerLayout.Add(container);
            }

            foreach (ColorPaletteLabels colorPaletteLabels in allColorPaletteLabels)
            {
                colorPaletteLabels.BindAllColorPaletteLabels(rootVisualElement, allColorPaletteLabels);
                foreach (ColorPaletteLabel colorPaletteLabel in colorPaletteLabels.Labels)
                {
                    LabelPointerManipulator _ = new LabelPointerManipulator(rootVisualElement, colorPaletteLabel, colorPaletteLabels, allColorPaletteLabels);
                }
            }
        }
    }
}
#endif
