using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer: SaintsPropertyDrawer
    {
        private bool _unfold;
        private bool _forceUnfold;

        private readonly Texture2D _checkboxCheckedTexture2D;
        private readonly Texture2D _checkboxEmptyTexture2D;
        private readonly Texture2D _checkboxIndeterminateTexture2D;

        private readonly GUIContent _checkBoxCheckedContent;
        private readonly GUIContent _checkBoxEmptyContent;
        private readonly GUIContent _checkBoxIndeterminateContent;

        private readonly GUIStyle _iconButtonStyle;

        private struct BtnInfo
        {
            public GUIContent Label;
            public GUIStyle LabelStyle;
            public float LabelWidth;
            public Action Action;
            public bool Disabled;
            public bool Toggled;
        }

        private bool _initExpandState;

        public EnumFlagsAttributeDrawer()
        {
            _checkboxCheckedTexture2D = RichTextDrawer.LoadTexture("checkbox-checked.png");
            _checkboxEmptyTexture2D = RichTextDrawer.LoadTexture("checkbox-outline-blank.png");
            _checkboxIndeterminateTexture2D = RichTextDrawer.LoadTexture("checkbox-outline-indeterminate.png");

            _checkBoxCheckedContent = new GUIContent(_checkboxCheckedTexture2D);
            _checkBoxEmptyContent = new GUIContent(_checkboxEmptyTexture2D);
            _checkBoxIndeterminateContent = new GUIContent(_checkboxIndeterminateTexture2D);

            const int padding = 2;

            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(padding, padding, padding, padding),
            };
        }

        ~EnumFlagsAttributeDrawer()
        {
            Object.DestroyImmediate(_checkboxCheckedTexture2D);
            Object.DestroyImmediate(_checkboxEmptyTexture2D);
            Object.DestroyImmediate(_checkboxIndeterminateTexture2D);
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth)
        {
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if (!_initExpandState)
            {
                _initExpandState = true;
                _unfold = enumFlagsAttribute.DefaultExpanded;
            }

            bool unfold = _unfold || _forceUnfold;

            // Debug.Log($"_unfold={_unfold}, _forceUnfold={_forceUnfold}, Event.current.type={Event.current.type}");

            if (!unfold)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            int[] values = Enum.GetValues(SerializedUtils.GetType(property)).Cast<int>().ToArray();
            int allOnValue = values.Aggregate(0, (acc, value) => acc | value);
            int valueCount = values.Count(each => each != 0 && each != allOnValue);
            return EditorGUIUtility.singleLineHeight * (valueCount + 1);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if (!_initExpandState)
            {
                _initExpandState = true;
                _unfold = enumFlagsAttribute.DefaultExpanded;
            }

            int[] allIntValues = Enum.GetValues(SerializedUtils.GetType(property)).Cast<int>().Where(each => each != 0).ToArray();

            #region label+button
            Rect headRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            float labelWidth = string.IsNullOrEmpty(label.text)? 0: EditorGUIUtility.labelWidth;

            CheckAutoExpand(position.width - labelWidth, property, enumFlagsAttribute);
            if (_forceUnfold)
            {
                EditorGUI.LabelField(headRect, label);
            }
            else
            {
                _unfold = EditorGUI.Foldout(headRect, _unfold, label);
            }

            Rect fieldRect = RectUtils.SplitWidthRect(position, labelWidth).leftRect;

            // Rect leftRect = new Rect(headRect)
            // {
            //     x = headRect.x + labelWidth,
            //     width = headRect.width - labelWidth,
            // };

            bool noneChecked = property.intValue == 0;
            int allCheckedInt = allIntValues.Aggregate(0, (acc, value) => acc | value);
            bool allChecked = property.intValue == allCheckedInt;

            // Debug.Log($"property.intValue = {property.intValue}; noneChecked={noneChecked}, allChecked={allChecked}");

            bool useUnfold = _unfold || _forceUnfold;

            if(useUnfold)
            {
                BtnRender(fieldRect, new[]
                {
                    new BtnInfo
                    {
                        Label = _checkBoxEmptyContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = 0,
                        Disabled = false,
                        Toggled = noneChecked,
                    },
                    new BtnInfo
                    {
                        Label = _checkBoxCheckedContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = allCheckedInt,
                        Disabled = false,
                        Toggled = allChecked,
                    },
                });
            }
            else
            {
                BtnInfo toggleButton;
                if (allChecked)
                {
                    toggleButton = new BtnInfo
                    {
                        Label = _checkBoxCheckedContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = 0,
                        Disabled = false,
                        Toggled = false,
                    };
                }
                else if (noneChecked)
                {
                    toggleButton = new BtnInfo
                    {
                        Label = _checkBoxEmptyContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = allCheckedInt,
                        Disabled = false,
                        Toggled = false,
                    };
                }
                else
                {
                    toggleButton = new BtnInfo
                    {
                        Label = _checkBoxIndeterminateContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = 0,
                        Disabled = false,
                        Toggled = false,
                    };
                }

                List<BtnInfo> btnInfos = new List<BtnInfo>{toggleButton};
                int curValue = property.intValue;
                foreach ((int value, string name) in Enum.GetValues(SerializedUtils.GetType(property))
                             .Cast<object>()
                             .Select(each => ((int) each, each.ToString()))
                             .Where(each => each.Item1 != 0 && each.Item1 != allCheckedInt)
                         )
                {
                    bool on = (curValue & value) != 0;
                    GUIContent btnLabel = new GUIContent(name);
                    // GUIStyle btnStyle = on ? activeBtn : normalBtn;
                    btnInfos.Add(new BtnInfo
                    {
                        Label = btnLabel,
                        LabelStyle = EditorStyles.miniButton,
                        LabelWidth = EditorStyles.miniButton.CalcSize(btnLabel).x,
                        Action = () => property.intValue ^= value,
                        Disabled = false,
                        Toggled = on,
                    });
                }

                // btnInfos.Add(toggleButton);

                BtnRender(fieldRect, btnInfos);
            }
            #endregion

            // ReSharper disable once InvertIf
            if(useUnfold)
            {
                int curValue = property.intValue;
                foreach ((int value, string name, int index) in Enum.GetValues(SerializedUtils.GetType(property))
                             .Cast<object>()
                             .Where(each => (int) each != 0 && (int) each != allCheckedInt)
                             .Select((each, index) => ((int) each, each.ToString(), index)))
                {
                    bool on = (curValue & value) != 0;

                    GUIStyle normalBtn = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.LowerLeft,
                    };

                    Rect btnRect = new Rect(fieldRect)
                    {
                        // x = 40f,
                        y = headRect.y + headRect.height + EditorGUIUtility.singleLineHeight * index,
                        // width = position.width - 22f,
                        // width = position.width,
                        height = EditorGUIUtility.singleLineHeight,
                    };

                    using (EditorGUIBackgroundColor.ToggleButton(on))
                    {
                        if (GUI.Button(btnRect, $"{(on ? "☑" : "☐")} | {name}", normalBtn))
                        {
                            property.intValue ^= value;
                        }
                    }
                }
            }
        }

        private void CheckAutoExpand(float positionWidth, SerializedProperty property, EnumFlagsAttribute enumFlagsAttribute)
        {
            if (positionWidth <= 0)  // layout event will give this to negative... wait for repaint to do correct calculate
            {
                return;
            }

            _forceUnfold = false;

            if (_unfold)
            {
                return;
            }

            if (!enumFlagsAttribute.AutoExpand)
            {
                return;
            }

            (int, string)[] allValues = Enum.GetValues(SerializedUtils.GetType(property))
                .Cast<object>()
                .Select(each => ((int)each, each.ToString())).ToArray();

            int allCheckedInt = allValues.Select(each => each.Item1).Aggregate(0, (acc, value) => acc | value);
            IEnumerable<string> stringValues = allValues
                .Where(each => each.Item1 != 0 && each.Item1 != allCheckedInt)
                .Select(each => each.Item2);

            float totalBtnWidth = EditorGUIUtility.singleLineHeight + stringValues.Sum(each => EditorStyles.miniButton.CalcSize(new GUIContent(each)).x);

            _forceUnfold = totalBtnWidth > positionWidth;
            // Debug.Log($"totalBtnWidth = {totalBtnWidth}, positionWidth = {positionWidth}, _forceUnfold = {_forceUnfold}, event={Event.current.type}");
        }

        private static void BtnRender(Rect position, IReadOnlyList<BtnInfo> btnInfos)
        {
            // GUI.backgroundColor = Color.grey;
            // Color oldColor = GUI.backgroundColor;
            // float totalSpaceWidth = position.width;
            // float totalBtnWidth = btnInfos.Sum(each => each.LabelWidth);
            // if (totalSpaceWidth >= totalBtnWidth)
            // {
            //     // 倒排
            //     float backX = position.x + position.width;
            //     foreach (BtnInfo btnInfo in btnInfos.Reverse())
            //     {
            //         backX -= btnInfo.LabelWidth;
            //         Rect btnRect = new Rect(position)
            //         {
            //             x = backX,
            //             width = btnInfo.LabelWidth,
            //         };
            //
            //         using(new EditorGUI.DisabledScope(btnInfo.Disabled))
            //         using (EditorGUIBackgroundColor.ToggleButton(btnInfo.Toggled))
            //         {
            //             if (GUI.Button(btnRect, btnInfo.Label, btnInfo.LabelStyle))
            //             {
            //                 btnInfo.Action.Invoke();
            //             }
            //         }
            //
            //     }
            // }
            // else
            // {
            // 顺排
            float eachX = position.x;
            foreach (BtnInfo btnInfo in btnInfos)
            {
                Rect btnRect = new Rect(position)
                {
                    x = eachX,
                    width = btnInfo.LabelWidth,
                };
                using(new EditorGUI.DisabledScope(btnInfo.Disabled))
                using (EditorGUIBackgroundColor.ToggleButton(btnInfo.Toggled))
                {
                    if (GUI.Button(btnRect, btnInfo.Label, btnInfo.LabelStyle))
                    {
                        btnInfo.Action.Invoke();
                    }
                }

                eachX += btnInfo.LabelWidth;
            }
            // }
        }

    }
}
