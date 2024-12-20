using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public partial class EnumFlagsAttributeDrawer
    {
        #region IMGUI
        // private bool _unfold;
        private bool _forceUnfold;

        private GUIContent _checkBoxCheckedContent;
        private GUIContent _checkBoxEmptyContent;
        private GUIContent _checkBoxIndeterminateContent;

        private GUIStyle _iconButtonStyle;
        private GUIStyle _miniButtonStyle;
        private GUIStyle _normalButtonStyle;

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

        private bool EnsureImageResourcesLoaded()
        {
            if (_checkboxCheckedTexture2D != null)
            {
                return true;
            }

            LoadIcons();

            return false;
        }

        private void ImGuiLoadResources()
        {
            if (EnsureImageResourcesLoaded())
            {
                return;
            }

            // ImGuiEnsureDispose(property.serializedObject.targetObject);

            _checkBoxCheckedContent = new GUIContent(_checkboxCheckedTexture2D);
            _checkBoxEmptyContent = new GUIContent(_checkboxEmptyTexture2D);
            _checkBoxIndeterminateContent = new GUIContent(_checkboxIndeterminateTexture2D);

            const int padding = 2;

            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(padding, padding, padding, padding),
                richText = true,
            };
            _miniButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                richText = true,
            };
            _normalButtonStyle = new GUIStyle(_miniButtonStyle)
            {
                alignment = TextAnchor.MiddleLeft,
            };
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            ImGuiLoadResources();

            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if (!_initExpandState)
            {
                _initExpandState = true;
                property.isExpanded = enumFlagsAttribute.DefaultExpanded;
            }

            bool unfold = property.isExpanded || _forceUnfold;

            // Debug.Log($"_unfold={_unfold}, _forceUnfold={_forceUnfold}, Event.current.type={Event.current.type}");

            if (!unfold)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            int[] values = Enum.GetValues(info.FieldType).Cast<int>().ToArray();
            int allOnValue = values.Aggregate(0, (acc, value) => acc | value);
            int valueCount = values.Count(each => each != 0 && each != allOnValue);
            return EditorGUIUtility.singleLineHeight * (valueCount + 1);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if (!_initExpandState)
            {
                _initExpandState = true;
                property.isExpanded = enumFlagsAttribute.DefaultExpanded;
            }

            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            #region label+button
            Rect headRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            float labelWidth = string.IsNullOrEmpty(label.text)? 0: EditorGUIUtility.labelWidth;

            CheckAutoExpand(position.width - labelWidth, property, info, enumFlagsAttribute);
            if (_forceUnfold)
            {
                EditorGUI.LabelField(headRect, label);
            }
            else
            {
                using(new GUIEnabledScoop(true))
                {
                    property.isExpanded = EditorGUI.Foldout(headRect, property.isExpanded, label);
                }
            }

            Rect fieldRect = RectUtils.SplitWidthRect(position, labelWidth).leftRect;

            // Rect leftRect = new Rect(headRect)
            // {
            //     x = headRect.x + labelWidth,
            //     width = headRect.width - labelWidth,
            // };

            bool noneChecked = property.intValue == 0;
            bool allChecked = property.intValue == metaInfo.AllCheckedInt;

            // Debug.Log($"property.intValue = {property.intValue}; noneChecked={noneChecked}, allChecked={allChecked}");

            bool useUnfold = property.isExpanded || _forceUnfold;

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
                        Action = () => property.intValue = metaInfo.AllCheckedInt,
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
                        Action = () => property.intValue = metaInfo.AllCheckedInt,
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
                foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> kv in metaInfo.BitValueToName.Where(each => each.Key != 0 && each.Key != metaInfo.AllCheckedInt))
                {
                    int value = kv.Key;
                    string name = kv.Value.HasRichName? kv.Value.RichName: kv.Value.Name;

                    bool on = EnumFlagsUtil.isOn(curValue, value);
                    GUIContent btnLabel = new GUIContent(name);
                    // GUIStyle btnStyle = on ? activeBtn : normalBtn;
                    btnInfos.Add(new BtnInfo
                    {
                        Label = btnLabel,
                        LabelStyle = _miniButtonStyle,
                        LabelWidth = _miniButtonStyle.CalcSize(btnLabel).x,
                        Action = () => property.intValue = EnumFlagsUtil.ToggleBit(property.intValue, value),
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
                foreach ((int value, string name, int index) in metaInfo.BitValueToName
                             .Where(each => each.Key != 0 && each.Key != metaInfo.AllCheckedInt)
                             .Select((each, index) => (each.Key, each.Value.HasRichName? each.Value.RichName: each.Value.Name, index)))
                {
                    bool on = EnumFlagsUtil.isOn(curValue, value);

                    // GUIStyle normalBtn = _normalButtonStyle;

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
                        if (GUI.Button(btnRect, $"{(on ? "☑" : "☐")} | {name}", _normalButtonStyle))
                        {
                            property.intValue = EnumFlagsUtil.ToggleBit(property.intValue, value);
                        }
                    }
                }
            }
        }

        private void CheckAutoExpand(float positionWidth, SerializedProperty property, FieldInfo info, EnumFlagsAttribute enumFlagsAttribute)
        {
            if (positionWidth - 1 <= Mathf.Epsilon)  // layout event will give this to negative... wait for repaint to do correct calculate
            {
                return;
            }

            _forceUnfold = false;

            if (property.isExpanded)
            {
                return;
            }

            if (!enumFlagsAttribute.AutoExpand)
            {
                return;
            }

            (int, string)[] allValues = Enum.GetValues(info.FieldType)
                .Cast<object>()
                .Select(each => ((int)each, each.ToString())).ToArray();

            int allCheckedInt = allValues.Select(each => each.Item1).Aggregate(0, (acc, value) => acc | value);
            IEnumerable<string> stringValues = allValues
                .Where(each => each.Item1 != 0 && each.Item1 != allCheckedInt)
                .Select(each => each.Item2);

            float totalBtnWidth = EditorGUIUtility.singleLineHeight + stringValues.Sum(each => _miniButtonStyle.CalcSize(new GUIContent(each)).x);

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
        #endregion
    }
}
