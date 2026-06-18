using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
{
    public partial class AddressableResourceAttributeDrawer
    {
        private class InfoIMGUI
        {
            public bool Expanded;

            public bool IsSprite;
            public string GroupName;
            public string[] Labels;
            public NameType NameType;
            public string Name = "";

            public Object CurObject;

            public Texture2D EditIcon;
            public Texture2D SaveIcon;
            public Texture2D TrashIcon;
            public Texture2D CheckIcon;
            public Texture2D CloseIcon;
            public GUIStyle ButtonStyle;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, FieldInfo info)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI ensureInfo))
            {
                return ensureInfo;
            }

            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            bool isSprite = typeof(AssetReferenceSprite).IsAssignableFrom(fieldType);

            InfoCacheIMGUI[key] = ensureInfo = new InfoIMGUI
            {
                IsSprite = isSprite,
                Labels = Array.Empty<string>(),
            };
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return ensureInfo;
        }

        private static GUIStyle GetButtonStyle(InfoIMGUI cache)
        {
            return cache.ButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(3, 3, 3, 3),
            };
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            EnsureKey(property, info);
            return SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cacheInfo = EnsureKey(property, info);
            cacheInfo.EditIcon ??= Util.LoadResource<Texture2D>("pencil.png");
            if (GUI.Button(position, cacheInfo.EditIcon))
            {
                cacheInfo.Expanded = !cacheInfo.Expanded;
            }

            return cacheInfo.Expanded;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
            {
                return ImGuiHelpBox.GetHeight(AddressableUtil.ErrorNoSettings, width, MessageType.Error);
            }

            InfoIMGUI cacheInfo = EnsureKey(property, info);
            if (!cacheInfo.Expanded)
            {
                return 0;
            }

            return cacheInfo.IsSprite
                ? SingleLineHeight * 4 + 64
                : SingleLineHeight * 5;
            // return cacheInfo.Expanded ? SingleLineHeight * 4 : 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                return ImGuiHelpBox.Draw(position, AddressableUtil.ErrorNoSettings, MessageType.Error);
            }

            InfoIMGUI cacheInfo = EnsureKey(property, info);
            if (!cacheInfo.Expanded)
            {
                return position;
            }

            EditorGUI.DrawRect(position, EColor.EditorEmphasized.GetColor());

            Object currentObj = null;
            string guid = property.FindPropertyRelative("m_AssetGUID").stringValue;
            if (!string.IsNullOrEmpty(guid))
            {
                Object loadObj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                if (loadObj != null)
                {
                    currentObj = loadObj;
                    if (cacheInfo.CurObject == null)
                    {
                        cacheInfo.CurObject = currentObj;
                    }
                }
            }

            // resource object
            (Rect resourceRowRawRect, Rect resourceRowRectLeftSpace) = RectUtils.SplitHeightRect(position, cacheInfo.IsSprite? 64: SingleLineHeight);
            Rect resourceRow = new Rect(resourceRowRawRect)
            {
                x = resourceRowRawRect.x + 1,
                y = resourceRowRawRect.y + 1,
                height = resourceRowRawRect.height - 2,
            };

            AddressableAssetEntry entry = null;
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object inspectingTarget = cacheInfo.CurObject != null? cacheInfo.CurObject: currentObj;
                inspectingTarget =
                    EditorGUI.ObjectField(resourceRow, new GUIContent("Resource"), inspectingTarget, cacheInfo.IsSprite ? typeof(Sprite) : typeof(Object), false);
                entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(inspectingTarget)));
                if (changed.changed)
                {
                    cacheInfo.CurObject = inspectingTarget;
                    cacheInfo.GroupName = entry == null ? "" : entry.parentGroup.Name;
                    cacheInfo.Labels = entry == null ? Array.Empty<string>() : entry.labels.ToArray();
                    if (inspectingTarget == null)
                    {
                        cacheInfo.Name = "";
                    }
                    else
                    {
                        cacheInfo.Name = entry == null ? GetObjectName(cacheInfo.NameType, inspectingTarget) : entry.address;
                    }
                }
            }

            // AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(curObj)).ToString());

            // group
            (Rect groupRowRawRect, Rect groupRowRectLeftSpace) = RectUtils.SplitHeightRect(resourceRowRectLeftSpace, SingleLineHeight);
            Rect groupRow = new Rect(groupRowRawRect)
            {
                y = groupRowRawRect.y + 1,
                height = groupRowRawRect.height - 2,
            };
            Rect groupRowButton = EditorGUI.PrefixLabel(groupRow, new GUIContent("Group"));
            if (EditorGUI.DropdownButton(groupRowButton, new GUIContent(cacheInfo.GroupName), FocusType.Keyboard))
            {
                Dropdown<string> dropdownListValue = new Dropdown<string>("Select a Group");
                string[] curValues = settings.groups.Select(each => each.Name).ToArray();

                dropdownListValue.Add("None", "");
                if (curValues.Length > 0)
                {
                    dropdownListValue.AddSeparator();
                    foreach (string curValue in curValues)
                    {
                        dropdownListValue.Add(curValue, curValue);
                    }
                }

                AdvancedDropdownMetaInfo groupMetaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurValues = string.IsNullOrEmpty(cacheInfo.GroupName)
                        ? Array.Empty<object>()
                        : new object[] { cacheInfo.GroupName },
                    DropdownListValue = dropdownListValue,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                PopupWindow.Show(groupRowButton, new SaintsTreeDropdownIMGUI(
                    groupMetaInfo,
                    Mathf.Max(groupRowButton.width, 220f),
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        cacheInfo.GroupName = (string)curItem;
                        return null;
                    }));
            }

            // label
            (Rect labelRowRawRect, Rect labelRowRectLeftSpace) = RectUtils.SplitHeightRect(groupRowRectLeftSpace, SingleLineHeight);
            Rect labelRow = new Rect(labelRowRawRect)
            {
                y = labelRowRawRect.y + 1,
                height = labelRowRawRect.height - 2,
            };
            Rect labelRowButton = EditorGUI.PrefixLabel(labelRow, new GUIContent("Labels"));
            // ReSharper disable once InvertIf
            if (EditorGUI.DropdownButton(labelRowButton, new GUIContent(string.Join(",", cacheInfo.Labels)), FocusType.Keyboard))
            {
                Dropdown<string> labelDropdown = new Dropdown<string>("Select Labels");
                foreach (string eachLabel in settings.GetLabels())
                {
                    labelDropdown.Add(eachLabel, eachLabel);
                }

                AdvancedDropdownMetaInfo labelMetaInfo = new AdvancedDropdownMetaInfo
                {
                    Error = "",
                    CurValues = cacheInfo.Labels.Cast<object>().ToArray(),
                    DropdownListValue = labelDropdown,
                    SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                };

                PopupWindow.Show(labelRowButton, new SaintsTreeDropdownIMGUI(
                    labelMetaInfo,
                    Mathf.Max(labelRowButton.width, 220f),
                    320f,
                    true,
                    (curItem, isOn) =>
                    {
                        string selectedLabel = (string)curItem;
                        cacheInfo.Labels = isOn
                            ? cacheInfo.Labels.Append(selectedLabel).Distinct().ToArray()
                            : cacheInfo.Labels.Where(each => each != selectedLabel).ToArray();
                        return cacheInfo.Labels.Cast<object>().ToArray();
                    }));
            }

            // file name
            (Rect fileNameRowRawRect, Rect fileNameRowRectLeftSpace) = RectUtils.SplitHeightRect(labelRowRectLeftSpace, SingleLineHeight);
            Rect fileNameRow = new Rect(fileNameRowRawRect)
            {
                y = fileNameRowRawRect.y + 1,
                height = fileNameRowRawRect.height - 2,
            };
            (Rect fileNameRect, Rect fileNameButtonRect) = RectUtils.SplitWidthRect(fileNameRow, fileNameRow.width - SingleLineHeight);
            using(new EditorGUI.DisabledScope(cacheInfo.CurObject == null))
            {
                cacheInfo.Name = EditorGUI.TextField(fileNameRect, "Key", cacheInfo.Name);
                if (EditorGUI.DropdownButton(fileNameButtonRect, new GUIContent(""), FocusType.Keyboard))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    foreach (NameType nameType in Enum.GetValues(typeof(NameType)).Cast<NameType>())
                    {
                        genericMenu.AddItem(new GUIContent(nameType.ToFriendlyString()), false, () =>
                        {
                            cacheInfo.NameType = nameType;
                            cacheInfo.Name = GetObjectName(nameType, cacheInfo.CurObject);
                        });
                    }
                    genericMenu.DropDown(fileNameButtonRect);
                }
            }

            // button
            (Rect buttonRowRawRect, Rect buttonRowRectLeftSpace) = RectUtils.SplitHeightRect(fileNameRowRectLeftSpace, SingleLineHeight);
            Rect saveButton = new Rect
            {
                x = buttonRowRawRect.x + 1,
                y = buttonRowRawRect.y + 1,
                width = SingleLineHeight - 2,
                height = SingleLineHeight - 2,
            };
            using(new EditorGUI.DisabledScope(cacheInfo.CurObject == null || string.IsNullOrEmpty(cacheInfo.GroupName)))
            {
                cacheInfo.SaveIcon ??= Util.LoadResource<Texture2D>("save.png");
                if (GUI.Button(saveButton, cacheInfo.SaveIcon, GetButtonStyle(cacheInfo)))
                {
                    if (cacheInfo.CurObject != null)
                    {
                        string groupName = cacheInfo.GroupName;
                        AddressableAssetGroup group = settings.groups.FirstOrDefault(each => each.Name == groupName);
                        string curGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(cacheInfo.CurObject));
                        AddressableAssetEntry saveEntry = settings.CreateOrMoveEntry(curGuid, group);
                        saveEntry.address = cacheInfo.Name;
                        foreach (string eachLabel in settings.GetLabels())
                        {
                            saveEntry.SetLabel(eachLabel, cacheInfo.Labels.Contains(eachLabel));
                        }
                    }
                }
            }

            Rect deleteButton = new Rect
            {
                x = buttonRowRawRect.x + SingleLineHeight + 1,
                y = buttonRowRawRect.y + 1,
                width = SingleLineHeight - 2,
                height = SingleLineHeight - 2,
            };
            using (new EditorGUI.DisabledScope(entry == null))
            {
                cacheInfo.TrashIcon ??= Util.LoadResource<Texture2D>("trash.png");
                if(GUI.Button(deleteButton, cacheInfo.TrashIcon, GetButtonStyle(cacheInfo)))
                {
                    settings.RemoveAssetEntry(entry.guid);
                }
            }

            Rect last2ButtonsRect = new Rect(buttonRowRawRect)
            {
                x = buttonRowRawRect.x + buttonRowRawRect.width - SingleLineHeight * 2,
                width = SingleLineHeight * 2,
            };
            (Rect okButton, Rect closeButton) = RectUtils.SplitWidthRect(last2ButtonsRect, SingleLineHeight);
            using(new EditorGUI.DisabledScope(cacheInfo.CurObject == null || string.IsNullOrEmpty(cacheInfo.GroupName)))
            {
                cacheInfo.CheckIcon ??= Util.LoadResource<Texture2D>("check.png");
                if (GUI.Button(okButton, cacheInfo.CheckIcon, GetButtonStyle(cacheInfo)))
                {
                    string groupName = cacheInfo.GroupName;
                    AddressableAssetGroup group = settings.groups.FirstOrDefault(each => each.Name == groupName);
                    string curGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(cacheInfo.CurObject));
                    AddressableAssetEntry saveEntry = settings.CreateOrMoveEntry(curGuid, group);
                    saveEntry.address = cacheInfo.Name;
                    foreach (string eachLabel in settings.GetLabels())
                    {
                        saveEntry.SetLabel(eachLabel, cacheInfo.Labels.Contains(eachLabel));
                    }

                    property.FindPropertyRelative("m_AssetGUID").stringValue = guid;

                    // sub asset
                    if (cacheInfo.IsSprite && cacheInfo.CurObject is Sprite sprite)
                    {
                        property.FindPropertyRelative("m_SubObjectName").stringValue = sprite.name;
                        property.FindPropertyRelative("m_SubObjectType").stringValue = typeof(Sprite).AssemblyQualifiedName;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                    TriggerChangedIMGUI(property, cacheInfo.CurObject);

                    cacheInfo.Expanded = false;
                }
            }

            cacheInfo.CloseIcon ??= Util.LoadResource<Texture2D>("close.png");
            if (GUI.Button(closeButton, cacheInfo.CloseIcon, GetButtonStyle(cacheInfo)))
            {
                cacheInfo.Expanded = false;
            }

            return buttonRowRectLeftSpace;
        }
    }
}
