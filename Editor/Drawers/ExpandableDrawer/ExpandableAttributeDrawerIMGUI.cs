using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ExpandableDrawer
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

    public partial class ExpandableAttributeDrawer: IMakeRenderer
    {

        private class ExpandableInfo
        {
            public string Error;
            public Object TargetObject;
            public SerializedObject SerializedObject;
            public IReadOnlyList<ISaintsRenderer> Renderers;
        }

        private static readonly Dictionary<string, ExpandableInfo> IdToInfo = new Dictionary<string, ExpandableInfo>();

        private static ExpandableInfo EnsureExpandableInfo(IMakeRenderer makeRenderer, SerializedProperty property, MemberInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);

            Object serObject = GetSerObject(property, info, parent);

            bool hasKey = IdToInfo.TryGetValue(key, out ExpandableInfo expandableInfo);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(!hasKey)
            {
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    // ReSharper disable once InvertIf
                    if (IdToInfo.TryGetValue(key, out ExpandableInfo value))
                    {
                        // ReSharper disable once UseNullPropagation
                        if (value.SerializedObject != null)
                        {
                            // Debug.Log($"dispose {key}");
                            value.SerializedObject.Dispose();
                        }

                        IdToInfo.Remove(key);
                    }
                });
            }

            if (hasKey && ReferenceEquals(serObject, expandableInfo.TargetObject))
            {
                return expandableInfo;
            }

            if (serObject == null)
            {
                // if (property.isExpanded)
                // {
                //     property.isExpanded = false;
                // }

                return new ExpandableInfo
                {
                    Error = "",
                    TargetObject = null,
                    SerializedObject = null,
                    Renderers = Array.Empty<ISaintsRenderer>(),
                };
            }

            if(hasKey && expandableInfo.SerializedObject != null)
            {
                // Debug.Log($"dispose {key}");
                try
                {
                    expandableInfo.SerializedObject.Dispose();
                }
                catch (Exception)
                {
                    // do nothing
                }
            }

            SerializedObject ser = new SerializedObject(serObject);

            IReadOnlyList<ISaintsRenderer> renderers = SaintsEditor.Setup(Array.Empty<string>(), ser, makeRenderer, serObject);

            // Debug.Log(serObject);

            // Debug.Log($"create {key}");

            return IdToInfo[key] = new ExpandableInfo
            {
                Error = "",
                TargetObject = serObject,
                SerializedObject = ser,
                Renderers = renderers,
            };
        }

        protected override float DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            ExpandableInfo serInfo = EnsureExpandableInfo(this, property, info, parent);
            if (serInfo.Error != "")
            {
                return -1;
            }

            bool curExpanded = property.isExpanded;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_EXPANDABLE
            Debug.Log($"cur expand {curExpanded}/{KeyExpanded(property)}");
#endif
            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                using (new GUIEnabledScoop(true))
                {
                    bool newExpanded = EditorGUI.Foldout(position, curExpanded,
                        new GUIContent(new string(' ', property.displayName.Length)), true);
                    if (changed.changed)
                    {
                        property.isExpanded = newExpanded;
                    }
                }
            }

            return SaintsFieldConfigUtil.GetFoldoutSpaceImGui();
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
            ExpandableInfo serInfo = EnsureExpandableInfo(this, property, info, parent);
            float basicHeight =
                serInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(serInfo.Error, width, MessageType.Error);

            if (!property.isExpanded || serInfo.SerializedObject == null)
            {
                return basicHeight;
            }

            serInfo.SerializedObject.UpdateIfRequiredOrScript();

            // float expandedHeight = SerializedUtils.GetAllField(serInfo.SerializedObject)
            //     .Select(childProperty => EditorGUI.GetPropertyHeight(childProperty, true) + 2)
            //     .Sum();
            float expandedHeight = serInfo.Renderers.Sum(renderer => renderer.GetHeightIMGUI(width - IndentWidth));
            // Debug.Log($"width={width}, count={serInfo.Renderers.Count}, height={expandedHeight}");
            // Debug.Log(serInfo.SerializedObject.targetObject);
            return basicHeight + expandedHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // Debug.Log(position.height);

            ExpandableInfo serInfo = EnsureExpandableInfo(this, property, info, parent);

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

            // Rect indentedRect;
            // using (new EditorGUI.IndentLevelScope(1))
            // {
            //     indentedRect = EditorGUI.IndentedRect(leftRect);
            // }
            //
            // float indentWidth = indentedRect.x - leftRect.x;

            // Debug.Log(serInfo.Renderers.Count);

            // Debug.Log(leftRect.height);

            GUI.Box(leftRect, GUIContent.none);

            Rect expandabledRect = RectUtils.SplitWidthRect(leftRect, IndentWidth).leftRect;
            float expandableWidth = expandabledRect.width;

            // float usedHeight = 0;

            using (new EditorGUI.IndentLevelScope(1))
            using (new AdaptLabelWidth())
            using (new ResetIndentScoop())
            using (new ExpandableIMGUIScoop())
            {
                // foreach (SerializedProperty iterator in SerializedUtils.GetAllField(serInfo.SerializedObject))
                // {
                //     float childHeight = EditorGUI.GetPropertyHeight(iterator, true) + 2;
                //     (Rect childRect, Rect leftOutRect) = RectUtils.SplitHeightRect(indentedRect, childHeight);
                //     indentedRect = leftOutRect;
                //     usedHeight += childHeight;
                //
                //     GUI.Box(new Rect(childRect)
                //     {
                //         x = childRect.x - indentWidth,
                //         width = childRect.width + indentWidth,
                //     }, GUIContent.none);
                //     EditorGUI.PropertyField(new Rect(childRect)
                //     {
                //         y = childRect.y + 1,
                //         height = childRect.height - 2,
                //     }, iterator, true);
                // }

                foreach (ISaintsRenderer saintsRenderer in serInfo.Renderers)
                {
                    float childHeight = saintsRenderer.GetHeightIMGUI(expandableWidth);
                    (Rect childRect, Rect leftOutRect) = RectUtils.SplitHeightRect(expandabledRect, childHeight);
                    expandabledRect = leftOutRect;
                    // usedHeight += childHeight;

                    // GUI.Box(new Rect(childRect)
                    // {
                    //     x = childRect.x - indentWidth,
                    //     width = childRect.width + indentWidth,
                    // }, GUIContent.none);
                    // EditorGUI.PropertyField(new Rect(childRect)
                    // {
                    //     y = childRect.y + 1,
                    //     height = childRect.height - 2,
                    // }, iterator, true);
                    try
                    {
                        saintsRenderer.RenderPositionIMGUI(childRect);
                    }
                    catch (NullReferenceException)
                    {
                        CleanAndRemove(property);
                        return expandabledRect;
                    }
                    catch (ObjectDisposedException)
                    {
                        CleanAndRemove(property);
                        return expandabledRect;
                    }


                }

                serInfo.SerializedObject.ApplyModifiedProperties();
            }

            // return new Rect(leftRect)
            // {
            //     y = indentedRect.y + indentedRect.height,
            //     height = leftRect.height - usedHeight,
            // };
            return expandabledRect;
        }

        private static void CleanAndRemove(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!IdToInfo.TryGetValue(key, out ExpandableInfo info))
            {
                return;
            }

            if (info.SerializedObject != null)
            {
                // Debug.Log($"Disposed on error {key}");
                try
                {
                    info.SerializedObject.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            IdToInfo.Remove(key);
        }

        public AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }
    }
}
