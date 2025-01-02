using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private Object[] InBuildScenes => EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(each => AssetDatabase.LoadAssetAtPath<Object>(each.path))
            .ToArray();

        [Button("Run!")]
        private IEnumerator RunAutoRunners()
        {
            foreach (EditorBuildSettingsScene editorBuildSettingsScene in EditorBuildSettings.scenes)
            {
                string scenePath = editorBuildSettingsScene.path;
                Debug.Log($"Processing {scenePath}");
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                {
                    // Debug.Log(rootGameObject);
                    foreach (Component comp in rootGameObject.transform.GetComponentsInChildren<Component>(true))
                    {
                        SerializedObject so;
                        try
                        {
                            so = new SerializedObject(comp);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }

                        using(so)
                        {
                            SerializedProperty iterator = so.GetIterator();
                            while (iterator.NextVisible(true))
                            {
                                // Debug.Log(iterator.propertyPath);
                                (SerializedUtils.FieldOrProp fieldOrProp, object parent) info;
                                try
                                {
                                    info = SerializedUtils.GetFieldInfoAndDirectParent(iterator);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                MemberInfo memberInfo = info.fieldOrProp.IsField
                                    ? info.fieldOrProp.FieldInfo
                                    : info.fieldOrProp.PropertyInfo;
                                PropertyAttribute[] attributes = memberInfo.GetCustomAttributes()
                                    .OfType<PropertyAttribute>()
                                    .ToArray();
                                if (attributes.Any(each => each is RequiredAttribute))
                                {
                                    Debug.Log(iterator.propertyPath);
                                }
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        [Button("Save To Project"), PlayaHideIf(nameof(_isFromFile))]
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
            AssetDatabase.CreateAsset(copy, $"Assets/Editor Default Resources/{EditorResourcePath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Reset target to saved file");
            EditorRefreshTarget();
            Selection.activeObject = EditorInspectingTarget;
        }

        public override void OnEditorDestroy()
        {
            Debug.Log(EditorInspectingTarget);
            Debug.Log(((AutoRunnerWindow)EditorInspectingTarget).buildingScenes);
            EditorUtility.SetDirty(EditorInspectingTarget);
        }
    }
}
