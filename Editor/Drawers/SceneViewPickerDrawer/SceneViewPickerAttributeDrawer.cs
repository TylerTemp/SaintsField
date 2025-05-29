using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.HandleDrawers;
using SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
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
                return DistanceSqrt.CompareTo(other.DistanceSqrt);
            }
        }

        private struct PickingInfo
        {
            public readonly SerializedProperty ExpectedProperty;
            public readonly List<FindTargetRecord> FoundTargets;
            public readonly Action StopPicking;

            public PickingInfo(SerializedProperty expectedProperty, List<FindTargetRecord> foundTargets, Action stopPicking)
            {
                ExpectedProperty = expectedProperty;
                FoundTargets = foundTargets;
                StopPicking = stopPicking;
            }
        }

        private PickingInfo _pickingInfo;

        private static void OnSceneGUIInternal(SceneView sceneView, PickingInfo pickingInfo)
        {

            if (pickingInfo.FoundTargets.Count == 0)
            {
                return;
            }

            if (Event.current.type == EventType.MouseMove)
            {
                // Debug.Log("Repaint");
                sceneView.Repaint();
            }

            Vector2 mousePosGui = Event.current.mousePosition;

            foreach (FindTargetRecord findTargetRecord in pickingInfo.FoundTargets)
            {
                Vector3 objScreenPos = sceneView.camera.WorldToScreenPoint(findTargetRecord.FindTargetInfo.Transform.position);
                bool inView = objScreenPos.z >= 0;
                findTargetRecord.InView = inView;
                if (inView)
                {
                    findTargetRecord.DistanceSqrt = (objScreenPos - (Vector3)mousePosGui).sqrMagnitude;
                }
            }
            pickingInfo.FoundTargets.Sort();
            FindTargetRecord firstPicking = pickingInfo.FoundTargets[0];

            Vector3 mouseWorld = HandleUtility.GUIPointToWorldRay(mousePosGui)
                .GetPoint(10);

            using (new HandleColorScoop(new Color(1, 1, 1, 0.75f)))
            {
                Handles.DrawDottedLine(firstPicking.FindTargetInfo.Transform.position, mouseWorld, 2.0f);
            }

            using(new HandlesBeginGUIScoop())
            {
                int foundCount = 1;
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
                Vector2 labelSize = GUI.skin.label.CalcSize(labelContent);
                // labelSize += Vector2.one * 4;
                // Rect nameRect = new Rect(
                //     Event.current.mousePosition + Vector2.down * 10 - labelSize * 0.5f, labelSize);
                Rect nameRect = new Rect(
                    mousePosGui.x - labelSize.x / 2,
                    mousePosGui.y + labelSize.y,
                    labelSize.x + 15,
                    labelSize.y + 2);

                EditorGUI.DropShadowLabel(nameRect, labelContent);
            }

            if (Event.current.type != EventType.MouseDown || Event.current.alt ||
                Event.current.control)
            {
                return;
            }

            if (Event.current.button == 0)
            {
                pickingInfo.ExpectedProperty.objectReferenceValue = firstPicking.FindTargetInfo.Target;
                pickingInfo.ExpectedProperty.serializedObject.ApplyModifiedProperties();
                pickingInfo.StopPicking.Invoke();
                sceneView.Repaint();

                // EditorApplication.delayCall += () => Selection.activeObject = pickingInfo.ExpectedProperty.serializedObject.targetObject;
            }
            else if (Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu
                {
                    allowDuplicateNames = true,
                };

                for (int index = 1; index < pickingInfo.FoundTargets.Count; index++)
                {
                    FindTargetRecord checkTarget = pickingInfo.FoundTargets[index];
                    if (!checkTarget.InView)
                    {
                        continue;
                    }
                    // Declare this so it is referenced correctly in the anonymous method passed to the menu.
                    // Candidate candidate = nearbyCandidates[i];

                    menu.AddItem(new GUIContent(checkTarget.FindTargetInfo.Path), false, () =>
                    {
                        pickingInfo.ExpectedProperty.objectReferenceValue = checkTarget.FindTargetInfo.Target;
                        pickingInfo.ExpectedProperty.serializedObject.ApplyModifiedProperties();
                        pickingInfo.StopPicking.Invoke();
                        sceneView.Repaint();
                    });
                }

                menu.ShowAsContext();
            }
            else
            {
                pickingInfo.StopPicking.Invoke();
                sceneView.Repaint();
            }
        }

        private static Scene GetScene(Object target)
        {
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

            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
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

            if (typeof(Component).IsAssignableFrom(expectFieldType))
            {
                foundTargets = FindComponentInRoots(rootGameObjects, expectFieldType, expectInterfaceType)
                    .Select(each => new FindTargetRecord
                    {
                        FindTargetInfo = each,
                    })
                    .ToList();
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

            return new PickingInfo(expectProperty, foundTargets, stopPicking);
        }

        private static IEnumerable<FindTargetInfo> FindComponentInRoots(GameObject[] rootGameObjects, Type expectFieldType, Type expectInterfaceType)
        {
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                Component[] components = rootGameObject.GetComponentsInChildren(expectFieldType, true);
                if (components.Length > 0)
                {
                    foreach (Component component in components)
                    {
                        if (expectInterfaceType != null && !expectInterfaceType.IsAssignableFrom(component.GetType()))
                        {
                            continue;
                        }

                        yield return new FindTargetInfo(component, component.name, component.transform,
                            GetHierarchyPath(component.gameObject));
                    }
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
            return string.Join("\\", pathParts);
        }

        private static IEnumerable<FindTargetInfo> FindAllGameObject(IEnumerable<GameObject> rootGameObjects, string prefix = "")
        {
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                string thisName = prefix == "" ? rootGameObject.name : $"{prefix}\\{rootGameObject.name}";

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
