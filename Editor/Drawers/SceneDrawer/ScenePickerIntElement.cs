using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public class ScenePickerIntElement: ScenePickerBaseElement<int>
    {
        private readonly struct ScenePickerIntPayload : IScenePickerPayload
        {
            public string Name { get; }
            public readonly int Index;

            public bool IsSceneAsset(SceneAsset sceneAsset)
            {
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                foreach ((EditorBuildSettingsScene value, int index) in EditorBuildSettings.scenes.Where(each => each.enabled).WithIndex())
                {
                    if (value.path == scenePath && index == Index)
                    {
                        return true;
                    }
                }

                return false;
            }

            public ScenePickerIntPayload(string name, int index)
            {
                Name = name;
                Index = index;
            }
        }

        private readonly bool _fullPath;

        public ScenePickerIntElement(SceneAttribute sceneAttribute)
        {
            _fullPath = sceneAttribute.FullPath;
        }

        private SceneHelpBox _sceneHelpBox;

        public void BindStringHelpBox(SceneHelpBox sceneHelpBox)
        {
            Debug.Assert(sceneHelpBox != null);
            _sceneHelpBox = sceneHelpBox;

            UpdateHelpBoxError();

            sceneHelpBox.EnableClicked.AddListener(() =>
            {
                if (_errorEditorScene == null)
                {
                    return;
                }

                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                string path = _errorEditorScene.path;
                int index = Array.FindIndex(scenes, each => each.path == path);
                if (index != -1)
                {
                    _errorEditorScene.enabled = true;
                    scenes[index] = _errorEditorScene;
                    EditorBuildSettings.scenes = scenes;

                    int newIndex = scenes.Take(index).Count(each => each.enabled);

                    _errorEventInt = -1;
                    _errorEditorScene = null;

                    value = newIndex;
                }
            });

            sceneHelpBox.AddClicked.AddListener(() =>
            {
                if (_errorSceneAsset != null)
                {
                    string newPath = AssetDatabase.GetAssetPath(_errorSceneAsset);
                    EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

                    int index = Array.FindIndex(scenes, each => each.path == newPath);
                    if (index == -1)
                    {
                        EditorBuildSettingsScene[] newScenes = scenes.Append(
                            new EditorBuildSettingsScene(newPath,true)).ToArray();
                        EditorBuildSettings.scenes = newScenes;

                        int newIndex = newScenes.Count(each => each.enabled) - 1;

                        _errorEventInt = -1;
                        _errorEditorScene = null;

                        value = newIndex;
                    }
                }
            });
        }

        private void UpdateHelpBoxError()
        {
            if (_sceneHelpBox == null)
            {
                return;
            }

            if (_errorEditorScene != null)
            {
                _sceneHelpBox.text = $"{_errorEditorScene.path} not enabled";
                _sceneHelpBox.style.display = DisplayStyle.Flex;
                _sceneHelpBox.EnableButton.style.display = DisplayStyle.Flex;
                _sceneHelpBox.AddButton.style.display = DisplayStyle.None;
                return;
            }

            if (_errorSceneAsset != null)
            {
                _sceneHelpBox.text = $"{_errorSceneAsset} not in build list";
                _sceneHelpBox.style.display = DisplayStyle.Flex;
                _sceneHelpBox.EnableButton.style.display = DisplayStyle.None;
                _sceneHelpBox.AddButton.style.display = DisplayStyle.Flex;
                return;
            }

            if (_errorEventInt != -1)
            {
                _sceneHelpBox.text = $"{_errorEventInt} not in build list";
                _sceneHelpBox.AddButton.style.display = DisplayStyle.None;

                _sceneHelpBox.style.display = DisplayStyle.Flex;
                _sceneHelpBox.EnableButton.style.display = DisplayStyle.None;
                return;
            }

            _sceneHelpBox.style.display = DisplayStyle.None;
        }

        private int _errorEventInt = -1;
        private EditorBuildSettingsScene _errorEditorScene;
        private SceneAsset _errorSceneAsset;

        protected override bool AllowEmpty() => false;

        protected override IScenePickerPayload MakeEmpty() => throw new Exception();

        protected override bool CurrentIsPayload(IScenePickerPayload payload) => ((ScenePickerIntPayload) payload).Index == value;

        protected override void PostProcessDropdownList(AdvancedDropdownList<IScenePickerPayload> dropdown)
        {
            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", new ScenePickerIntPayload("", -1), false, "d_editicon.sml");
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            HasCachedValue = true;
            CachedValue = newValue;
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes.Where(each => each.enabled).ToArray();
            if (newValue >= scenes.Length || newValue < 0)
            {
                _errorEventInt = newValue;
                UpdateHelpBoxError();
                return;
            }

            EditorBuildSettingsScene scene = scenes[newValue];
            _sceneField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
            _errorEventInt = -1;
            _errorEditorScene = null;
            _errorSceneAsset = null;
            UpdateHelpBoxError();
        }

        protected override IReadOnlyList<IScenePickerPayload> GetScenePickerPayloads()
        {
            List<IScenePickerPayload> result = new List<IScenePickerPayload>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach ((EditorBuildSettingsScene editorScene, int index) in EditorBuildSettings.scenes.Where(each => each.enabled).WithIndex())
            {
                result.Add(new ScenePickerIntPayload(RuntimeUtil.TrimScenePath(editorScene.path, _fullPath), index));
            }

            return result;
        }

        protected override void SetSelectedPayload(IScenePickerPayload scenePickerPayload)
        {
            ScenePickerIntPayload payload = (ScenePickerIntPayload)scenePickerPayload;
            if (payload.Index == -1)
            {
                SceneUtils.OpenBuildSettings();
            }
            else
            {
                value = payload.Index;
            }
        }

        protected override void OnSceneFieldChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue == null)  // This is not allowed, set it back
            {
                SetValueWithoutNotify(value);
                return;
            }

            _errorSceneAsset = null;

            SceneAsset sceneAsset = (SceneAsset)evt.newValue;
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            int index = 0;
            foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
            {

                if (editorScene.enabled)
                {
                    if (scenePath == editorScene.path)
                    {
                        value = index;
                        return;
                    }

                    index += 1;
                }
                else if (scenePath == editorScene.path)  // not enabled
                {
                    _errorEventInt = -1;
                    _errorEditorScene = editorScene;
                    // Debug.Log($"not enabled {ErrorEditorScene}");
                    UpdateHelpBoxError();
                    return;
                }
            }

            // not in list
            _errorEventInt = -1;
            _errorSceneAsset = sceneAsset;
            UpdateHelpBoxError();
        }
    }

    public class ScenePickerIntField : BaseField<int>
    {
        public readonly ScenePickerIntElement ScenePickerIntElement;

        public ScenePickerIntField(string label, ScenePickerIntElement visualInput) : base(label, visualInput)
        {
            visualInput.DropdownRoot = this;
            ScenePickerIntElement = visualInput;
        }

        public override int value
        {
            get => ScenePickerIntElement.value;
            set => ScenePickerIntElement.value = value;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            ScenePickerIntElement.SetValueWithoutNotify(newValue);
        }
    }
}
