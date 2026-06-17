using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SaintsField.Editor;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.GuidDrawer
{
    public partial class GuidAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public bool Initialized;
            public string LastCommittedValue = "";
            public readonly string[] Parts = new string[5];
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private static readonly int[] GuidPartLengths = { 8, 4, 4, 4, 12 };
        private static readonly float[] GuidMinWidths = { 72f, 42f, 42f, 42f, 96f };
        private static readonly float[] GuidWeights = { 8f, 4f, 4f, 4f, 12f };
        private static readonly Color WarningColor = new Color(0.8490566f, 0.3003738f, 0.3003738f);

        private const float InlineGap = 2f;
        private const float SeparatorWidth = 6f;

        private static GUIStyle _imageButtonStyle;
        private static GUIStyle ImageButtonStyle => _imageButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(0, 0, 0, 0),
            imagePosition = ImagePosition.ImageOnly,
            alignment = TextAnchor.MiddleCenter
        };

        private static Texture2D _dropdownIcon;
        private static Texture2D _warningIcon;

        protected override bool UseCreateFieldIMGUI => true;

        private static InfoIMGUI EnsureKey(SerializedProperty property, int index)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}[{index}]";
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return infoCache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width, int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth,
            object parent)
        {
            return TryGetStringProperty(property) == null
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            SerializedProperty stringProperty = TryGetStringProperty(property);
            if (stringProperty == null)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            InfoIMGUI cache = EnsureKey(property, index);
            SyncCache(cache, stringProperty.stringValue);

            bool isSerializedActual = !ReferenceEquals(stringProperty, property);
            DrawGuidField(position, label, cache, property, stringProperty, changedValue =>
            {
                TriggerChangedIMGUI(property, isSerializedActual ? new Guid(changedValue) : changedValue);
            });
        }

        internal static SerializedProperty TryGetStringProperty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return property;
            }

            return property.FindPropertyRelative(nameof(SaintsSerializedProperty.stringValue));
        }

        private static void DrawGuidField(Rect position, GUIContent label, InfoIMGUI cache, SerializedProperty rootProperty,
            SerializedProperty stringProperty, Action<string> onValueChanged)
        {
            EnsureIconContent();

            Rect fieldRect = string.IsNullOrEmpty(label?.text)
                ? position
                : EditorGUI.PrefixLabel(position, label);

            string mergedValue = MergeParts(cache.Parts);
            bool validGuid = Guid.TryParse(mergedValue, out _);

            float buttonWidth = Mathf.Min(EditorGUIUtility.singleLineHeight, fieldRect.width);
            float contentWidth = Mathf.Max(1f,
                fieldRect.width - buttonWidth - InlineGap - SeparatorWidth * 4f - InlineGap * 8f);
            float[] widths = ResolveWidths(contentWidth, GuidMinWidths, GuidWeights);

            float x = fieldRect.x;
            for (int partIndex = 0; partIndex < cache.Parts.Length; partIndex++)
            {
                Rect partRect = new Rect(x, fieldRect.y, widths[partIndex], fieldRect.height);
                string currentPart = cache.Parts[partIndex] ?? "";
                string newPart = DrawPartField(partRect, currentPart, GuidPartLengths[partIndex]);
                cache.Parts[partIndex] = LimitPartText(newPart, GuidPartLengths[partIndex]);

                x = partRect.xMax;
                if (partIndex < cache.Parts.Length - 1)
                {
                    x += InlineGap;
                    Rect separatorRect = new Rect(x, fieldRect.y, SeparatorWidth, fieldRect.height);
                    DrawSeparator(separatorRect, ":");
                    x = separatorRect.xMax + InlineGap;
                }
            }

            Rect buttonRect = new Rect(x + InlineGap, fieldRect.y, Mathf.Max(1f, fieldRect.xMax - x - InlineGap),
                fieldRect.height);
            if (GUI.Button(buttonRect, GetButtonContent(validGuid, mergedValue), ImageButtonStyle))
            {
                ShowGuidMenu(buttonRect, rootProperty.serializedObject.targetObject, mergedValue, selectedValue =>
                {
                    ApplyValue(cache, stringProperty, selectedValue);
                    onValueChanged?.Invoke(selectedValue);
                });
            }

            string updatedMergedValue = MergeParts(cache.Parts);
            if (Guid.TryParse(updatedMergedValue, out _) && updatedMergedValue != stringProperty.stringValue)
            {
                ApplyValue(cache, stringProperty, updatedMergedValue);
                onValueChanged?.Invoke(updatedMergedValue);
            }
        }

        private static void SyncCache(InfoIMGUI cache, string propertyValue)
        {
            string safeValue = propertyValue ?? "";
            if (cache.Initialized && cache.LastCommittedValue == safeValue)
            {
                return;
            }

            cache.Initialized = true;
            cache.LastCommittedValue = safeValue;
            string[] parts = SplitGuidParts(safeValue);
            for (int index = 0; index < cache.Parts.Length; index++)
            {
                cache.Parts[index] = parts[index];
            }
        }

        private static void ApplyValue(InfoIMGUI cache, SerializedProperty stringProperty, string value)
        {
            stringProperty.stringValue = value;
            stringProperty.serializedObject.ApplyModifiedProperties();
            cache.LastCommittedValue = value ?? "";

            string[] parts = SplitGuidParts(cache.LastCommittedValue);
            for (int index = 0; index < cache.Parts.Length; index++)
            {
                cache.Parts[index] = parts[index];
            }
        }

        private static string DrawPartField(Rect position, string value, int length)
        {
            Color oldColor = GUI.backgroundColor;
            if (!IsValidHexPart(value, length))
            {
                GUI.backgroundColor = WarningColor;
            }

            string newValue = EditorGUI.TextField(position, GUIContent.none, value ?? "");
            GUI.backgroundColor = oldColor;
            return newValue;
        }

        private static void DrawSeparator(Rect position, string content)
        {
            EditorGUI.LabelField(position, content, EditorStyles.centeredGreyMiniLabel);
        }

        private static GUIContent GetButtonContent(bool validGuid, string mergedValue)
        {
            Texture2D icon = validGuid ? _dropdownIcon : _warningIcon;
            string tooltip = validGuid ? "" : $"Invalid GUID {mergedValue}";
            return new GUIContent(icon, tooltip);
        }

        private static void ShowGuidMenu(Rect buttonRect, Object targetObject, string currentValue,
            Action<string> onSelected)
        {
            GenericMenu menu = new GenericMenu();
            List<(string name, Guid guid, bool disabled, bool separator)> options = GetGuidOptions(targetObject);
            foreach ((string name, Guid guid, bool disabled, bool separator) in options)
            {
                if (separator)
                {
                    menu.AddSeparator("");
                    continue;
                }

                bool isChecked = currentValue == guid.ToString();
                if (disabled)
                {
                    menu.AddDisabledItem(new GUIContent(name), isChecked);
                    continue;
                }

                menu.AddItem(new GUIContent(name), isChecked, () => onSelected?.Invoke(guid.ToString()));
            }

            menu.DropDown(buttonRect);
        }

        private static List<(string name, Guid guid, bool disabled, bool separator)> GetGuidOptions(Object targetObject)
        {
            List<(string name, Guid guid, bool disabled, bool separator)> options = new List<(string name, Guid guid, bool disabled, bool separator)>
            {
                ("New", Guid.NewGuid(), false, false),
                ("Empty", Guid.Empty, false, false),
            };

            List<(string name, Guid guid, bool disabled, bool separator)> extraOptions =
                CollectExtraGuidOptions(targetObject);
            if (extraOptions.Count > 0)
            {
                options.Add(("", Guid.Empty, false, true));
                options.AddRange(extraOptions);
            }

            return options;
        }

        private static List<(string name, Guid guid, bool disabled, bool separator)> CollectExtraGuidOptions(
            Object targetObject)
        {
            List<(string name, Guid guid, bool disabled, bool separator)> result =
                new List<(string name, Guid guid, bool disabled, bool separator)>();

            switch (targetObject)
            {
                case ScriptableObject so:
                    TryAddGuidFromObject(result, so);
                    break;
                case Component comp:
                    TryAddPrefabGameObject(result, comp.gameObject);
                    break;
                case GameObject go:
                    TryAddPrefabGameObject(result, go);
                    break;
            }

            MonoScript monoScript = SaintsEditor.GetMonoScript(targetObject);
            if (monoScript != null)
            {
                TryAddGuidFromObject(result, monoScript);
            }

            return result;
        }

        private static void TryAddPrefabGameObject(List<(string name, Guid guid, bool disabled, bool separator)> result,
            GameObject gameObject)
        {
            foreach (string assetPath in GetAllPrefabAssetPaths(gameObject))
            {
                TryAddByAssetPath(result, assetPath);
            }
        }

        private static List<string> GetAllPrefabAssetPaths(GameObject go)
        {
            List<string> paths = new List<string>();
            if (go == null)
            {
                return paths;
            }

            PrefabStage stage = PrefabStageUtility.GetPrefabStage(go);
            if (stage != null)
            {
                AddUnique(stage.assetPath);
            }

            AddUnique(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go));

            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(go);
            if (root != null)
            {
                AddUnique(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root));
            }

            AddUnique(AssetDatabase.GetAssetPath(go));

            Object current = PrefabUtility.GetCorrespondingObjectFromSource(go);
            int safety = 0;
            while (current != null && safety++ < 64)
            {
                AddUnique(AssetDatabase.GetAssetPath(current));
                current = PrefabUtility.GetCorrespondingObjectFromSource(current);
            }

            return paths;

            void AddUnique(string path)
            {
                if (!string.IsNullOrEmpty(path) && !paths.Contains(path))
                {
                    paths.Add(path);
                }
            }
        }

        private static void TryAddGuidFromObject(
            List<(string name, Guid guid, bool disabled, bool separator)> result, Object uObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(uObject);
            if (!string.IsNullOrEmpty(assetPath))
            {
                TryAddByAssetPath(result, assetPath);
            }
        }

        private static void TryAddByAssetPath(
            List<(string name, Guid guid, bool disabled, bool separator)> result, string assetPath)
        {
            string unityGuid = AssetDatabase.AssetPathToGUID(assetPath);
            (bool guidOk, Guid guidValue) = ParseUnityGuid(unityGuid);
            if (!guidOk)
            {
                return;
            }

            string fileName = Path.GetFileName(assetPath);
            result.Add((fileName, guidValue, false, false));
        }

        private static (bool ok, Guid guid) ParseUnityGuid(string cleaned)
        {
            if (string.IsNullOrEmpty(cleaned) || cleaned.Length != 32)
            {
                return (false, Guid.Empty);
            }

            string hyphenated =
                $"{cleaned[..8]}-{cleaned.Substring(8, 4)}-{cleaned.Substring(12, 4)}-{cleaned.Substring(16, 4)}-{cleaned.Substring(20, 12)}";
            return Guid.TryParse(hyphenated, out Guid guidValue)
                ? (true, guidValue)
                : (false, Guid.Empty);
        }

        private static string[] SplitGuidParts(string value)
        {
            string[] result = { "", "", "", "", "" };
            string safeValue = value ?? "";
            string[] split = safeValue.Split('-');
            if (split.Length == 1)
            {
                (bool ok, Guid guid) = ParseUnityGuid(safeValue);
                if (ok)
                {
                    split = guid.ToString().Split('-');
                }
            }

            int count = Math.Min(result.Length, split.Length);
            for (int index = 0; index < count; index++)
            {
                result[index] = LimitPartText(split[index], GuidPartLengths[index]);
            }

            return result;
        }

        private static string MergeParts(IReadOnlyList<string> parts)
        {
            return string.Join("-", parts);
        }

        private static bool IsValidHexPart(string value, int length)
        {
            if (string.IsNullOrEmpty(value) || value.Length != length)
            {
                return false;
            }

            foreach (char each in value)
            {
                bool digit = each >= '0' && each <= '9';
                bool lowerHex = each >= 'a' && each <= 'f';
                if (!digit && !lowerHex)
                {
                    return false;
                }
            }

            return true;
        }

        private static string LimitPartText(string value, int length)
        {
            string safeValue = value ?? "";
            return safeValue.Length <= length ? safeValue : safeValue[..length];
        }

        private static float[] ResolveWidths(float totalWidth, IReadOnlyList<float> minWidths, IReadOnlyList<float> weights)
        {
            float[] result = new float[minWidths.Count];
            float minWidthSum = 0f;
            float weightSum = 0f;
            for (int index = 0; index < minWidths.Count; index++)
            {
                minWidthSum += minWidths[index];
                weightSum += weights[index];
            }

            if (totalWidth <= minWidthSum + 0.01f)
            {
                float scale = minWidthSum < Mathf.Epsilon ? 1f : totalWidth / minWidthSum;
                for (int index = 0; index < result.Length; index++)
                {
                    result[index] = Mathf.Max(1f, minWidths[index] * scale);
                }

                return result;
            }

            float extraWidth = totalWidth - minWidthSum;
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = minWidths[index] + extraWidth * (weights[index] / weightSum);
            }

            return result;
        }

        private static void EnsureIconContent()
        {
            _dropdownIcon ??= Util.LoadResource<Texture2D>("classic-dropdown-gray.png");
            _warningIcon ??= (Texture2D)EditorGUIUtility.IconContent("console.warnicon").image;
        }
    }
}
