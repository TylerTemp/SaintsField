using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner
{
    // [CustomEditor(typeof(SaintsEditorWindowSpecialEditor))]
    public class AutoRunnerWindow: SaintsEditorWindow
    {
        private const string EditorResourcePath = "SaintsField/AutoRunner.asset";

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Auto Runner")]
#else
        // [MenuItem("Window/Saints/Auto Runner")]
#endif
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<AutoRunnerWindow>(false, "SaintsField Auto Runner");
            window.Show();
        }

        private bool _isFromFile;

        // [Button]
        public override Object GetTarget()
        {
            AutoRunnerWindow autoRunnerWindow = EditorGUIUtility.Load(EditorResourcePath) as AutoRunnerWindow;
            Debug.Log($"load: {autoRunnerWindow}");
            if (autoRunnerWindow == null)
            {
                _isFromFile = false;
                return this;
            }

            _isFromFile = true;
            return autoRunnerWindow;
        }

        [LeftToggle] public bool buildingScenes;

        [ShowInInspector, PlayaShowIf(nameof(buildingScenes))]
        private SceneAsset[] InBuildScenes => EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(each => AssetDatabase.LoadAssetAtPath<SceneAsset>(each.path))
            .ToArray();

        public SceneAsset[] sceneList = {};

        private IReadOnlyDictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>> _typeToDrawer;

        [Button("Run!")]
        // ReSharper disable once UnusedMember.Local
        private IEnumerator RunAutoRunners()
        {
            string[] scenePaths = sceneList
                .Select(AssetDatabase.GetAssetPath)
                .Concat((buildingScenes
                    ? EditorBuildSettings.scenes.Where(each => each.enabled).Select(each => each.path)
                    : Array.Empty<string>()))
                .ToArray();

            List<AutoRunnerResult> autoRunnerResults = new List<AutoRunnerResult>();

            foreach (string scenePath in scenePaths)
            {
                Debug.Log($"Processing {scenePath}");
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                {
                    Debug.Log(rootGameObject);
                    foreach (Component comp in rootGameObject.transform.GetComponentsInChildren<Component>(true))
                    {
                        yield return null;
                        SerializedObject so;
                        try
                        {
                            so = new SerializedObject(comp);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }

                        bool hasFixer = false;

                        SerializedProperty property = so.GetIterator();
                        while (property.NextVisible(true))
                        {
                            yield return null;
                            // Debug.Log(iterator.propertyPath);
                            (SerializedUtils.FieldOrProp fieldOrProp, object parent) info;
                            try
                            {
                                info = SerializedUtils.GetFieldInfoAndDirectParent(property);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            MemberInfo memberInfo = info.fieldOrProp.IsField
                                // ReSharper disable once RedundantCast
                                ? (MemberInfo)info.fieldOrProp.FieldInfo
                                : info.fieldOrProp.PropertyInfo;

                            PropertyAttribute[] saintsAttribute = memberInfo.GetCustomAttributes()
                                .OfType<PropertyAttribute>()
                                .Where(each => each is ISaintsAttribute)
                                .ToArray();
                            foreach (PropertyAttribute saintsPropertyAttribute in saintsAttribute)
                            {
                                yield return null;
                                if (!_typeToDrawer.TryGetValue(saintsPropertyAttribute.GetType(), out IReadOnlyList<(bool isSaints, Type drawerType)> drawers))
                                {
                                    continue;
                                }

                                foreach (Type drawerType in drawers.Where(each => each.isSaints).Select(each => each.drawerType))
                                {
                                    yield return null;
                                    SaintsPropertyDrawer saintsPropertyDrawer = (SaintsPropertyDrawer)Activator.CreateInstance(drawerType);
                                    if (saintsPropertyDrawer is IAutoRunnerDrawer autoRunnerDrawer)
                                    {
                                        // Debug.Log($"{property.propertyPath}/{autoRunnerDrawer}");
                                        SerializedProperty prop = property.Copy();
                                        AutoRunnerFixerResult autoRunnerResult =
                                            autoRunnerDrawer.AutoRun(prop, memberInfo, info.parent);
                                        if(autoRunnerResult != null)
                                        {
                                            hasFixer = true;
                                            // Debug.Log(autoRunnerResult.Error);
                                            autoRunnerResults.Add(new AutoRunnerResult
                                            {
                                                FixerResult = autoRunnerResult,
                                                MainTarget = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath),
                                                SubTarget = comp,
                                                SerializedProperty = prop,
                                                SerializedObject = so,
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        if (!hasFixer)
                        {
                            so.Dispose();
                        }
                    }
                }

                yield return null;
            }

            EditorUtility.SetDirty(EditorInspectingTarget == null? this: EditorInspectingTarget);
            results = autoRunnerResults.ToArray();
        }

        [Button("Save To Project"), PlayaHideIf(nameof(_isFromFile))]
        // ReSharper disable once UnusedMember.Local
        private void SaveToProject()
        {
            if (!Directory.Exists("Assets/Editor Default Resources"))
            {
                Debug.Log($"Create folder: Assets/Editor Default Resources");
                AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
            }

            if (!Directory.Exists("Assets/Editor Default Resources/SaintsField"))
            {
                Debug.Log($"Create folder: Assets/Editor Default Resources/SaintsField");
                AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsField");
            }

            Debug.Log(
                $"Create saintsFieldConfig: Assets/Editor Default Resources/{EditorResourcePath}");
            AutoRunnerWindow copy = Instantiate(this);
            copy.results = Array.Empty<AutoRunnerResult>();
            AssetDatabase.CreateAsset(copy, $"Assets/Editor Default Resources/{EditorResourcePath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Reset target to saved file");
            EditorRefreshTarget();
            Selection.activeObject = EditorInspectingTarget;
        }

        // public AutoRunnerResult result = new AutoRunnerResult();
        public AutoRunnerResult[] results = {};

        public override void OnEditorEnable()
        {
            _typeToDrawer = SaintsPropertyDrawer.EnsureAndGetTypeToDrawers();
        }

        public override void OnEditorDestroy()
        {
            // Debug.Log(EditorInspectingTarget);
            // Debug.Log(((AutoRunnerWindow)EditorInspectingTarget).buildingScenes);
            // EditorUtility.SetDirty(EditorInspectingTarget);
            foreach (AutoRunnerResult autoRunnerResult in results)
            {
                try
                {
                    autoRunnerResult.SerializedObject.Dispose();
                }
                catch (Exception)
                {
                    // do nothing
                }
            }

            results = Array.Empty<AutoRunnerResult>();
        }
    }
}
