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

    public partial class ExpandableAttributeDrawer
    {

        private class ExpandableInfo
        {
            public string Error;
            // public HashSet<Object> TargetObjects;
            public SerializedObject SerializedObject;
            public IReadOnlyList<ISaintsRenderer> Renderers;
            public UnityEditor.Editor Editor;
            public bool UseEditorIMGUI;
            public float EditorHeight = EditorGUIUtility.singleLineHeight;
            public float EditorWidth;
        }

        private static readonly Dictionary<string, ExpandableInfo> IdToInfo = new Dictionary<string, ExpandableInfo>();

        private static ExpandableInfo EnsureExpandableInfo(IMakeRenderer makeRenderer, SerializedProperty property, MemberInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);

            SerializedObject serializedObject = GetSerObject(property, info, parent);

            bool hasKey = IdToInfo.TryGetValue(key, out ExpandableInfo expandableInfo);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(!hasKey)
            {
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    // ReSharper disable once InvertIf
                    if (IdToInfo.TryGetValue(key, out ExpandableInfo value))
                    {
                        DisposeInfo(value);
                        IdToInfo.Remove(key);
                    }
                });
            }

            if (hasKey && EqualSerObject(serializedObject, expandableInfo.SerializedObject))
            {
                // GetSerObject creates a fresh wrapper. Keep the cached one and release this probe.
                serializedObject?.Dispose();
                return expandableInfo;
            }

            if (serializedObject == null)
            {
                // if (property.isExpanded)
                // {
                //     property.isExpanded = false;
                // }

                if (hasKey)
                {
                    DisposeInfo(expandableInfo);
                    IdToInfo.Remove(key);
                }

                return new ExpandableInfo
                {
                    Error = "",
                    SerializedObject = null,
                    Renderers = Array.Empty<ISaintsRenderer>(),
                };
            }

            if(hasKey)
            {
                DisposeInfo(expandableInfo);
                IdToInfo.Remove(key);
            }

            ExpandableInfo newInfo;
            try
            {
                newInfo = MakeExpandableInfo(makeRenderer, serializedObject);
            }
            catch (Exception)
            {
                serializedObject.Dispose();
                throw;
            }

            // Debug.Log(serObject);

            // Debug.Log($"create {key}");

            return IdToInfo[key] = newInfo;
        }

        private static ExpandableInfo MakeExpandableInfo(IMakeRenderer makeRenderer, SerializedObject serializedObject)
        {
            UnityEditor.Editor editor = CreateTargetEditor(serializedObject.targetObjects);

            if (editor is IMakeRenderer editorMakeRenderer)
            {
                IReadOnlyList<ISaintsRenderer> renderers;
                try
                {
                    renderers = SaintsEditor.Setup(Array.Empty<string>(), serializedObject, editorMakeRenderer,
                        serializedObject.targetObjects);
                }
                catch (Exception)
                {
                    Object.DestroyImmediate(editor);
                    throw;
                }

                return new ExpandableInfo
                {
                    Error = "",
                    SerializedObject = serializedObject,
                    Renderers = renderers,
                    Editor = editor,
                    UseEditorIMGUI = false,
                };
            }

            if (editor != null)
            {
                return new ExpandableInfo
                {
                    Error = "",
                    SerializedObject = serializedObject,
                    Renderers = Array.Empty<ISaintsRenderer>(),
                    Editor = editor,
                    UseEditorIMGUI = true,
                };
            }

            return new ExpandableInfo
            {
                Error = "",
                SerializedObject = serializedObject,
                Renderers = SaintsEditor.Setup(Array.Empty<string>(), serializedObject, makeRenderer, serializedObject.targetObjects),
            };
        }

        private static UnityEditor.Editor CreateTargetEditor(Object[] targets)
        {
            try
            {
                return UnityEditor.Editor.CreateEditor(targets);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void DisposeInfo(ExpandableInfo info)
        {
            if (info == null)
            {
                return;
            }

            if (info.Renderers != null)
            {
                foreach (ISaintsRenderer renderer in info.Renderers)
                {
                    try
                    {
                        renderer.OnDestroy();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            if (info.SerializedObject != null)
            {
                // Debug.Log($"dispose {key}");
                try
                {
                    info.SerializedObject.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (info.Editor != null)
            {
                Object.DestroyImmediate(info.Editor);
                info.Editor = null;
            }
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
            ExpandableInfo serInfo = EnsureExpandableInfo(this, property, info, parent);
            float basicHeight =
                serInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(serInfo.Error, width, MessageType.Error);

            if (!property.isExpanded || serInfo.SerializedObject == null)
            {
                return basicHeight;
            }

            serInfo.SerializedObject.UpdateIfRequiredOrScript();

            if (serInfo.UseEditorIMGUI)
            {
                return basicHeight + Mathf.Max(EditorGUIUtility.singleLineHeight, serInfo.EditorHeight);
            }

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
            FieldInfo info, object parent)
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
                if (serInfo.UseEditorIMGUI)
                {
                    float childHeight = Mathf.Max(EditorGUIUtility.singleLineHeight, serInfo.EditorHeight);
                    (Rect childRect, Rect leftOutRect) = RectUtils.SplitHeightRect(expandabledRect, childHeight);
                    DrawEditorIMGUI(childRect, serInfo, property);
                    return leftOutRect;
                }

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

        private static void DrawEditorIMGUI(Rect position, ExpandableInfo serInfo, SerializedProperty property)
        {
            if (serInfo.Editor == null)
            {
                return;
            }

            const float MaxMeasuredEditorHeight = 100000f;
            Rect hostRect = position;
            float fallbackWidth = EditorGUIUtility.currentViewWidth > hostRect.x
                ? EditorGUIUtility.currentViewWidth - hostRect.x - 4f
                : 0f;
            float hostWidth = Mathf.Max(1f, hostRect.width, fallbackWidth);
            hostRect.width = hostWidth;
            Rect localArea = new Rect(0, 0, hostWidth, MaxMeasuredEditorHeight);
            GUILayoutOption[] fixedWidthOptions =
            {
                GUILayout.Width(hostWidth),
                GUILayout.MinWidth(hostWidth),
                GUILayout.MaxWidth(hostWidth),
            };

            bool groupStarted = false;
            bool areaStarted = false;
            bool verticalStarted = false;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            bool oldWideMode = EditorGUIUtility.wideMode;

            try
            {
                GUI.BeginGroup(hostRect);
                groupStarted = true;

                GUILayout.BeginArea(localArea, GUIContent.none, GUIStyle.none);
                areaStarted = true;

                GUILayout.BeginVertical(GUIStyle.none, fixedWidthOptions);
                verticalStarted = true;

                EditorGUIUtility.wideMode = hostWidth > 330f;
                EditorGUIUtility.labelWidth = Mathf.Min(oldLabelWidth, Mathf.Max(120f, hostWidth * 0.45f));

                serInfo.Editor.OnInspectorGUI();

                Rect lastRect = Event.current.type == EventType.Repaint
                    ? GUILayoutUtility.GetLastRect()
                    : default;

                GUILayout.EndVertical();
                verticalStarted = false;

                if (Event.current.type == EventType.Repaint)
                {
                    float newHeight = Mathf.Max(EditorGUIUtility.singleLineHeight, Mathf.Ceil(lastRect.yMax));
                    bool heightChanged = Mathf.Abs(serInfo.EditorHeight - newHeight) > 0.5f;
                    bool widthChanged = Mathf.Abs(serInfo.EditorWidth - position.width) > 0.5f;

                    if (heightChanged || widthChanged)
                    {
                        serInfo.EditorHeight = newHeight;
                        serInfo.EditorWidth = position.width;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
            }
            catch (ArgumentException)
            {
                // Some built-in editors assume they own the inspector layout. Keep the drawer alive.
            }
            catch (NullReferenceException)
            {
                CleanAndRemove(property);
            }
            catch (ObjectDisposedException)
            {
                CleanAndRemove(property);
            }
            finally
            {
                EditorGUIUtility.labelWidth = oldLabelWidth;
                EditorGUIUtility.wideMode = oldWideMode;

                if (verticalStarted)
                {
                    GUILayout.EndVertical();
                }

                if (areaStarted)
                {
                    GUILayout.EndArea();
                }

                if (groupStarted)
                {
                    GUI.EndGroup();
                }
            }
        }

        private static void CleanAndRemove(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!IdToInfo.TryGetValue(key, out ExpandableInfo info))
            {
                return;
            }

            DisposeInfo(info);
            IdToInfo.Remove(key);
        }
    }
}
