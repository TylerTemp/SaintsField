using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FolderDrawers.ResourcesFolderDrawer
{
    public partial class ResourceFolderAttributeDrawer
    {
        private static Texture2D _folderIcon;

        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool FreezeError;
            public string FrozenValue;
            public bool Initialized;
            public bool NeedRefresh = true;
            public UnityEngine.Events.UnityAction OnEditorChanged;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            infoCache.OnEditorChanged = () => infoCache.NeedRefresh = true;
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(infoCache.OnEditorChanged);
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(infoCache.OnEditorChanged);
                InfoCacheIMGUI.Remove(key);
            });
            return infoCache;
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => SingleLineHeight;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            UpdateStatus(cache, property);

            _folderIcon ??= Util.LoadResource<Texture2D>("resources-folder.png");
            Rect buttonRect = new Rect(position)
            {
                x = position.x + 1,
                width = position.width - 2,
            };

            if (property.propertyType == SerializedPropertyType.String)
            {
                ResourceFolderAttribute folderAttribute = (ResourceFolderAttribute)saintsAttribute;
                if (GUI.Button(buttonRect, _folderIcon, GUIStyle.none))
                {
                    (string error, string actualFolder) = OnClick(property, folderAttribute);
                    if (error == "")
                    {
                        if (actualFolder != "")
                        {
                            cache.Error = "";
                            cache.FreezeError = false;
                            cache.Initialized = true;
                            cache.NeedRefresh = false;
                            property.stringValue = actualFolder;
                            property.serializedObject.ApplyModifiedProperties();
                            TriggerChangedIMGUI(property, actualFolder);
                        }
                    }
                    else
                    {
                        cache.Error = error;
                        cache.FreezeError = true;
                        cache.FrozenValue = property.stringValue;
                    }
                }

                HandleDragAndDrop(fullRect, property, cache);
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Button(buttonRect, _folderIcon, GUIStyle.none);
                }
            }

            return true;
        }

        private static void HandleDragAndDrop(Rect fullRect, SerializedProperty property, InfoIMGUI cache)
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            if (!fullRect.Contains(evt.mousePosition))
            {
                return;
            }

            string fineFolder = CanDrop(DragAndDrop.objectReferences).FirstOrDefault();
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.None;
                if (string.IsNullOrEmpty(fineFolder))
                {
                    return;
                }

                DragAndDrop.AcceptDrag();
                cache.Error = "";
                cache.FreezeError = false;
                cache.Initialized = true;
                cache.NeedRefresh = false;
                property.stringValue = fineFolder;
                property.serializedObject.ApplyModifiedProperties();
                TriggerChangedIMGUI(property, fineFolder);
            }
            else
            {
                DragAndDrop.visualMode = string.IsNullOrEmpty(fineFolder)
                    ? DragAndDropVisualMode.Rejected
                    : DragAndDropVisualMode.Copy;
            }

            evt.Use();
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            UpdateStatus(cache, property);
            return cache.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            UpdateStatus(cache, property);
            return cache.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cache.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cache = EnsureKey(property);
            UpdateStatus(cache, property);
            return cache.Error == "" ? position : ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
        }

        private static void UpdateStatus(InfoIMGUI cache, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                cache.FreezeError = false;
                cache.Error = $"Target is not a string: {property.propertyType}";
                return;
            }

            if (cache.FreezeError && cache.FrozenValue == property.stringValue)
            {
                return;
            }

            if (cache.Initialized && !cache.NeedRefresh)
            {
                return;
            }

            cache.FreezeError = false;
            cache.Initialized = true;
            cache.NeedRefresh = false;
            cache.Error = CheckFolder(property.stringValue);
        }
    }
}
