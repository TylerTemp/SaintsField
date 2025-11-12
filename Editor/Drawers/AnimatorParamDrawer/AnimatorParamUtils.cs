using System;
using System.Collections.Generic;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AnimatorParamDrawer
{
    public static class AnimatorParamUtils
    {
        public static void OpenAnimator(UnityEngine.Object animatorController)
        {
            Selection.activeObject = animatorController;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }

        private static string _brownColor;

        public static void ShowDropdown(bool isString, object curValue, IReadOnlyList<AnimatorControllerParameter> animatorControllerParams, Animator _cachedAnimator, Rect rootWorldBound, Action<AnimatorControllerParameter> setValue)
        {
            _brownColor ??= $"#{ColorUtility.ToHtmlStringRGB(EColor.Brown.GetColor())}";

            AnimatorControllerParameter selectedParam = null;

            AdvancedDropdownList<AnimatorControllerParameter> lis =
                new AdvancedDropdownList<AnimatorControllerParameter>();
            foreach (AnimatorControllerParameter cachedAnimatorControllerParam in animatorControllerParams)
            {
                lis.Add(
                    $"{cachedAnimatorControllerParam.name} <color={_brownColor}>{cachedAnimatorControllerParam.type}</color> <color=#808080>({cachedAnimatorControllerParam.nameHash})</color>",
                    cachedAnimatorControllerParam);

                if (isString && cachedAnimatorControllerParam.name == (string)curValue
                    || !isString && cachedAnimatorControllerParam.nameHash == (int)curValue)
                {
                    selectedParam = cachedAnimatorControllerParam;
                }
            }

            if(_cachedAnimator != null)
            {
                if (animatorControllerParams.Count > 0)
                {
                    lis.AddSeparator();
                }

                lis.Add("Edit Animator...", null);
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedParam is null ? Array.Empty<object>(): new object[] { selectedParam },
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
                    AnimatorControllerParameter newV = (AnimatorControllerParameter)curItem;
                    if (newV is null)
                    {
                        if(_cachedAnimator != null)
                        {
                            OpenAnimator(_cachedAnimator);
                        }

                        return null;
                    }

                    setValue(newV);
                    return null;
                }
            );

            PopupWindow.Show(worldBound, sa);
        }

    }
}
