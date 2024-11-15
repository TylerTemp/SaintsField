#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Utils
{
#if UNITY_2021_3_OR_NEWER
    public static class UIToolkitUtils
    {

        // public static void FixLabelWidthLoopUIToolkit(Label label)
        // {
        //     // FixLabelWidthUIToolkit(label);
        //     // label.schedule.Execute(() => FixLabelWidthUIToolkit(label)).StartingIn(250);
        //     label.RegisterCallback<GeometryChangedEvent>(evt => FixLabelWidthUIToolkit((Label)evt.target));
        // }
        //
        // // ReSharper disable once SuggestBaseTypeForParameter
        // public static void FixLabelWidthUIToolkit(Label label)
        // {
        //     StyleLength autoLength = new StyleLength(StyleKeyword.Auto);
        //     StyleLength curLenght = label.style.width;
        //     float resolvedWidth = label.resolvedStyle.width;
        //     // if(curLenght.value != autoLength)
        //     // don't ask me why we need to compare with 0, ask Unity...
        //     if(
        //         // !(curLenght.value.IsAuto()  // IsAuto() is not available in 2021.3.0f1
        //         !(curLenght.keyword == StyleKeyword.Auto
        //           || curLenght.value == 0)
        //         && !float.IsNaN(resolvedWidth) && resolvedWidth > 0)
        //     {
        //         // Debug.Log($"try fix {label.style.width}({curLenght.value.IsAuto()}); {resolvedWidth > 0} {resolvedWidth}");
        //         label.style.width = autoLength;
        //         // label.schedule.Execute(() => label.style.width = autoLength);
        //     }
        // }

        public static void WaitUntilThenDo<T>(VisualElement container, Func<(bool ok, T result)> until, Action<T> thenDo, long delay=0)
        {
            (bool ok, T result) = until.Invoke();
            if (ok)
            {
                thenDo.Invoke(result);
                return;
            }

            if(delay > 1000)
            {
                return;
            }

            // if (delay <= 0)
            // {
            //     container.schedule.Execute(() =>
            //     {
            //         (bool ok, T result) = until.Invoke();
            //         if (ok)
            //         {
            //             thenDo.Invoke(result);
            //         }
            //     });
            // }

            container.schedule.Execute(() => WaitUntilThenDo(container, until, thenDo, delay+200)).StartingIn(delay);
        }

        public static void ChangeLabelLoop(VisualElement container, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull, RichTextDrawer richTextDrawer)
        {
            // container.RegisterCallback<GeometryChangedEvent>(evt => ChangeLabel((VisualElement)evt.target, chunks));
            ChangeLabel(container, chunksOrNull, richTextDrawer, 0f);
        }

        private static void ChangeLabel(VisualElement container, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull, RichTextDrawer richTextDrawer, float delayTime)
        {
            if (delayTime > 1f)  // stop trying after 1 second
            {
                IMGUIContainer imguiContainer = container.Q<IMGUIContainer>(className: IMGUILabelHelper.ClassName);
                if (imguiContainer?.userData is IMGUILabelHelper imguiLabelHelper)
                {
                    if (chunksOrNull is null)
                    {
                        imguiLabelHelper.NoLabel = true;
                        return;
                    }

                    // ReSharper disable once PossibleMultipleEnumeration
                    RichTextDrawer.RichTextChunk[] chunks = chunksOrNull.ToArray();
                    string labelString = string.Join("", chunks.Where(each => !each.IsIcon).Select(each => each.Content));
                    imguiLabelHelper.RichLabel = labelString;
                    imguiLabelHelper.NoLabel = false;
                }
                return;
            }

            Label label = container.Q<Label>(className: "unity-label");
            if (label == null)
            {
                container.schedule.Execute(() => ChangeLabel(container, chunksOrNull, richTextDrawer, delayTime + 0.3f));
                return;
            }

            SetLabel(label, chunksOrNull, richTextDrawer);
        }

        public static void SetLabel(Label label, IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull,
            RichTextDrawer richTextDrawer)
        {
            if (chunksOrNull == null)
            {
                label.style.display = DisplayStyle.None;
                return;
            }

            label.Clear();
            label.text = "";
            label.style.flexDirection = FlexDirection.Row;
            // label.style.alignItems = Align.Center;
            // label.style.height = EditorGUIUtility.singleLineHeight;
            foreach (VisualElement richChunk in richTextDrawer.DrawChunksUIToolKit(chunksOrNull))
            {
                label.Add(richChunk);
            }
            label.style.display = DisplayStyle.Flex;
        }

        public class DropdownButtonField : BaseField<string>
        {
            public readonly Button ButtonElement;
            public readonly Label ButtonLabelElement;
            // private readonly MethodInfo AlignLabel;

            public DropdownButtonField(string label, Button visualInput, Label buttonLabel) : base(label, visualInput)
            {
                ButtonElement = visualInput;
                ButtonLabelElement = buttonLabel;

                // AlignLabel = typeof(BaseField<string>).GetMethod("AlignLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            // public void AlignLabelForce()
            // {
            //     AlignLabel.Invoke(this, new object[]{});
            // }
        }

        public static DropdownButtonField MakeDropdownButtonUIToolkit(string label)
        {
            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    flexGrow = 1,
                    flexShrink = 1,

                    paddingRight = 2,
                    marginRight = 0,
                    marginLeft = 0,
                    alignItems = Align.FlexStart,
                },
                // name = NameButtonField(property),
                // userData = metaInfo.SelectedIndex == -1
                //     ? null
                //     : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2,
            };

            Label buttonLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    // paddingRight = 20,
                    // textOverflow = TextOverflow.Ellipsis,
                    // unityOverflowClipBox = OverflowClipBox.PaddingBox,
                    overflow = Overflow.Hidden,
                    marginRight = 15,
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            };

            button.Add(buttonLabel);

            DropdownButtonField dropdownButtonField = new DropdownButtonField(label, button, buttonLabel)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            // dropdownButtonField.AddToClassList("unity-base-field__aligned");
            dropdownButtonField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);

            dropdownButtonField.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    maxWidth = 12,
                    maxHeight = EditorGUIUtility.singleLineHeight,
                    position = Position.Absolute,
                    right = 4,
                },
                pickingMode = PickingMode.Ignore,
            });

            return dropdownButtonField;
        }

        public static IEnumerable<VisualElement> FindParentClass(VisualElement element, string className)
        {
            return IterUpWithSelf(element).Where(each => each.ClassListContains(className));
        }

        public static IEnumerable<VisualElement> IterUpWithSelf(VisualElement element)
        {
            if(element == null)
            {
                yield break;
            }

            yield return element;

            foreach (VisualElement visualElement in IterUpWithSelf(element.parent))
            {
                yield return visualElement;
            }
        }
    }
#endif
}
