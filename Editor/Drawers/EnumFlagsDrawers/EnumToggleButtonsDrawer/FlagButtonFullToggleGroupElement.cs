using System;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class FlagButtonFullToggleGroupElement: VisualElement
    {
        public readonly Button HToggleButton;
        public readonly Button HCheckAllButton;
        public readonly Button HEmptyButton;
        private readonly Texture2D _checkboxCheckedTexture2D;
        private readonly Texture2D _checkboxEmptyTexture2D;
        private readonly Texture2D _checkboxIndeterminateTexture2D;

        public FlagButtonFullToggleGroupElement()
        {
            style.flexDirection = FlexDirection.Row;

            _checkboxCheckedTexture2D = Util.LoadResource<Texture2D>("checkbox-checked.png");
            _checkboxEmptyTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
            _checkboxIndeterminateTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-indeterminate.png");

            HToggleButton = new Button
            {
                style =
                {
                    width = EditorGUIUtility.singleLineHeight - 2,
                    // height = EditorGUIUtility.singleLineHeight - 2,
                    height = 20,
                    // paddingTop = 0,
                    // paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    backgroundImage = _checkboxIndeterminateTexture2D,
                    // backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    // borderTopWidth = 0,
                    // borderBottomWidth = 0,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 4, EditorGUIUtility.singleLineHeight - 4),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            Add(HToggleButton);

            HCheckAllButton = new Button
            {
                style =
                {
                    width = EditorGUIUtility.singleLineHeight - 2,
                    // height = EditorGUIUtility.singleLineHeight - 2,
                    height = 20,
                    // paddingTop = 0,
                    // paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    backgroundImage = _checkboxCheckedTexture2D,
                    // backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    // borderTopWidth = 0,
                    // borderBottomWidth = 0,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                    borderTopRightRadius = 0,
                    borderBottomRightRadius = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 4, EditorGUIUtility.singleLineHeight - 4),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            Add(HCheckAllButton);

            HEmptyButton = new Button
            {
                style =
                {
                    width = EditorGUIUtility.singleLineHeight - 2,
                    // height = EditorGUIUtility.singleLineHeight - 2,
                    height = 20,
                    // paddingTop = 0,
                    // paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    backgroundImage = _checkboxEmptyTexture2D,
                    // backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    // borderTopWidth = 0,
                    // borderBottomWidth = 0,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 4, EditorGUIUtility.singleLineHeight - 4),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            Add(HEmptyButton);

            ToFullToggles(false);
        }

        public void ToFullToggles(bool full)
        {
            if (full)
            {
                HToggleButton.style.display = DisplayStyle.None;
                HCheckAllButton.style.display = DisplayStyle.Flex;
                HEmptyButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                HToggleButton.style.display = DisplayStyle.Flex;
                HCheckAllButton.style.display = DisplayStyle.None;
                HEmptyButton.style.display = DisplayStyle.None;
            }
        }

        public void RefreshValue(object curValue, EnumMetaInfo metaInfo)
        {
            object newEnum = Enum.ToObject(metaInfo.EnumType, curValue);
            object zeroBit = Enum.ToObject(metaInfo.EnumType, 0);
            if (newEnum.Equals(zeroBit))  // now is zero, click to everything
            {
                HToggleButton.style.backgroundImage = _checkboxEmptyTexture2D;
                HToggleButton.userData = metaInfo.EverythingBit;

                HEmptyButton.SetEnabled(false);
                HCheckAllButton.SetEnabled(true);
            }
            else if (newEnum.Equals(metaInfo.EverythingBit))  // now is everything, click to zero
            {
                HToggleButton.style.backgroundImage = _checkboxCheckedTexture2D;
                HToggleButton.userData = zeroBit;

                HEmptyButton.SetEnabled(true);
                HCheckAllButton.SetEnabled(false);
            }
            else  // now is partly, click to everything
            {
                HToggleButton.style.backgroundImage = _checkboxIndeterminateTexture2D;
                HToggleButton.userData = metaInfo.EverythingBit;

                HEmptyButton.SetEnabled(true);
                HCheckAllButton.SetEnabled(true);
            }
        }

    }
}
