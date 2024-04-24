
using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Utils
{
#if UNITY_2021_3_OR_NEWER
    public static class UIToolkitUtils
    {

        public static void FixLabelWidthLoopUIToolkit(Label label)
        {
            // FixLabelWidthUIToolkit(label);
            // label.schedule.Execute(() => FixLabelWidthUIToolkit(label)).StartingIn(250);
            label.RegisterCallback<GeometryChangedEvent>(evt => FixLabelWidthUIToolkit((Label)evt.target));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public static void FixLabelWidthUIToolkit(Label label)
        {
            StyleLength autoLength = new StyleLength(StyleKeyword.Auto);
            StyleLength curLenght = label.style.width;
            float resolvedWidth = label.resolvedStyle.width;
            // if(curLenght.value != autoLength)
            // don't ask me why we need to compare with 0, ask Unity...
            if(
                // !(curLenght.value.IsAuto()  // IsAuto() is not available in 2021.3.0f1
                !(curLenght.keyword == StyleKeyword.Auto
                  || curLenght.value == 0)
                && !float.IsNaN(resolvedWidth) && resolvedWidth > 0)
            {
                // Debug.Log($"try fix {label.style.width}({curLenght.value.IsAuto()}); {resolvedWidth > 0} {resolvedWidth}");
                label.style.width = autoLength;
                // label.schedule.Execute(() => label.style.width = autoLength);
            }
        }

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

        public struct DropdownButtonUIToolkit
        {
            // ReSharper disable InconsistentNaming
            public Button Button;
            public Label Label;
            // ReSharper enable InconsistentNaming
        }

        public static DropdownButtonUIToolkit MakeDropdownButtonUIToolkit()
        {
            Button button = new Button
            {
                style =
                {
                    height = EditorGUIUtility.singleLineHeight,
                    // flexGrow = 1,
                    paddingRight = 2,
                    marginRight = 0,
                    marginLeft = 0,
                },
                // name = NameButtonField(property),
                // userData = metaInfo.SelectedIndex == -1
                //     ? null
                //     : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2,
            };

            VisualElement buttonLabelContainer = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                },
            };

            Label label = new Label();

            buttonLabelContainer.Add(label);
            buttonLabelContainer.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    maxWidth = 15,
                },
            });

            button.Add(buttonLabelContainer);

            // return Button;
            return new DropdownButtonUIToolkit
            {
                Button = button,
                Label = label,
            };
        }
    }
#endif
}
