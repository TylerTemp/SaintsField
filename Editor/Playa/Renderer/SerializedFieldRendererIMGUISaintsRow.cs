using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
            if(listDrawerSettingsAttribute is null)
            {
                return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
            }

            bool hasSearch = listDrawerSettingsAttribute.Searchable;
            bool hasPaging = listDrawerSettingsAttribute.NumberOfItemsPerPage > 0;
            // ArraySizeAttribute arraySizeAttribute = FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
            //
            // if(!hasSearch && !hasPaging && arraySizeAttribute == null)
            // {
            //     Debug.Log($"No IMGUIListInfo");
            //     _imGuiListInfo = null;
            //     return EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty, true);
            // }

            if (_imGuiListInfo == null)
            {
                int numberOfItemsPrePage = listDrawerSettingsAttribute.NumberOfItemsPerPage;
                _imGuiListInfo = new ImGuiListInfo
                {
                    Property = FieldWithInfo.SerializedProperty,
                    PreCheckResult = preCheckResult,
                    HasSearch = hasSearch,
                    HasPaging = hasPaging,
                    PagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, 0, "", numberOfItemsPrePage),
                    NumberOfItemsPrePage = numberOfItemsPrePage,
                    PageIndex = 0,
                    SearchText = "",
                };
                FieldWithInfo.SerializedProperty.isExpanded = true;
            }
            else
            {
                _imGuiListInfo.PagingInfo = GetPagingInfo(FieldWithInfo.SerializedProperty, _imGuiListInfo.PageIndex,
                    _imGuiListInfo.SearchText, _imGuiListInfo.NumberOfItemsPrePage);
            }

            if (!FieldWithInfo.SerializedProperty.isExpanded)
            {
                return SaintsPropertyDrawer.SingleLineHeight;
            }

            int extraLineCount = (hasSearch || hasPaging) ? 4 : 3;

            float height = _imGuiListInfo.PagingInfo.IndexesCurPage
                               .Select(index => EditorGUI.GetPropertyHeight(FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(index), true))
                               .Sum()
                           + (_imGuiListInfo.PagingInfo.IndexesCurPage.Count == 0? EditorGUIUtility.singleLineHeight: 0)
                           + SaintsPropertyDrawer.SingleLineHeight * extraLineCount;  // header with controller line, footer (plus, minus)
            return height;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            bool isArray = FieldWithInfo.SerializedProperty.isArray;
            OnArraySizeChangedAttribute onArraySizeChangedAttribute =
                FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            int arraySize = -1;
            if (isArray && onArraySizeChangedAttribute != null)
            {
                arraySize = FieldWithInfo.SerializedProperty.arraySize;
            }

            if (preCheckResult.ArraySize != -1 && FieldWithInfo.SerializedProperty.arraySize != preCheckResult.ArraySize)
            {
                FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();
                if(_imGuiListInfo != null && listDrawerSettingsAttribute != null)
                {
                    _imGuiListInfo.PreCheckResult = preCheckResult;
                    ArraySizeAttribute arraySizeAttribute = FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault();
                    // ReSharper disable once ConvertToUsingDeclaration
                    using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                    {
                        DrawListDrawerSettingsField(FieldWithInfo.SerializedProperty, position, arraySizeAttribute, listDrawerSettingsAttribute.Delayed);
                        if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                           arraySize != FieldWithInfo.SerializedProperty.arraySize)
                        {
                            FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                            InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                                FieldWithInfo.SerializedProperty,
                                (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                        }
                    }
                    return;
                }

                GUIContent useGUIContent = preCheckResult.HasRichLabel
                    ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length), tooltip: FieldWithInfo.SerializedProperty.tooltip)
                    : new GUIContent(FieldWithInfo.SerializedProperty.displayName, tooltip: FieldWithInfo.SerializedProperty.tooltip);

                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.PropertyField(position, FieldWithInfo.SerializedProperty, useGUIContent, true);
                    if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                       arraySize != FieldWithInfo.SerializedProperty.arraySize)
                    {
                        FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                        InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                            FieldWithInfo.SerializedProperty,
                            (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                    }
                }

                #region RichLabel
                if (preCheckResult.HasRichLabel)
                {
                    Rect richRect = new Rect(position)
                    {
                        height = SaintsPropertyDrawer.SingleLineHeight,
                    };

                    // EditorGUI.DrawRect(richRect, Color.blue);
                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if(_richTextDrawer == null)
                    {
                        _richTextDrawer = new RichTextDrawer();
                    }

                    // Debug.Log(preCheckResult.RichLabelXml);
                    if (_curXml != preCheckResult.RichLabelXml)
                    {
                        _curXmlChunks =
                            RichTextDrawer
                                .ParseRichXml(preCheckResult.RichLabelXml, FieldWithInfo.SerializedProperty.displayName, FieldWithInfo.FieldInfo, FieldWithInfo.Target)
                                .ToArray();
                    }

                    _curXml = preCheckResult.RichLabelXml;

                    _richTextDrawer.DrawChunks(richRect, new GUIContent(FieldWithInfo.SerializedProperty.displayName), _curXmlChunks);
                }
                #endregion
            }
            // EditorGUI.DrawRect(position, Color.blue);
        }
    }
}
