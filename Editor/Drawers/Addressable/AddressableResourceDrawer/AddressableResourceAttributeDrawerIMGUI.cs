using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
{
    public partial class AddressableResourceAttributeDrawer
    {
        private class LabelPopupWindow: PopupWindowContent
        {
            private readonly IReadOnlyList<string> _options;
            private readonly Action<IReadOnlyList<string>> _onOk;

            private List<string> _curSelected;

            public LabelPopupWindow(IReadOnlyList<string> curSelected, IReadOnlyList<string> options, Action<IReadOnlyList<string>> onOk)
            {
                _options = options;
                _onOk = onOk;
                _curSelected = curSelected.ToList();
            }

            public override Vector2 GetWindowSize()
            {
                int optionCount = _options.Count;
                return new Vector2(
                    250,
                    (_options.Count + 1) * SingleLineHeight
                    + (optionCount > 0? 15: 0)
                    + 10f
                );
            }

            public override void OnGUI(Rect rect)
            {
                bool isAllSelected = _curSelected.Count == _options.Count;
                using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    bool toggleAll = EditorGUILayout.ToggleLeft("All", isAllSelected);
                    if (changed.changed)
                    {
                        if (toggleAll)
                        {
                            _curSelected = _options.ToList();
                        }
                        else
                        {
                            _curSelected.Clear();
                        }
                    }
                }

                if (_options.Count > 0)
                {
                    Rect line = EditorGUILayout.GetControlRect(false, 1);
                    EditorGUI.DrawRect(line, EColor.EditorSeparator.GetColor());
                }

                foreach (string option in _options)
                {
                    bool curSelected = _curSelected.Contains(option);
                    bool newSelected = EditorGUILayout.ToggleLeft(option, curSelected);
                    if (newSelected != curSelected)
                    {
                        if (newSelected)
                        {
                            _curSelected.Add(option);
                        }
                        else
                        {
                            _curSelected.Remove(option);
                        }
                    }
                }

                if (GUILayout.Button("OK"))
                {
                    _onOk(_curSelected);
                    editorWindow.Close();
                }
            }
        }

        private static Texture2D _editIcon;
        private static Texture2D EditIcon
        {
            get
            {
                if (_editIcon == null)
                {
                    _editIcon = Util.LoadResource<Texture2D>("pencil.png");
                }

                return _editIcon;
            }
        }

        private static Texture2D _saveIcon;
        private static Texture2D SaveIcon
        {
            get
            {
                if (_saveIcon == null)
                {
                    _saveIcon = Util.LoadResource<Texture2D>("save.png");
                }

                return _saveIcon;
            }
        }

        private static Texture2D _trashIcon;
        private static Texture2D TrashIcon
        {
            get
            {
                if (_trashIcon == null)
                {
                    _trashIcon = Util.LoadResource<Texture2D>("trash.png");
                }

                return _trashIcon;
            }
        }

        private static Texture2D _checkIcon;
        private static Texture2D CheckIcon
        {
            get
            {
                if (_checkIcon == null)
                {
                    _checkIcon = Util.LoadResource<Texture2D>("check.png");
                }

                return _checkIcon;
            }
        }

        private static Texture2D _closeIcon;
        private static Texture2D CloseIcon
        {
            get
            {
                if (_closeIcon == null)
                {
                    _closeIcon = Util.LoadResource<Texture2D>("close.png");
                }

                return _closeIcon;
            }
        }

        private static GUIStyle _buttonStyle;
        private static GUIStyle ButtonStyle
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        padding = new RectOffset(3, 3, 3, 3),
                    };
                }

                return _buttonStyle;
            }
        }

        private class InfoIMGUI
        {
            public bool Expanded;

            public bool IsSprite;
            public string GroupName;
            public string[] Labels;
            public NameType NameType;
            public string Name = "";

            public Object CurObject;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheImGui = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureInfo(string key, SerializedProperty property, FieldInfo info)
        {
            if (InfoCacheImGui.TryGetValue(key, out InfoIMGUI ensureInfo))
            {
                return ensureInfo;
            }

            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            bool isSprite = typeof(AssetReferenceSprite).IsAssignableFrom(fieldType);

            return InfoCacheImGui[key] = new InfoIMGUI
            {
                IsSprite = isSprite,
                Labels = Array.Empty<string>(),
            };
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoCacheImGui.Remove(key);
            });
            // EnsureInfo(key);

            return SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            InfoIMGUI cacheInfo = EnsureInfo(key, property, info);
            if (GUI.Button(position, EditIcon))
            {
                cacheInfo.Expanded = !cacheInfo.Expanded;
            }

            return cacheInfo.Expanded;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
            {
                return ImGuiHelpBox.GetHeight(AddressableUtil.ErrorNoSettings, width, MessageType.Error);
            }

            string key = SerializedUtils.GetUniqueId(property);
            InfoIMGUI cacheInfo = EnsureInfo(key, property, info);
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
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                return ImGuiHelpBox.Draw(position, AddressableUtil.ErrorNoSettings, MessageType.Error);
            }

            string key = SerializedUtils.GetUniqueId(property);
            InfoIMGUI cacheInfo = EnsureInfo(key, property, info);
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
                    if (cacheInfo.CurObject == null || onGuiPayload.changed)
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
                if (changed.changed || onGuiPayload.changed)
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
                AdvancedDropdownList<string> dropdownListValue = new AdvancedDropdownList<string>("Select a Group");
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

                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownListValue,
                    AdvancedDropdownUtil.GetSizeIMGUI(dropdownListValue, groupRow.width),
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        cacheInfo.GroupName = (string)curItem;
                    },
                    _ => null);

                dropdown.Show(groupRow);
                dropdown.BindWindowPosition();
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
                LabelPopupWindow labelPopupWindow = new LabelPopupWindow(cacheInfo.Labels, settings.GetLabels(), newLabels =>
                {
                    // Debug.Log($"labels: {string.Join(", ", newLabels)}");
                    cacheInfo.Labels = newLabels.ToArray();
                });
                PopupWindow.Show(labelRowButton, labelPopupWindow);
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
                if (GUI.Button(saveButton, SaveIcon, ButtonStyle))
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
                if(GUI.Button(deleteButton, TrashIcon, ButtonStyle))
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
                if (GUI.Button(okButton, CheckIcon, ButtonStyle))
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
                    onGuiPayload.SetValue(cacheInfo.CurObject);

                    cacheInfo.Expanded = false;
                }
            }

            if (GUI.Button(closeButton, CloseIcon, ButtonStyle))
            {
                cacheInfo.Expanded = false;
            }

            return buttonRowRectLeftSpace;
        }
    }
}
