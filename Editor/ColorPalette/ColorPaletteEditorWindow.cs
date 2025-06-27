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

// #if SAINTSFIELD_DEBUG
//         [MenuItem("Saints/Color Palette...")]
// #else
//         [MenuItem("Window/Saints/Color Palette...")]
// #endif
//         public static void OpenColorPaletteEditorWindow()
//         {
//             ColorPaletteEditorWindow window = GetWindow<ColorPaletteEditorWindow>(false, "Color Palette");
//             window.Show();
//         }
//
// #if SAINTSFIELD_DEBUG
//         [InitializeOnLoadMethod]
//         private static void ReOpen()
//         {
//             ColorPaletteEditorWindow window = GetWindow<ColorPaletteEditorWindow>(false, "Color Palette");
//             window.Close();
//             OpenColorPaletteEditorWindow();
//         }
// #endif

        protected override void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            ScrollView rootScoller = EditorCreatInspectingTarget();
            root.Add(rootScoller);

            VisualTreeAsset containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/Container.uxml");

            VisualElement containerLayout = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                },
            };
            rootScoller.Insert(0, containerLayout);

            // StyleSheet style = Util.LoadResource<StyleSheet>("UIToolkit/Chip/ChipStyle.uss");

            for (int _ = 0; _ < 10; _++)
            {
                TemplateContainer container = containerTree.CloneTree();
                VisualElement containerRoot = container.Q<VisualElement>("container-root");

                foreach (VisualElement child in containerRoot.Query<VisualElement>("chip-root").ToList())
                {
                    child.RemoveFromHierarchy();
                }

                VisualElement colorContainer = new VisualElement
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = SaintsPropertyDrawer.SingleLineHeight,
                        marginBottom = 4,
                    },
                };

                ColorField colorField = new ColorField
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = SaintsPropertyDrawer.SingleLineHeight - 2,
                    },
                };
                colorContainer.Add(colorField);

                containerRoot.Insert(0, colorContainer);

                for (int index = 0; index < 4; index++)
                {
                    ColorPaletteLabel colorPaletteLabel = new ColorPaletteLabel($"Label{index}");
                    // colorPaletteLabel.styleSheets.Add(style);
                    // colorPaletteLabel.DualButtonChip.Button1.style.cursor = UIElementsEditorUtility.CreateDefaultCursorStyle(
                    //     MouseCursor.Pan);
                    containerRoot.Add(colorPaletteLabel);
                }

                containerLayout.Add(container);
            }
        }
    }
}
