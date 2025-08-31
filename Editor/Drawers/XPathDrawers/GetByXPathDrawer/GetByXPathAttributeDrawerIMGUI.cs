using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
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

        public class GetByXPathGenericCache
        {
            // public int ImGuiRenderCount;  // IMGUI fix
            // public double ImGuiResourcesLastTime;  // IMGUI fix

            public double UpdateResourceAfterTime;

            // public double UpdatedLastTime;
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

        public static readonly Dictionary<string, GetByXPathGenericCache> SharedCache = new Dictionary<string, GetByXPathGenericCache>();

        // private static readonly Dictionary<UnityEngine.Object, HashSet<string>> InspectingTargets = new Dictionary<UnityEngine.Object, HashSet<string>>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // return 0;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return 0;
            }

            string arrayRemovedKey;

            try
            {
                arrayRemovedKey = SerializedUtils.GetUniqueIdArray(property);
            }
#pragma warning disable CS0168
            catch (ObjectDisposedException e)
#pragma warning restore CS0168
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif

                return 0;
            }
#pragma warning disable CS0168
            catch (NullReferenceException e)
#pragma warning restore CS0168
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif

                return 0;
            }

//             UnityEngine.Object curInspectingTarget = property.serializedObject.targetObject;
//             if (!InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> keySet))
//             {
//                 InspectingTargets[curInspectingTarget] = keySet = new HashSet<string>();
//
//                 void OnSelectionChangedIMGUI()
//                 {
//                     bool stillSelected = Array.IndexOf(Selection.objects, curInspectingTarget) != -1;
//                     // Debug.Log($"{stillSelected}/{string.Join(", ", Selection.objects.Cast<Object>())}");
//                     if (stillSelected)
//                     {
//                         return;
//                     }
//
//                     Selection.selectionChanged -= OnSelectionChangedIMGUI;
//                     if (InspectingTargets.TryGetValue(curInspectingTarget, out HashSet<string> set))
//                     {
//                         foreach (string removeKey in set)
//                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
//                             Debug.Log($"#GetByXPath# CleanUp {removeKey}");
// #endif
//                             SharedCache.Remove(removeKey);
//                         }
//                     }
//                     InspectingTargets.Remove(curInspectingTarget);
//                 }
//
//                 Selection.selectionChanged += OnSelectionChangedIMGUI;
//             }
//             keySet.Add(arrayRemovedKey);

            bool configExists = SharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache genericCache);
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
                return firstAttribute.UsePickerButton ? SingleLineHeight : 0;
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

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
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

            bool configExists = SharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache);
            if (!configExists)
            {
                SharedCache[key] = genericCache = new GetByXPathGenericCache
                {
                    Error = "",
                    GetByXPathAttributes = allAttributes.OfType<GetByXPathAttribute>().ToArray(),
                    UpdateResourceAfterTime = double.MinValue,
                };

                void ProjectChangedHandler()
                {
                    // Debug.Log($"ProjectChangedHandler {key}");
                    if(SharedCache.TryGetValue(key, out GetByXPathGenericCache cache))
                    {
                        double curTime = EditorApplication.timeSinceStartup;
                        // Debug.Log($"update {key} {curTime} {cache.UpdateResourceAfterTime}");
                        if (cache.UpdateResourceAfterTime <= curTime)
                        {
                            // update resources after 0.5s
                            cache.UpdateResourceAfterTime = EditorApplication.timeSinceStartup + 0.5;
                            // Debug.Log($"Update in {cache.UpdateResourceAfterTime}");
                        }

                    }
                }

                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(ProjectChangedHandler);

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    // Debug.Log($"remove key watch {key}");
                    SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(ProjectChangedHandler);
                    SharedCache.Remove(key);
                });
            }

            if (genericCache.Error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.Log(genericCache.Error);
#endif
                return false;
            }

            if (!genericCache.GetByXPathAttributes[0].Equals(saintsAttribute))
            {
                // Debug.Log($"{genericCache.GetByXPathAttributes[0]}/{saintsAttribute}");
                // Debug.Log("return");
                return false;
            }

            if (NothingSigner(genericCache.GetByXPathAttributes[0]))
            {
                // Debug.Log("return");
                return DrawPicker(genericCache.GetByXPathAttributes[0], genericCache, position, property,
                    genericCache.IndexToPropertyCache[SerializedUtils.PropertyPathIndex(property.propertyPath)], onGUIPayload, info);
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            UpdateSharedCacheBase(genericCache, property, info);
            // if (genericCache.UpdateResourceAfterTime > 0)
            // {
            //     Debug.Log($"{genericCache.UpdateResourceAfterTime} {EditorApplication.timeSinceStartup}");
            // }
            // Debug.Log(genericCache.UpdateResourceAfterTime);
            if(!configExists || genericCache.UpdateResourceAfterTime > EditorApplication.timeSinceStartup)
            {
                genericCache.UpdateResourceAfterTime = double.MinValue;
                UpdateSharedCacheSource(genericCache, property, info);
            }

            UpdateSharedCacheSetValue(genericCache, !configExists, property);

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
                        if(DoSignPropertyCache(propertyCache, false))
                        {
                            propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                            onGUIPayload.SetValue(propertyCache.TargetValue);
                        }
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
                        if(DoSignPropertyCache(propertyCache, false))
                        {
                            propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                            onGUIPayload.SetValue(null);
                        }
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
                            var oldValue = propertyCache.TargetValue;
                            propertyCache.TargetValue = newValue;
                            if(DoSignPropertyCache(propertyCache, false))
                            {
                                propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                                onGUIPayload.SetValue(newValue);
                            }
                            else
                            {
                                propertyCache.TargetValue = oldValue;
                            }
                        }, propertyCache.Parent);
                }
            }

            return willDraw;
        }

        private bool DrawPicker(GetByXPathAttribute firstAttr, GetByXPathGenericCache genericCache, Rect leftRect, SerializedProperty property,
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
                        object oldValue = propertyCache.TargetValue;
                        propertyCache.TargetValue = newValue;
                        if(DoSignPropertyCache(propertyCache, false))
                        {
                            propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                            onGUIPayload.SetValue(newValue);
                        }
                        else
                        {
                            propertyCache.TargetValue = oldValue;
                        }
                    }, propertyCache.Parent);
            }

            return true;

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
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload,
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

            if(!SharedCache.TryGetValue(key, out GetByXPathGenericCache genericCache))
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
