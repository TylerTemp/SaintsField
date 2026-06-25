using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsRowDrawer
{
    public partial class SaintsRowAttributeDrawer
    {
        public sealed class ManagedReferenceBodyInfo
        {
            public IReadOnlyList<ISaintsRenderer> Renderers = Array.Empty<ISaintsRenderer>();
#if UNITY_2021_3_OR_NEWER
            public long ManagedReferenceId = long.MinValue;
#endif
        }

        private class ImGuiCacheInfo
        {
            public string Error = "";
            public object Value;
            public ManagedReferenceBodyInfo BodyInfo = new ManagedReferenceBodyInfo();
        }

        private static readonly Dictionary<string, ImGuiCacheInfo> ImGuiCache =
            new Dictionary<string, ImGuiCacheInfo>();

        private float _filedWidthCache = -1;

        private static void DestroyRenderers(IReadOnlyList<ISaintsRenderer> renderers)
        {
            foreach (ISaintsRenderer saintsRenderer in renderers)
            {
                saintsRenderer.OnDestroy();
            }
        }

        public static void ClearManagedReferenceBody(ManagedReferenceBodyInfo bodyInfo)
        {
            DestroyRenderers(bodyInfo.Renderers);
            bodyInfo.Renderers = Array.Empty<ISaintsRenderer>();
#if UNITY_2021_3_OR_NEWER
            bodyInfo.ManagedReferenceId = long.MinValue;
#endif
        }

        public static void SyncManagedReferenceBody(ManagedReferenceBodyInfo bodyInfo, SerializedProperty property,
            FieldInfo info, object parent, object managedReferenceValue, SaintsPropertyDrawer makeRenderer)
        {
#if UNITY_2021_3_OR_NEWER
            long managedReferenceId = managedReferenceValue == null ? long.MinValue : property.managedReferenceId;
            if (bodyInfo.ManagedReferenceId != managedReferenceId)
            {
                ClearManagedReferenceBody(bodyInfo);
                bodyInfo.ManagedReferenceId = managedReferenceId;
            }
#endif

            if (managedReferenceValue == null)
            {
                return;
            }

            if (bodyInfo.Renderers.Count == 0)
            {
                Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                    .ToDictionary(each => each.name, each => each.property);
                int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                bodyInfo.Renderers = SaintsEditor.HelperGetRenderers(serializedFieldNames,
                    property.serializedObject, makeRenderer, parent, info, propertyIndex, new[] { managedReferenceValue });
            }

            foreach (ISaintsRenderer saintsRenderer in bodyInfo.Renderers)
            {
                saintsRenderer.SetSerializedProperty(property);
            }
        }

        public static float GetManagedReferenceBodyHeight(ManagedReferenceBodyInfo bodyInfo, float width)
        {
            float totalHeight = 0f;
            foreach (ISaintsRenderer saintsRenderer in bodyInfo.Renderers)
            {
                totalHeight += saintsRenderer.GetHeightIMGUI(width);
            }

            return totalHeight;
        }

        public static void DrawManagedReferenceBody(Rect position, ManagedReferenceBodyInfo bodyInfo)
        {
            float yAcc = position.y;
            foreach (ISaintsRenderer saintsRenderer in bodyInfo.Renderers)
            {
                float height = saintsRenderer.GetHeightIMGUI(position.width);
                Rect rect = new Rect(position)
                {
                    y = yAcc,
                    height = height,
                };
                saintsRenderer.RenderPositionIMGUI(rect);
                yAcc += height;
            }
        }

        private static void ClearCacheInfo(ImGuiCacheInfo cacheInfo)
        {
            ClearManagedReferenceBody(cacheInfo.BodyInfo);
            cacheInfo.Value = null;
            cacheInfo.Error = "";
        }

        private static void RemoveCache(string key)
        {
            if (!ImGuiCache.TryGetValue(key, out ImGuiCacheInfo cacheInfo))
            {
                return;
            }

            ClearCacheInfo(cacheInfo);
            ImGuiCache.Remove(key);
        }

        private ImGuiCacheInfo EnsureKey(FieldInfo info, SerializedProperty property)
        {
            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            string key = $"{SerializedUtils.GetUniqueId(property)}_{propertyIndex}";
            if (!ImGuiCache.TryGetValue(key, out ImGuiCacheInfo cacheInfo))
            {
                cacheInfo = new ImGuiCacheInfo();
                ImGuiCache[key] = cacheInfo;
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    RemoveCache(key);
                });
            }

#if UNITY_2021_3_OR_NEWER
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                long managedReferenceId;
                try
                {
                    managedReferenceId = property.managedReferenceId;
                }
                catch (InvalidOperationException)
                {
                    return cacheInfo;
                }

                object managedReferenceValue = property.managedReferenceValue;
                cacheInfo.Value = managedReferenceValue;
                if (managedReferenceValue == null)
                {
                    return cacheInfo;
                }

                SyncManagedReferenceBody(cacheInfo.BodyInfo, property, info, null, managedReferenceValue, this);

                return cacheInfo;
            }
#endif

            object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parentValue == null)
            {
                cacheInfo.Error = $"Parent of {property.propertyPath} not found";
                return cacheInfo;
            }

            (string error, int _, object current) = Util.GetValue(property, info, parentValue);
            if (error != "")
            {
                cacheInfo.Error = error;
                return cacheInfo;
            }

            if (current == null)
            {
                cacheInfo.Error = $"Failed to get value from {property.propertyPath}";
                return cacheInfo;
            }

            if (cacheInfo.BodyInfo.Renderers.Count == 0)
            {
                Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                    .ToDictionary(each => each.name, each => each.property);
                cacheInfo.BodyInfo.Renderers = SaintsEditor.HelperGetRenderers(serializedFieldNames,
                    property.serializedObject, this, null, info, propertyIndex, new[] { current });
            }
            else if (cacheInfo.Value != null && cacheInfo.Value != current)
            {
                foreach (ISaintsRenderer saintsRenderer in cacheInfo.BodyInfo.Renderers)
                {
                    saintsRenderer.RefreshTargets(new[] { current });
                }
            }

            foreach (ISaintsRenderer saintsRenderer in cacheInfo.BodyInfo.Renderers)
            {
                saintsRenderer.SetSerializedProperty(property);
            }

            cacheInfo.Value = current;
            cacheInfo.Error = "";
            return cacheInfo;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
            => GetRowFieldHeight(property, label, info, (SaintsRowAttribute)saintsAttribute);

        public float GetRowFieldHeight(SerializedProperty property, GUIContent label, FieldInfo info,
            SaintsRowAttribute saintsRowAttribute = null)
        {
            float fullWidth = _filedWidthCache <= 0
                ? EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15
                : _filedWidthCache;

            saintsRowAttribute ??= new SaintsRowAttribute();
            float baseLineHeight = saintsRowAttribute.Inline ? 0 : SingleLineHeight;
            float fieldHeight = 0f;
            ImGuiCacheInfo cacheInfo = EnsureKey(info, property);

            if (cacheInfo.Error != "")
            {
                fieldHeight = ImGuiHelpBox.GetHeight(cacheInfo.Error, fullWidth, MessageType.Error);
                return baseLineHeight + fieldHeight;
            }

            // ReSharper disable once InvertIf
            if (property.isExpanded || saintsRowAttribute.Inline)
            {
                fieldHeight += GetManagedReferenceBodyHeight(cacheInfo.BodyInfo, fullWidth);
            }

            return baseLineHeight + fieldHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
            => DrawRowField(position, property, label, info, (SaintsRowAttribute)saintsAttribute);

        public void DrawRowField(Rect position, SerializedProperty property, GUIContent label, FieldInfo info,
            SaintsRowAttribute saintsRowAttribute = null)
        {
            saintsRowAttribute ??= new SaintsRowAttribute();

            if (!saintsRowAttribute.Inline)
            {
                // Debug.Log($"render foldout {position.x}, {position.y}");
                Rect foldoutRect = new Rect(position)
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                };
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
                DrawOverrideRichText(foldoutRect, label, overrideRichTextChunks);
            }

            if (!saintsRowAttribute.Inline && !property.isExpanded)
            {
                return;
            }

            Rect leftRect = saintsRowAttribute.Inline
                ? new Rect(position)
                : new Rect(position)
                {
                    x = position.x + SaintsPropertyDrawer.IndentWidth,
                    y = position.y + SaintsPropertyDrawer.SingleLineHeight,
                    height = position.height - SaintsPropertyDrawer.SingleLineHeight,
                    width = position.width - SaintsPropertyDrawer.IndentWidth,
                };

            if (leftRect.width - 1 > Mathf.Epsilon && Event.current.type == EventType.Repaint)
            {
                _filedWidthCache = leftRect.width;
            }

            ImGuiCacheInfo cacheInfo = EnsureKey(info, property);
            if (cacheInfo.Error != "")
            {
                ImGuiHelpBox.Draw(leftRect, cacheInfo.Error, MessageType.Error);
                return;
            }

            DrawManagedReferenceBody(leftRect, cacheInfo.BodyInfo);
        }
    }
}
