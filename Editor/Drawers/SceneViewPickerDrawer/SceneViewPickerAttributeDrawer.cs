using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.HandleDrawers;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SceneViewPickerAttribute), true)]
    public partial class SceneViewPickerAttributeDrawer: SaintsPropertyDrawer
    {
        private static readonly UnityEvent StopAllPicking = new UnityEvent();

        private static GUIStyle _leftButtonStyleCache;

        private static GUIStyle LeftButtonStyle
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_leftButtonStyleCache == null)
                {
                    _leftButtonStyleCache = new GUIStyle(GUI.skin.button)
                    {
                        border = new RectOffset(0, 0, 0, 0),
                        alignment = TextAnchor.MiddleLeft,
                        // padding = new RectOffset(0, 0, 0, 0),
                        // normal = { background = null },
                        // hover = { background = null },
                        // active = { background = null },
                        // focused = { background = null },
                    };
                }

                return _leftButtonStyleCache;
            }
        }

        private readonly struct FindTargetInfo
        {
            public readonly Object Target;
            public readonly string DisplayName;

            public readonly Transform Transform;
            public readonly string Path;

            public FindTargetInfo(Object target, string displayName, Transform transform, string path)
            {
                Target = target;
                DisplayName = displayName;
                Transform = transform;
                Path = path;
            }
        }

        private class FindTargetRecord: IComparable<FindTargetRecord>
        {
            public FindTargetInfo FindTargetInfo;
            public bool InView;
            public float DistanceSqrt;
            public Texture2D Icon;

            public int IconLoadCount;

            public int CompareTo(FindTargetRecord other)
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (InView && !other.InView)
                {
                    return -1;
                }
                if (!InView && other.InView)
                {
                    return 1;
                }
                int distanceSqrt = DistanceSqrt.CompareTo(other.DistanceSqrt);
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (distanceSqrt != 0)
                {
                    return distanceSqrt;
                }

                return string.Compare(FindTargetInfo.Path, other.FindTargetInfo.Path, StringComparison.Ordinal);
            }
        }

        private struct PickingInfo
        {
            public readonly SerializedProperty ExpectedProperty;
            public readonly string ExpectedPropertyDisplayName;
            public readonly List<FindTargetRecord> FoundTargets;
            public readonly Action StopPicking;

            public PickingInfo(string displayName, SerializedProperty expectedProperty, List<FindTargetRecord> foundTargets, Action stopPicking)
            {
                ExpectedPropertyDisplayName = displayName;
                ExpectedProperty = expectedProperty;
                FoundTargets = foundTargets;
                StopPicking = stopPicking;
            }
        }

        private PickingInfo _pickingInfo;

        private static bool _showSelectingPanel;
        // private static Vector2 _selectingPanelPos;
        private static Vector2 _selectingPanelMouseFrozenPos;
        private static string _selectingPanelSearching = "";
        private static Vector2 _scrollPos;
        private static float _selectingPanelWidth = -1f;

        private static void OnSceneGUIInternal(SceneView sceneView, PickingInfo pickingInfo)
        {
            if (!SerializedUtils.IsOk(pickingInfo.ExpectedProperty))
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning("Property disposed");
#endif
                pickingInfo.StopPicking();
                return;
            }

            if (pickingInfo.FoundTargets.Count == 0)
            {
                using (new HandlesBeginGUIScoop())
                {
                    const string label = "No Results";
                    GUIContent labelContent = new GUIContent(label);
                    GUIStyle guiStyle = new GUIStyle("PreOverlayLabel")
                    {
                        normal =
                        {
                            textColor = Color.red,
                        },
                    };
                    Vector2 labelSize = guiStyle.CalcSize(labelContent);
                    Rect nameRect = new Rect(
                        Event.current.mousePosition.x - labelSize.x / 2,
                        Event.current.mousePosition.y + labelSize.y,
                        labelSize.x + 15,
                        labelSize.y + 2);

                    EditorGUI.DropShadowLabel(nameRect, labelContent, guiStyle);
                }

                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp)
                {
                    pickingInfo.StopPicking();
                    Event.current.Use();
                }

                sceneView.Repaint();

                return;
            }

            if (Event.current.type == EventType.MouseMove)
            {
                // Debug.Log("Repaint");
                sceneView.Repaint();
            }

            if(!_showSelectingPanel)
            {
                _selectingPanelMouseFrozenPos = Event.current.mousePosition;
                foreach (FindTargetRecord findTargetRecord in pickingInfo.FoundTargets)
                {
                    Vector3 objScreenPos = sceneView.camera.WorldToScreenPoint(findTargetRecord.FindTargetInfo.Transform.position);
                    bool inView = objScreenPos.z >= 0;
                    findTargetRecord.InView = inView;
                    if (inView)
                    {
                        findTargetRecord.DistanceSqrt = (objScreenPos - (Vector3)_selectingPanelMouseFrozenPos).sqrMagnitude;
                    }
                }
                pickingInfo.FoundTargets.Sort();
            }

            FindTargetRecord firstPicking = pickingInfo.FoundTargets[0];

            Vector3 mouseWorld = HandleUtility.GUIPointToWorldRay(_selectingPanelMouseFrozenPos)
                .GetPoint(10);

            const float alpha = 0.75f;
            Color useColor = Util.GetIsEqual(firstPicking.FindTargetInfo.Target,
                pickingInfo.ExpectedProperty.objectReferenceValue)
                ? Color.gray
                : Color.white;
            useColor.a = alpha;

            using (new HandleColorScoop(useColor))
            {
                Handles.DrawDottedLine(firstPicking.FindTargetInfo.Transform.position, mouseWorld, 2.0f);
            }

            int foundCount = 1;

            using(new HandlesBeginGUIScoop())
            {
                for (int index = 1; index < pickingInfo.FoundTargets.Count; index++)
                {
                    FindTargetRecord checkTarget = pickingInfo.FoundTargets[index];
                    if (!checkTarget.InView)
                    {
                        break;
                    }

                    foundCount++;
                }

                string label = foundCount > 1
                    ? $"{firstPicking.FindTargetInfo.DisplayName} [+{foundCount - 1}]"
                    : firstPicking.FindTargetInfo.DisplayName;
                GUIContent labelContent = new GUIContent(label);
                useColor.a = 1f;
                GUIStyle guiStyle = new GUIStyle("PreOverlayLabel")
                {
                    normal =
                    {
                        textColor = useColor,
                    },
                };

                Vector2 labelSize = guiStyle.CalcSize(labelContent);
                // labelSize += Vector2.one * 4;
                // Rect nameRect = new Rect(
                //     Event.current.mousePosition + Vector2.down * 10 - labelSize * 0.5f, labelSize);
                Rect nameRect = new Rect(
                    _selectingPanelMouseFrozenPos.x - labelSize.x / 2,
                    _selectingPanelMouseFrozenPos.y + labelSize.y,
                    labelSize.x + 15,
                    labelSize.y + 2);

                EditorGUI.DropShadowLabel(nameRect, labelContent, guiStyle);

                if (_showSelectingPanel)
                {
                    float calcWidth = 100;
                    float height = EditorGUIUtility.singleLineHeight + 4;
                    List<FindTargetRecord> showTargets = new List<FindTargetRecord>();
                    foreach (FindTargetRecord findTargetRecord in pickingInfo.FoundTargets)
                    {
                        if (!findTargetRecord.InView)
                        {
                            break;
                        }

                        if(string.IsNullOrEmpty(_selectingPanelSearching) || _selectingPanelSearching.ToLower().Split(' ').All(searchSeg => findTargetRecord.FindTargetInfo.Path.ToLower().Contains(searchSeg)))
                        {
                            showTargets.Add(findTargetRecord);

                            string name = findTargetRecord.FindTargetInfo.Path;
                            if(_selectingPanelWidth < 0)
                            {
                                float thisWidth = GUI.skin.button.CalcSize(new GUIContent(name, findTargetRecord.Icon)).x;
                                if (thisWidth > calcWidth)
                                {
                                    calcWidth = Mathf.Min(thisWidth, 400);
                                }
                            }

                            height += EditorGUIUtility.singleLineHeight;
                        }

                        if (findTargetRecord.IconLoadCount < 30 && findTargetRecord.Icon is null)
                        {
                            Texture2D icon = AssetPreview.GetMiniThumbnail(findTargetRecord.FindTargetInfo.Target);
                            if (icon)
                            {
                                findTargetRecord.Icon = icon;
                                findTargetRecord.IconLoadCount = int.MaxValue;
                            }
                            else
                            {
                                findTargetRecord.IconLoadCount++;
                            }
                        }
                    }

                    if (_selectingPanelWidth < 0)
                    {
                        _selectingPanelWidth = calcWidth;
                    }

                    float useX = _selectingPanelMouseFrozenPos.x - _selectingPanelWidth / 2;
                    if (useX + _selectingPanelWidth > Screen.width)
                    {
                        useX = Screen.width - _selectingPanelWidth;
                    }
                    else if (useX < 0)
                    {
                        useX = 0;
                    }

                    float useY = _selectingPanelMouseFrozenPos.y -
                                 (EditorGUIUtility.singleLineHeight + 4 + EditorGUIUtility.singleLineHeight / 2);
                    float viewHeight = Mathf.Min(600, height, Screen.height - useY - 100);
                    if (viewHeight < 100)
                    {
                        viewHeight = 100;
                    }

                    GUI.Window("SceneViewPickerAttributeDrawer".GetHashCode(), new Rect(useX, useY, _selectingPanelWidth, viewHeight), _ =>
                    {
                        Rect search = new Rect(2, 2, _selectingPanelWidth - 4, EditorGUIUtility.singleLineHeight);
                        GUI.SetNextControlName("SceneViewPickerAttributeDrawerSearchField");
                        _selectingPanelSearching = GUI.TextField(search, _selectingPanelSearching);
                        // Content of window here
                        // GUILayout.Button("A Button");

                        using (GUIBeginScrollViewScoop scrollView = new GUIBeginScrollViewScoop(
                                   new Rect(0, EditorGUIUtility.singleLineHeight + 4, _selectingPanelWidth, viewHeight - (EditorGUIUtility.singleLineHeight + 4)),
                                   _scrollPos,
                                   new Rect(0, EditorGUIUtility.singleLineHeight + 4, _selectingPanelWidth - 15, height - (EditorGUIUtility.singleLineHeight + 4)))
                               )
                        {
                            _scrollPos = scrollView.ScrollPosition;
                            int index = 0;
                            foreach (FindTargetRecord findTargetRecord in showTargets)
                            {
                                Rect buttonRect = new Rect(0,
                                    EditorGUIUtility.singleLineHeight + 4 + EditorGUIUtility.singleLineHeight * index,
                                    _selectingPanelWidth, EditorGUIUtility.singleLineHeight);

                                if (viewHeight < height)
                                {
                                    buttonRect.width -= 15;
                                }

                                bool isSelected = Util.GetIsEqual(findTargetRecord.FindTargetInfo.Target,
                                    pickingInfo.ExpectedProperty.objectReferenceValue);

                                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                                {
                                    GUI.Toggle(buttonRect, isSelected, new GUIContent(findTargetRecord.FindTargetInfo.Path, findTargetRecord.Icon),
                                        LeftButtonStyle);
                                    if (changed.changed)
                                    {
                                        if (pickingInfo.ExpectedProperty.objectReferenceValue !=
                                            findTargetRecord.FindTargetInfo.Target)
                                        {
                                            pickingInfo.ExpectedProperty.objectReferenceValue =
                                                findTargetRecord.FindTargetInfo.Target;
                                            pickingInfo.ExpectedProperty.serializedObject.ApplyModifiedProperties();
                                            EnqueueSceneViewNotification(
                                                $"Sign {findTargetRecord.FindTargetInfo.DisplayName} to {pickingInfo.ExpectedPropertyDisplayName}");
                                        }

                                        pickingInfo.StopPicking.Invoke();
                                        sceneView.Repaint();
                                        return;
                                    }
                                }

                                index++;
                            }
                        }

                        if (Event.current.type == EventType.Repaint) {
                            GUI.FocusControl("SceneViewPickerAttributeDrawerSearchField");
                            // shouldFocus = false;
                        }
                    }, "Title");

                }
            }

            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
            {
                pickingInfo.StopPicking.Invoke();
                sceneView.Repaint();
                Event.current.Use();
            }

            if (Event.current.type != EventType.MouseUp || Event.current.alt ||
                Event.current.control)
            {
                return;
            }

            if (Event.current.button == 0)
            {
                if (foundCount > 1)
                {
                    if (!_showSelectingPanel)
                    {
                        _selectingPanelMouseFrozenPos = Event.current.mousePosition;
                        _selectingPanelWidth = -1f;
                    }
                    _showSelectingPanel = !_showSelectingPanel;
                }
                else
                {
                    _showSelectingPanel = false;
                    if (pickingInfo.ExpectedProperty.objectReferenceValue != firstPicking.FindTargetInfo.Target)
                    {
                        pickingInfo.ExpectedProperty.objectReferenceValue = firstPicking.FindTargetInfo.Target;
                        pickingInfo.ExpectedProperty.serializedObject.ApplyModifiedProperties();
                        EnqueueSceneViewNotification(
                            $"Sign {firstPicking.FindTargetInfo.DisplayName} to {pickingInfo.ExpectedPropertyDisplayName}");
                    }

                    pickingInfo.StopPicking.Invoke();
                    sceneView.Repaint();
                }
                Event.current.Use();
            }
            else if (Event.current.button == 1)
            {
                _showSelectingPanel = false;
            }
            else if (Event.current.button == 2)
            {
                _showSelectingPanel = false;
                pickingInfo.StopPicking.Invoke();
                sceneView.Repaint();
                Event.current.Use();
            }
        }

        private static Scene GetScene(Object target)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (target)
            {
                case Component component:
                    return component.gameObject.scene;
                case GameObject gameObject:
                    return gameObject.scene;
                default:
                    return default;
            }
        }

        private static PickingInfo InitPickingInfo(SerializedProperty property, FieldInfo info, Action stopPicking)
        {
            Scene currentScene = GetScene(property.serializedObject.targetObject);
            Debug.Assert(currentScene.IsValid(), property.propertyPath);

            int propIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            bool inArray = propIndex >= 0;
            Type rawType = inArray
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

            Type expectFieldType = rawType;
            Type expectInterfaceType = null;
            SerializedProperty expectProperty = property;

            (Type saintsValueType, Type saintsInterfaceType) = SaintsInterfaceDrawer.GetTypes(property, info);
            bool isSaintsInterface = saintsInterfaceType != null;
            if (saintsInterfaceType != null)
            {
                expectFieldType = saintsValueType;
                expectInterfaceType = saintsInterfaceType;
            }

            string wrapPropName = ReflectUtils.GetIWrapPropName(rawType);
            bool isWrap = wrapPropName != null;
            if (isWrap)
            {
                if(!isSaintsInterface)
                {
                    expectFieldType = ReflectUtils.GetIWrapPropType(rawType, wrapPropName);
                }
                expectProperty = property.FindPropertyRelative(wrapPropName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, wrapPropName);
            }

            GameObject[] rootGameObjects = currentScene.GetRootGameObjects();

            List<FindTargetRecord> foundTargets;

            bool expectUObjType = expectFieldType == typeof(Object);

            if (typeof(Component).IsAssignableFrom(expectFieldType) || expectUObjType)
            {
                Type compType = expectUObjType
                    ? typeof(Component)
                    : expectFieldType;

                // Debug.Log($"get component {compType}");

                foundTargets = FindComponentInRoots(rootGameObjects, compType, expectInterfaceType)
                    .Select(each => new FindTargetRecord
                    {
                        FindTargetInfo = each,
                    })
                    .ToList();

                if (expectUObjType && expectInterfaceType == null)
                {
                    foundTargets.AddRange(FindAllGameObject(rootGameObjects)
                        .Select(each => new FindTargetRecord
                        {
                            FindTargetInfo = each,
                        }));
                }
            }
            else if(expectFieldType == typeof(GameObject))
            {
                Debug.Assert(expectInterfaceType == null, "expectInterfaceType should be null when expectFieldType is GameObject");
                foundTargets = FindAllGameObject(rootGameObjects)
                    .Select(each => new FindTargetRecord
                    {
                        FindTargetInfo = each,
                    })
                    .ToList();
                // foundTargets = FindComponentInRoots(rootGameObjects, expectFieldType, expectInterfaceType).ToList();
            }
            else
            {
                foundTargets = new List<FindTargetRecord>();
            }

            string displayName;
            if (inArray)
            {
                string propPath = property.propertyPath;
                string[] propSplit = propPath.Split('.');
                displayName = $"{ObjectNames.NicifyVariableName(propSplit[0])}[{propIndex}]";
            }
            else
            {
                displayName = property.displayName;
            }

            return new PickingInfo(displayName, expectProperty, foundTargets, stopPicking);
        }

        private static IEnumerable<FindTargetInfo> FindComponentInRoots(GameObject[] rootGameObjects, Type expectFieldType, Type expectInterfaceType)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                foreach (Component component in rootGameObject.GetComponentsInChildren(expectFieldType, true))
                {
                    if (!component)
                    {
                        continue;
                    }

                    if (expectInterfaceType != null && !expectInterfaceType.IsAssignableFrom(component.GetType()))
                    {
                        continue;
                    }

                    yield return new FindTargetInfo(component, component.name, component.transform,
                        GetHierarchyPath(component.gameObject));
                }
            }
        }

        private static string GetHierarchyPath(GameObject obj)
        {
            List<string> pathParts = new List<string>()
            {
                obj.name,
            };
            Transform current = obj.transform.parent;
            while (current)
            {
                pathParts.Add(current.name);
                // path = current.name + "/" + path;
                current = current.parent;
            }
            pathParts.Reverse();
            return string.Join("/", pathParts);
        }

        private static IEnumerable<FindTargetInfo> FindAllGameObject(IEnumerable<GameObject> rootGameObjects, string prefix = "")
        {
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                string thisName = prefix == "" ? rootGameObject.name : $"{prefix}/{rootGameObject.name}";

                yield return new FindTargetInfo(rootGameObject, rootGameObject.name, rootGameObject.transform, thisName);

                List<GameObject> children = new List<GameObject>();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (Transform directChild in rootGameObject.transform)
                {
                    children.Add(directChild.gameObject);
                }

                foreach (FindTargetInfo findTargetInfo in FindAllGameObject(children, $"{thisName}"))
                {
                    yield return findTargetInfo;
                }
            }
        }
    }
}
