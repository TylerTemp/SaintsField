using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using SaintsField.Editor.Playa.RendererGroup;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            bool isArray = FieldWithInfo.SerializedProperty.isArray;
            OnArraySizeChangedAttribute onArraySizeChangedAttribute =
                FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            int arraySize = -1;
            if (isArray && onArraySizeChangedAttribute != null)
            {
                arraySize = FieldWithInfo.SerializedProperty.arraySize;
            }

            if (preCheckResult.ArraySize != -1 && (
                    (preCheckResult.ArraySize == 0 && FieldWithInfo.SerializedProperty.arraySize > 0)
                    || (preCheckResult.ArraySize > 0 && FieldWithInfo.SerializedProperty.arraySize == 0)
                ))
            {
                FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

            ListDrawerSettingsAttribute listDrawerSettingsAttribute = FieldWithInfo.PlayaAttributes.OfType<ListDrawerSettingsAttribute>().FirstOrDefault();

            // bool hasSearch = listDrawerSettingsAttribute?.Searchable ?? false;
            // bool hasPaging = listDrawerSettingsAttribute?.NumberOfItemsPerPage > 0;

            // if(hasSearch || hasPaging || arraySizeAttribute != null)
            if(listDrawerSettingsAttribute != null)
            {
                Rect rect = EditorGUILayout.GetControlRect(true, 0f);
                float listDrawerHeight = GetHeightIMGUI(rect.width);
                Rect position = GUILayoutUtility.GetRect(0, listDrawerHeight);
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
                ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length))
                : new GUIContent(FieldWithInfo.SerializedProperty.displayName);

            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool needIndent = SaintsRendererGroup.IMGUINeedIndentFix && (FieldWithInfo.SerializedProperty.isArray ||
                                                                             FieldWithInfo.SerializedProperty
                                                                                 .propertyType ==
                                                                             SerializedPropertyType.Generic);
                using(new EditorGUI.IndentLevelScope(needIndent? 1: 0))
                {
                    TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
                    if (tableAttribute == null)
                    {
                        EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, useGUIContent,
                            GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        arraySize = FieldWithInfo.SerializedProperty.arraySize;
                        if (arraySize == 0)
                        {
                            EditorGUILayout.PropertyField(FieldWithInfo.SerializedProperty, useGUIContent,
                                GUILayout.ExpandWidth(true));
                        }
                        else
                        {
                            FieldWithInfo.SerializedProperty.isExpanded =
                                EditorGUILayout.Foldout(FieldWithInfo.SerializedProperty.isExpanded, useGUIContent);
                            if(FieldWithInfo.SerializedProperty.isExpanded)
                            {
                                EditorGUILayout.PropertyField(
                                    FieldWithInfo.SerializedProperty.GetArrayElementAtIndex(0), useGUIContent,
                                    GUILayout.ExpandWidth(true));
                            }
                        }
                    }

                }

                if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                   arraySize != FieldWithInfo.SerializedProperty.arraySize)
                {
                    // Debug.Log("size changed");
                    FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                        FieldWithInfo.SerializedProperty,
                        (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                }
            }

            if (preCheckResult.HasRichLabel)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                // GUILayout.Label("Mouse over!");
                Rect richRect = new Rect(lastRect)
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                };
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
        }
    }
}
