using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    public class ExpandableIMGUIScoop: IDisposable
    {
        private static int _scoopCount;

        public static bool IsInScoop => _scoopCount > 0;

        public ExpandableIMGUIScoop()
        {
            _scoopCount++;
        }

        public void Dispose()
        {
            _scoopCount--;
        }
    }

    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private class ExpandableInfo
        {
            public string Error;
            public SerializedObject SerializedObject;
        }

        private static readonly Dictionary<string, ExpandableInfo> IdToInfo = new Dictionary<string, ExpandableInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";
#if UNITY_2019_2_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
#if UNITY_2019_3_OR_NEWER
        [InitializeOnEnterPlayMode]
#endif
        private static void ImGuiClearSharedData()
        {
            foreach (ExpandableInfo expandableInfo in IdToInfo.Values)
            {
                DisposeExpandableInfo(expandableInfo);
            }
            IdToInfo.Clear();
        }

        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            if (IdToInfo.TryGetValue(_cacheKey, out ExpandableInfo expandableInfo))
            {
                DisposeExpandableInfo(expandableInfo);
                IdToInfo.Remove(_cacheKey);
            }
        }

        private static void DisposeExpandableInfo(ExpandableInfo expandableInfo)
        {
            // ReSharper disable once MergeIntoPattern
            // ReSharper disable once MergeSequentialChecks
            if (expandableInfo == null || expandableInfo.SerializedObject == null)
            {
                return;
            }

            try
            {
                expandableInfo.SerializedObject.Dispose();
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private ExpandableInfo GetSerializedObject(SerializedProperty property, FieldInfo info, object parent)
        {
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _cacheKey = GetKey(property);
            if(IdToInfo.TryGetValue(_cacheKey, out ExpandableInfo expandableInfo) && expandableInfo.SerializedObject != null)
            {
                return expandableInfo;
            }
            Object serObject = GetSerObject(property, info, parent);
            if (serObject == null)
            {
                if(IdToInfo.TryGetValue(_cacheKey, out ExpandableInfo expandableNullInfo))
                {
                    DisposeExpandableInfo(expandableNullInfo);
                }
                return IdToInfo[_cacheKey] = new ExpandableInfo
                {
                    Error = "",
                    SerializedObject = null,
                };
            }

            SerializedObject serializedObject = null;
            try
            {
                serializedObject = new SerializedObject(serObject);
            }
            catch (Exception e)
            {
                return IdToInfo[_cacheKey] = new ExpandableInfo
                {
                    Error = $"Failed to create a SerializedObject: {e.Message}",
                    SerializedObject = null,
                };
            }
            return IdToInfo[_cacheKey] = new ExpandableInfo
            {
                Error = "",
                SerializedObject = serializedObject,
            };
        }
        // private string _error = "";

        // list/array shares the same drawer in Unity
        // for convenience, we use the propertyPath as key as it already contains the index
        // private Dictionary<string, UnityEditor.Editor> _propertyPathToEditor = new Dictionary<string, UnityEditor.Editor>();

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            ExpandableInfo serInfo = GetSerializedObject(property, info, parent);
            if (serInfo.Error != "")
            {
                return -1;
            }

            bool curExpanded = property.isExpanded;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_EXPANDABLE
            Debug.Log($"cur expand {curExpanded}/{KeyExpanded(property)}");
#endif
            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                using(new GUIEnabledScoop(true))
                {
                    bool newExpanded = EditorGUI.Foldout(position, curExpanded,
                        new GUIContent(new string(' ', property.displayName.Length)), true);
                    if (changed.changed)
                    {
                        property.isExpanded = newExpanded;
                    }
                }
            }

            return 13;
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
            ExpandableInfo serInfo = GetSerializedObject(property, info, parent);
            float basicHeight = serInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(serInfo.Error, width, MessageType.Error);

            if (!property.isExpanded || serInfo.SerializedObject == null)
            {
                return basicHeight;
            }

            serInfo.SerializedObject.UpdateIfRequiredOrScript();
            float expandedHeight = GetAllField(serInfo.SerializedObject)
                .Select(childProperty => EditorGUI.GetPropertyHeight(childProperty, true) + 2)
                .Sum();
            return basicHeight + expandedHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            ExpandableInfo serInfo = GetSerializedObject(property, info, parent);

            Rect leftRect = position;

            if (serInfo.Error != "")
            {
                leftRect = ImGuiHelpBox.Draw(position, serInfo.Error, MessageType.Error);
            }

            bool isExpand = property.isExpanded;
            // Debug.Log($"below expand = {isExpand}");
            if (!isExpand || serInfo.SerializedObject == null)
            {
                return leftRect;
            }

            // serializedObject.Update();

            Rect indentedRect;
            using (new EditorGUI.IndentLevelScope(1))
            {
                indentedRect = EditorGUI.IndentedRect(leftRect);
            }

            float indentWidth = indentedRect.x - leftRect.x;

            float usedHeight = 0;

            using(new EditorGUI.IndentLevelScope(1))
            using(new AdaptLabelWidth())
            using(new ResetIndentScoop())
            using(new ExpandableIMGUIScoop())
            {
                foreach (SerializedProperty iterator in GetAllField(serInfo.SerializedObject))
                {
                    float childHeight = EditorGUI.GetPropertyHeight(iterator, true) + 2;
                    (Rect childRect, Rect leftOutRect) = RectUtils.SplitHeightRect(indentedRect, childHeight);
                    indentedRect = leftOutRect;
                    usedHeight += childHeight;

                    GUI.Box(new Rect(childRect)
                    {
                        x = childRect.x - indentWidth,
                        width = childRect.width + indentWidth,
                    }, GUIContent.none);
                    EditorGUI.PropertyField(new Rect(childRect)
                    {
                        y = childRect.y + 1,
                        height = childRect.height - 2,
                    }, iterator, true);
                }

                serInfo.SerializedObject.ApplyModifiedProperties();
            }

            return new Rect(leftRect)
            {
                y = indentedRect.y + indentedRect.height,
                height = leftRect.height - usedHeight,
            };
        }

        #endregion

        private static IEnumerable<SerializedProperty> GetAllField(SerializedObject obj)
        {
            obj.UpdateIfRequiredOrScript();
            SerializedProperty iterator = obj.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                // using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                if("m_Script" != iterator.propertyPath)
                {
                    yield return iterator;
                }
            }
        }

        private static Object GetSerObject(SerializedProperty property, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return property.objectReferenceValue;
            }

            (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

            if (error == "" && propertyValue is IWrapProp wrapProp)
            {
                return (Object)Util.GetWrapValue(wrapProp);
            }

            return null;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__ExpandableAttributeDrawer_Foldout";
        private static string NameProps(SerializedProperty property) => $"{property.propertyPath}__ExpandableAttributeDrawer_Props";

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            Foldout foldOut = new Foldout
            {
                style =
                {
                    // backgroundColor = Color.green,
                    // left = -5,
                    position = Position.Absolute,
                    width = LabelBaseWidth - IndentWidth,
                },
                name = NameFoldout(property),
                value = false,
            };

            foldOut.RegisterValueChangedCallback(v =>
            {
                container.Q<VisualElement>(NameProps(property)).style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return foldOut;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                    backgroundColor = EColor.CharcoalGray.GetColor(),
                },
                name = NameProps(property),
                userData = null,
            };

            visualElement.AddToClassList(ClassAllowDisable);

            return visualElement;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            Foldout foldOut = container.Q<Foldout>(NameFoldout(property));
            if (!foldOut.value)
            {
                return;
            }

            VisualElement propsElement = container.Q<VisualElement>(NameProps(property));
            Object curObject = (Object) propsElement.userData;

            Object serObject = GetSerObject(property, info, parent);

            if (ReferenceEquals(serObject, curObject))
            {
                return;
            }

            DisplayStyle foldoutDisplay = serObject == null ? DisplayStyle.None : DisplayStyle.Flex;
            if(foldOut.style.display != foldoutDisplay)
            {
                foldOut.style.display = foldoutDisplay;
            }

            propsElement.userData = serObject;
            propsElement.Clear();
            if (serObject == null)
            {
                return;
            }

            InspectorElement inspectorElement = new InspectorElement(serObject)
            {
                // style =
                // {
                //     width = Length.Percent(100),
                // },
            };

            propsElement.Add(inspectorElement);

            // foreach (PropertyField propertyField in GetPropertyFields(property, property.objectReferenceValue))
            // {
            //     propsElement.Add(propertyField);
            // }
        }

        #endregion

#endif
    }
}
