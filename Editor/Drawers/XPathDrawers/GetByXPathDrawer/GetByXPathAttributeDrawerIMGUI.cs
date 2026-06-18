using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private sealed class GetByXPathStatusIMGUI
        {
            public UnityAction ProjectChangedHandler;
        }

        private static readonly Dictionary<string, GetByXPathStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, GetByXPathStatusIMGUI>();

        private static GUIStyle _imageButtonStyle;
        private static Texture2D _refreshIcon;
        private static Texture2D _removeIcon;
        private static Texture2D _pickerIcon;
        private static GUIContent _refreshContent;
        private static GUIContent _removeContent;
        private static GUIContent _pickerContent;

        private static GUIStyle ImageButtonStyle => _imageButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(2, 2, 2, 2),
            imagePosition = ImagePosition.ImageOnly,
            alignment = TextAnchor.MiddleCenter,
        };

        private static GUIContent RefreshContent
        {
            get
            {
                _refreshIcon ??= Util.LoadResource<Texture2D>("refresh.png");
                return _refreshContent ??= new GUIContent(_refreshIcon, "Sign XPath value");
            }
        }

        private static GUIContent RemoveContent
        {
            get
            {
                _removeIcon ??= Util.LoadResource<Texture2D>("close.png");
                return _removeContent ??= new GUIContent(_removeIcon, "Sign null");
            }
        }

        private static GUIContent PickerContent
        {
            get
            {
                _pickerIcon ??= EditorGUIUtility.IconContent("d_pick_uielements").image as Texture2D;
                return _pickerContent ??= new GUIContent(_pickerIcon, "Select");
            }
        }

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !(saintsAttribute is GetByXPathAttribute getByXPathAttribute))
            {
                return 0;
            }

            float pickerWidth = getByXPathAttribute.UsePickerButton ? SingleLineHeight : 0;
            if (!TryGetKey(property, out string key))
            {
                return 0;
            }

            if (!SharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache))
            {
                return pickerWidth;
            }

            if (!IsFirstAttribute(genericCache, saintsAttribute))
            {
                return 0;
            }

            if (NothingSigner(getByXPathAttribute))
            {
                return pickerWidth;
            }

            if (genericCache.Error != "")
            {
                return pickerWidth;
            }

            if (!TryGetPropertyIndex(property, out int propertyIndex) ||
                !genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache))
            {
                return pickerWidth;
            }

            if (propertyCache.Error != "" || !propertyCache.MisMatch || !getByXPathAttribute.UseResignButton)
            {
                return pickerWidth;
            }

            return pickerWidth + SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }

            (string keyError, bool cacheCreated, GetByXPathGenericCache genericCache) =
                EnsureKey(property, allAttributes);
            if (keyError != "" || genericCache == null)
            {
                return false;
            }

            if (!IsFirstAttribute(genericCache, saintsAttribute))
            {
                return false;
            }

            if (!TryGetPropertyIndex(property, out int propertyIndex))
            {
                return false;
            }

            bool needsRefresh = cacheCreated ||
                                !genericCache.IndexToPropertyCache.ContainsKey(propertyIndex) ||
                                genericCache.UpdateResourceAfterTime > EditorApplication.timeSinceStartup;
            if (needsRefresh)
            {
                RefreshSharedCache(genericCache, property, info, cacheCreated);
            }

            if (genericCache.Error != "" ||
                !genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache) ||
                propertyCache.Error != "")
            {
                return false;
            }

            GetByXPathAttribute firstAttr = genericCache.GetByXPathAttributes[0];
            Rect pickerRect = position;
            bool willDraw = false;

            if (!NothingSigner(firstAttr) && propertyCache.MisMatch && firstAttr.UseResignButton)
            {
                willDraw = true;
                (Rect actionButtonRect, Rect remainingRect) = RectUtils.SplitWidthRect(position, SingleLineHeight);
                pickerRect = remainingRect;
                if (GUI.Button(actionButtonRect,
                        propertyCache.TargetIsNull ? RemoveContent : RefreshContent,
                        ImageButtonStyle))
                {
                    SignPropertyAndNotify(property, propertyCache);
                }
            }

            if (DrawPicker(firstAttr, genericCache, pickerRect, property, propertyCache, info))
            {
                willDraw = true;
            }

            return willDraw;
        }

        private static bool TryGetKey(SerializedProperty property, out string key)
        {
            try
            {
                key = SerializedUtils.GetUniqueIdArray(property);
                return true;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {
            }

            key = "";
            return false;
        }

        private static bool TryGetPropertyIndex(SerializedProperty property, out int propertyIndex)
        {
            try
            {
                propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                return true;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {
            }

            propertyIndex = -1;
            return false;
        }

        private static bool IsFirstAttribute(GetByXPathGenericCache cache, ISaintsAttribute saintsAttribute)
        {
            return cache.GetByXPathAttributes != null &&
                   cache.GetByXPathAttributes.Count > 0 &&
                   ReferenceEquals(cache.GetByXPathAttributes[0], saintsAttribute);
        }

        private static (string error, bool created, GetByXPathGenericCache cache) EnsureKey(
            SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes)
        {
            if (!TryGetKey(property, out string key))
            {
                return ("Property is disposed", false, null);
            }

            bool created = false;
            if (!SharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache))
            {
                SharedCache[key] = genericCache = new GetByXPathGenericCache
                {
                    Error = "",
                    GetByXPathAttributes = allAttributes.OfType<GetByXPathAttribute>().ToArray(),
                    UpdateResourceAfterTime = double.MinValue,
                };
                created = true;
            }
            else if (genericCache.GetByXPathAttributes == null || genericCache.GetByXPathAttributes.Count == 0)
            {
                genericCache.GetByXPathAttributes = allAttributes.OfType<GetByXPathAttribute>().ToArray();
            }

            EnsureWatcher(property, key);
            return ("", created, genericCache);
        }

        private static void EnsureWatcher(SerializedProperty property, string key)
        {
            if (InfoCacheIMGUI.ContainsKey(key))
            {
                return;
            }

            GetByXPathStatusIMGUI status = new GetByXPathStatusIMGUI();
            void ProjectChangedHandler()
            {
                if (!SharedCache.TryGetValue(key, out GetByXPathGenericCache cache))
                {
                    return;
                }

                double curTime = EditorApplication.timeSinceStartup;
                if (cache.UpdateResourceAfterTime <= curTime)
                {
                    cache.UpdateResourceAfterTime = EditorApplication.timeSinceStartup + 0.5;
                }
            }

            status.ProjectChangedHandler = ProjectChangedHandler;
            InfoCacheIMGUI[key] = status;
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(ProjectChangedHandler);

            NoLongerInspectingWatch(property.serializedObject.targetObject, $"{key}__GetByXPath_IMGUI", () =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(status.ProjectChangedHandler);
                InfoCacheIMGUI.Remove(key);
                SharedCache.Remove(key);
            });
        }

        private void RefreshSharedCache(GetByXPathGenericCache genericCache, SerializedProperty property,
            FieldInfo info, bool isFirstTime)
        {
            UpdateSharedCacheBase(genericCache, property, info);
            genericCache.UpdateResourceAfterTime = double.MinValue;
            if (genericCache.Error != "")
            {
                return;
            }

            UpdateSharedCacheSource(genericCache, property, info);
            UpdateSharedCacheSetValue(genericCache, isFirstTime, property);
        }

        private bool DrawPicker(GetByXPathAttribute firstAttr, GetByXPathGenericCache genericCache, Rect position,
            SerializedProperty property, PropertyCache propertyCache, FieldInfo info)
        {
            if (!firstAttr.UsePickerButton)
            {
                return false;
            }

            if (GUI.Button(position, PickerContent, ImageButtonStyle))
            {
                OpenPicker(property, info, genericCache.GetByXPathAttributes,
                    genericCache.ExpectedType, genericCache.ExpectedInterface,
                    newValue => SignPickedValue(property, propertyCache, newValue),
                    propertyCache.Parent);
            }

            return true;
        }

        private void SignPickedValue(SerializedProperty property, PropertyCache propertyCache, object newValue)
        {
            object oldValue = propertyCache.TargetValue;
            if (Util.GetIsEqual(oldValue, newValue))
            {
                return;
            }

            propertyCache.TargetValue = newValue;
            propertyCache.TargetIsNull = RuntimeUtil.IsNull(newValue);
            if (DoSignPropertyCache(propertyCache, false))
            {
                propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                TriggerChangedIMGUI(property, newValue);
                return;
            }

            propertyCache.TargetValue = oldValue;
            propertyCache.TargetIsNull = RuntimeUtil.IsNull(oldValue);
        }

        private void SignPropertyAndNotify(SerializedProperty property, PropertyCache propertyCache)
        {
            if (!DoSignPropertyCache(propertyCache, false))
            {
                return;
            }

            propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
            TriggerChangedIMGUI(property, propertyCache.TargetValue);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            string content = GetBelowMessage(property, saintsAttribute);
            return !string.IsNullOrEmpty(content);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent)
        {
            string content = GetBelowMessage(property, saintsAttribute);
            return string.IsNullOrEmpty(content)
                ? 0
                : ImGuiHelpBox.GetHeight(content, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            string content = GetBelowMessage(property, saintsAttribute);
            return string.IsNullOrEmpty(content)
                ? position
                : ImGuiHelpBox.Draw(position, content, MessageType.Error);
        }

        private static string GetBelowMessage(SerializedProperty property,
            ISaintsAttribute saintsAttribute)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "";
            }

            if (!TryGetKey(property, out string key) ||
                !SharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache))
            {
                return "";
            }

            if (genericCache.Error != "")
            {
                return genericCache.Error;
            }

            if (!IsFirstAttribute(genericCache, saintsAttribute) ||
                !TryGetPropertyIndex(property, out int propertyIndex) ||
                !genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache))
            {
                return "";
            }

            if (propertyCache.Error != "")
            {
                return propertyCache.Error;
            }

            if(propertyCache.MisMatch && genericCache.GetByXPathAttributes[0].UseErrorMessage)
            {
                return GetMismatchErrorMessage(propertyCache.OriginalValue, propertyCache.TargetValue, propertyCache.TargetIsNull);
            }

            return "";
        }

    }
}
