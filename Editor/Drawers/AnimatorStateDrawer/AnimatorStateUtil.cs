using System;
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace SaintsField.Editor.Drawers.AnimatorStateDrawer
{
    // private static bool EqualAnimatorState(AnimatorStateChanged eachStateInfo, AnimatorStateChanged eachStateInfo2)
    // {
    //     bool layerIndexEqual = eachStateInfo2.layerIndex == eachStateInfo.layerIndex;
    //     bool stateNameEqual = eachStateInfo2.state.name == eachStateInfo.state.name;
    //     bool stateNameHashEqual =
    //         eachStateInfo2.state.nameHash == eachStateInfo.state.nameHash;
    //
    //     if (!layerIndexEqual || !stateNameEqual || !stateNameHashEqual)
    //     {
    //         return false;
    //     }
    //
    //     IReadOnlyList<string> subStateMachineNameChainProp =
    //         eachStateInfo2.subStateMachineNameChain;
    //     if (subStateMachineNameChainProp == null)
    //     {
    //         return true;
    //     }
    //
    //     return eachStateInfo.subStateMachineNameChain.SequenceEqual(subStateMachineNameChainProp);
    // }

    public static class AnimatorStateUtil
    {
        public static void OpenAnimator(UnityEngine.Object animatorController)
        {
            Selection.activeObject = animatorController;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }

        public static void ShowDropdown(int selectedIndex, IReadOnlyList<AnimatorStateChanged> animatorStates, RuntimeAnimatorController runtimeAnimatorController, Rect rootWorldBound, Action<AnimatorStateChanged> onChange)
        {
            AdvancedDropdownList<AnimatorStateChanged> lis =
                new AdvancedDropdownList<AnimatorStateChanged>();

            // foreach (int index in Enumerable.Range(0, metaInfo.AnimatorStates.Count))
            foreach (AnimatorStateChanged value in animatorStates)
            {
                string name = StateListItemLabel(value);
                // Debug.Log($"name=`{name}`");
                lis.Add(name, value);
            }

            if (runtimeAnimatorController != null)
            {
                if (animatorStates.Count > 0)
                {
                    lis.AddSeparator();
                }

                lis.Add($"Edit {runtimeAnimatorController.name}...", null);
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedIndex < 0 ? Array.Empty<object>(): new object[] { animatorStates[selectedIndex] },
                DropdownListValue = lis,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(rootWorldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                rootWorldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    AnimatorStateChanged newV = (AnimatorStateChanged)curItem;
                    if (newV is null)
                    {
                        if(runtimeAnimatorController != null)
                        {
                            OpenAnimator(runtimeAnimatorController);
                        }

                        return null;
                    }

                    onChange(newV);
                    return null;
                }
            );

            PopupWindow.Show(worldBound, sa);
        }

        private static string StateListItemLabel(AnimatorStateChanged animatorStateInfo)
        {
            string preText = animatorStateInfo.subStateMachineNameChain.Count == 0
                ? ""
                : $"{string.Join('/', animatorStateInfo.subStateMachineNameChain)}/";
            string clipText;
            string iconText = "";
            if (animatorStateInfo.animationClip == null)
            {
                clipText = "";
            }
            else
            {
                clipText = $" <color=gray>({animatorStateInfo.animationClip.name})</color>";
                iconText = "<icon=d_AnimationClip Icon/>";
            }
            return
                preText
                + iconText
                + animatorStateInfo.state.name
                + clipText
                + ": " + animatorStateInfo.layer.name;
        }

        public static void StateButtonLabel(Label label, AnimatorStateChanged animatorStateInfo, RichTextDrawer richTextDrawer)
        {
            List<RichTextDrawer.RichTextChunk> chunks = new List<RichTextDrawer.RichTextChunk>();
            if (animatorStateInfo.animationClip != null)
            {
                chunks.Add(new RichTextDrawer.RichTextChunk("<icon=d_AnimationClip Icon/>", true, "d_AnimationClip Icon"));
            }

            string content =
                $"{animatorStateInfo.state.name}" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(EColor.Gray.GetColor())}>" +
                (animatorStateInfo.animationClip == null ? "" : $" ({animatorStateInfo.animationClip.name})") +
                ": " +
                animatorStateInfo.layer.name +
                (animatorStateInfo.subStateMachineNameChain.Count == 0 ? "" : $"/{string.Join('/', animatorStateInfo.subStateMachineNameChain)}") +
                "</color>";
            chunks.Add(new RichTextDrawer.RichTextChunk(content, false, content));

            UIToolkitUtils.SetLabel(label, chunks, richTextDrawer);
        }
    }
}
