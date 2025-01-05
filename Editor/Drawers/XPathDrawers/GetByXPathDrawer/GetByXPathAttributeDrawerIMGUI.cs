using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;


namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private Texture2D _refreshIcon;
        private Texture2D _removeIcon;

        public class PropertyCache
        {
            public string Error;
            public bool MisMatch;
            public object OriginalValue;
            public object TargetValue;
            public bool TargetIsNull;

            public MemberInfo MemberInfo;
            public object Parent;
            public SerializedProperty SerializedProperty;
        }

        private class GetByXPathGenericCache
        {
            public int ImGuiRenderCount;  // IMGUI fix
            public double ImGuiResourcesLastTime;  // IMGUI fix

            public double UpdatedLastTime;
            public string Error;

            // public Texture2D RefreshIcon;
            // public Texture2D RemoveIcon;
            public object Parent;
            public Type ExpectedType;
            public Type ExpectedInterface;

            public IReadOnlyList<GetByXPathAttribute> GetByXPathAttributes;
            public IReadOnlyList<object> CachedResults;

            public SerializedProperty ArrayProperty;
            // public bool IsArray;

            public readonly Dictionary<int, PropertyCache> IndexToPropertyCache = new Dictionary<int, PropertyCache>();
        }

        private static readonly Dictionary<string, GetByXPathGenericCache> ImGuiSharedCache = new Dictionary<string, GetByXPathGenericCache>();




        private static readonly Dictionary<UnityEngine.Object, HashSet<string>> InspectingTargets = new Dictionary<UnityEngine.Object, HashSet<string>>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // return 0;
            if (EditorApplication.isPlaying)
            {
                return 0;
            }

            string arrayRemovedKey;

            try
            {
                arrayRemovedKey = SerializedUtils.GetUniqueIdArray(property);
            }
            catch (ObjectDisposedException e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif

                return 0;
            }
            catch (NullReferenceException e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif

                return 0;
            }

            UnityEngine.Object curInspectingTarget = property.serializedObject.targetObject;
            if (!InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> keySet))
            {
                InspectingTargets[curInspectingTarget] = keySet = new HashSet<string>();

                void OnSelectionChangedIMGUI()
                {
                    bool stillSelected = Array.IndexOf(Selection.objects, curInspectingTarget) != -1;
                    // Debug.Log($"{stillSelected}/{string.Join(", ", Selection.objects.Cast<Object>())}");
                    if (stillSelected)
                    {
                        return;
                    }

                    Selection.selectionChanged -= OnSelectionChangedIMGUI;
                    if (InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> set))
                    {
                        foreach (string removeKey in set)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                            Debug.Log($"#GetByXPath# CleanUp {removeKey}");
#endif
                            ImGuiSharedCache.Remove(removeKey);
                        }
                    }
                    InspectingTargets.Remove(curInspectingTarget);
                }

                Selection.selectionChanged += OnSelectionChangedIMGUI;
            }
            keySet.Add(arrayRemovedKey);

            bool configExists = ImGuiSharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache genericCache);
            if (!configExists)
            {
                return 0;
            }

            if (!ReferenceEquals(genericCache.GetByXPathAttributes[0], saintsAttribute))
            {
                return 0;
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            // update information for this property
            PropertyCache propertyCache = genericCache.IndexToPropertyCache[propertyIndex];
            GetByXPathAttribute firstAttribute = genericCache.GetByXPathAttributes[0];

            if (NothingSigner(genericCache.GetByXPathAttributes[0]))
            {
                return firstAttribute.UsePickerButton ? SingleLineHeight : 0;;
            }

            float useWidth = firstAttribute.UsePickerButton ? SingleLineHeight : 0;
            if (genericCache.Error != "")
            {
                return useWidth;
            }

            if (propertyCache.Error != "")
            {
                return useWidth;
            }

            if (!propertyCache.MisMatch)
            {
                return useWidth;
            }

            if (!firstAttribute.UseResignButton)
            {
                return useWidth;
            }

            return useWidth + SingleLineHeight;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlaying)
            {
                return false;
            }

            string key;
            try
            {
                key = SerializedUtils.GetUniqueIdArray(property);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }

            bool configExists = ImGuiSharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache);
            bool needUpdate = !configExists;
            bool isArray = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0;
            // not sure why...
            // the first pass will just fall. And the old version need 2 pass. This new version need 4 pass.
            int requiredCounter = isArray? SaintsFieldConfigUtil.GetByXPathArrayPassIMGUI(): SaintsFieldConfigUtil.GetByXPathFieldPassIMGUI();
            // int requiredCounter = 1;
            if (!needUpdate)
            {
                needUpdate = genericCache.ImGuiRenderCount <= requiredCounter;
                if (!needUpdate)
                {
                    double loopInterval = SaintsFieldConfigUtil.GetByXPathLoopIntervalMsIMGUI();
                    if(loopInterval > 0)
                    {
                        double curTime = EditorApplication.timeSinceStartup;
                        needUpdate = curTime - genericCache.UpdatedLastTime > loopInterval / 1000f;
                    }
                }
            }

            if (needUpdate)
            {
                if (genericCache == null)
                {
                    genericCache = new GetByXPathGenericCache
                    {
                        ImGuiRenderCount = 1,
                        Error = "",
                    };
                }
                else
                {
                    genericCache.ImGuiRenderCount = Mathf.Min(100, genericCache.ImGuiRenderCount + 1);
                }

                bool firstTime = genericCache.ImGuiRenderCount <= requiredCounter;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# UpdateImGuiSharedCache for {key} ({property.propertyPath}), firstTime={firstTime}, renderCount={genericCache.ImGuiRenderCount}");
#endif
                UpdateSharedCache(genericCache, firstTime, property, info, true);
                // var propCache = genericCache.IndexToPropertyCache[SerializedUtils.PropertyPathIndex(property.propertyPath)];
                // onGUIPayload.SetValue(property.objectReferenceValue);
                // property.serializedObject.ApplyModifiedProperties();
                ImGuiSharedCache[key] = genericCache;
                // Debug.Log(property.objectReferenceValue);
                // return false;
            }

            // Debug.Log(property.serializedObject.targetObject);
            // var t = ((Component)property.serializedObject.targetObject).gameObject.GetComponents<Component>()
            //     .First(each => each is not Transform);
            // // Debug.Log(t);
            // if (property.objectReferenceValue != t)
            // {
            //     Debug.Log($"Update {t}");
            //     property.objectReferenceValue = t;
            // }
            // Debug.Log(property.objectReferenceValue);

            if (genericCache.Error != "")
            {
                return false;
            }

            if (!ReferenceEquals(genericCache.GetByXPathAttributes[0], saintsAttribute))
            {
                return false;
            }

            if (NothingSigner(genericCache.GetByXPathAttributes[0]))
            {
                return DrawPicker(genericCache.GetByXPathAttributes[0], genericCache, position, property,
                    genericCache.IndexToPropertyCache[SerializedUtils.PropertyPathIndex(property.propertyPath)], onGUIPayload, info);
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if(!genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache))
            {
                return false;
            }

            if (propertyCache.Error != "")
            {
                return false;
            }

            if (!configExists)
            {
                return false;
            }

            Rect leftRect = position;
            bool willDraw = false;

            GetByXPathAttribute firstAttr = genericCache.GetByXPathAttributes[0];
            if (propertyCache.MisMatch && firstAttr.UseResignButton)
            {
                willDraw = true;
                (Rect actionButtonRect, Rect lRect) = RectUtils.SplitWidthRect(position, SingleLineHeight);
                leftRect = lRect;
                if (propertyCache.TargetIsNull)
                {
                    if (_removeIcon == null)
                    {
                        _removeIcon = Util.LoadResource<Texture2D>("close.png");
                    }

                    if (GUI.Button(actionButtonRect, _removeIcon))
                    {
                        DoSignPropertyCache(propertyCache);
                        onGUIPayload.SetValue(propertyCache.TargetValue);
                    }
                }
                else
                {
                    if (_refreshIcon == null)
                    {
                        _refreshIcon = Util.LoadResource<Texture2D>("refresh.png");
                    }

                    if (GUI.Button(actionButtonRect, _refreshIcon))
                    {
                        DoSignPropertyCache(propertyCache);
                        onGUIPayload.SetValue(null);
                    }
                }
            }

            // ReSharper disable once InvertIf
            if (firstAttr.UsePickerButton)
            {
                willDraw = true;
                if (GUI.Button(leftRect, "●"))
                {
                    OpenPicker(property, info, genericCache.GetByXPathAttributes,
                        genericCache.ExpectedType, genericCache.ExpectedInterface,
                        newValue =>
                        {
                            propertyCache.TargetValue = newValue;
                            DoSignPropertyCache(propertyCache);
                            onGUIPayload.SetValue(newValue);
                        }, propertyCache.Parent);
                }
            }

            return willDraw;
        }

        private static bool DrawPicker(GetByXPathAttribute firstAttr, GetByXPathGenericCache genericCache, Rect leftRect, SerializedProperty property,
            PropertyCache propertyCache, OnGUIPayload onGUIPayload, FieldInfo info)
        {
            if (!firstAttr.UsePickerButton)
            {
                return false;
            }

            if (GUI.Button(leftRect, "●"))
            {
                OpenPicker(property, info, genericCache.GetByXPathAttributes,
                    genericCache.ExpectedType, genericCache.ExpectedInterface,
                    newValue =>
                    {
                        propertyCache.TargetValue = newValue;
                        DoSignPropertyCache(propertyCache);
                        onGUIPayload.SetValue(newValue);
                    }, propertyCache.Parent);
            }

            return true;

        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            string content = GetBelowMessage(property, saintsAttribute);
            return !string.IsNullOrEmpty(content);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
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
            if (EditorApplication.isPlaying)
            {
                return "";
            }

            string key;
            try
            {
                key = SerializedUtils.GetUniqueIdArray(property);
            }
            catch (ObjectDisposedException)
            {
                return "";
            }
            catch (NullReferenceException)
            {
                return "";
            }

            if(!ImGuiSharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache))
            {
                return "";
            }

            if (genericCache.Error != "")
            {
                return genericCache.Error;
            }

            if (!ReferenceEquals(genericCache.GetByXPathAttributes[0], saintsAttribute))
            {
                return "";
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            if(!genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache))
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
