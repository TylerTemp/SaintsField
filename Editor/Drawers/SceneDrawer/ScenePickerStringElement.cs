using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public class ScenePickerStringElement: ScenePickerBaseElement<string>
    {
        private SceneHelpBox _sceneHelpBox;

        public void BindStringHelpBox(SceneHelpBox sceneHelpBox)
        {
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
                // ReSharper disable once InvertIf
                if (index != -1)
                {
                    _errorEditorScene.enabled = true;
                    scenes[index] = _errorEditorScene;
                    EditorBuildSettings.scenes = scenes;

                    _errorEventString = "";
                    _errorEditorScene = null;

                    value = RuntimeUtil.TrimScenePath(path, _fullPath);
                }
            });

            sceneHelpBox.AddClicked.AddListener(() =>
            {
                if (_errorSceneAsset != null)
                {
                    AddScenePath(AssetDatabase.GetAssetPath(_errorSceneAsset));
                }

                if (_errorEventString != "")
                {
                    AddScenePath($"Assets/{_errorEventString}.unity");
                }
            });
        }

        private void AddScenePath(string newPath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            int index = Array.FindIndex(scenes, each => each.path == newPath);
            if (index == -1)
            {
                EditorBuildSettingsScene[] newScenes = scenes.Append(
                    new EditorBuildSettingsScene(newPath,true)).ToArray();
                EditorBuildSettings.scenes = newScenes;
                _errorEventString = "";
                _errorEditorScene = null;

                value = RuntimeUtil.TrimScenePath(newPath, _fullPath);
            }
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
                _sceneHelpBox.text = $"{_errorEventString} not in build list";
                _sceneHelpBox.style.display = DisplayStyle.Flex;
                _sceneHelpBox.EnableButton.style.display = DisplayStyle.None;
                _sceneHelpBox.AddButton.style.display = DisplayStyle.Flex;
                return;
            }

            if (_errorEventString != "")
            {
                if (_fullPath)
                {
                    _sceneHelpBox.text = $"{_errorEventString} not in build list";
                    _sceneHelpBox.AddButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _sceneHelpBox.text = $"{_errorEventString} not in build list and we can not find a correct location";
                    _sceneHelpBox.AddButton.style.display = DisplayStyle.None;
                }

                _sceneHelpBox.style.display = DisplayStyle.Flex;
                _sceneHelpBox.EnableButton.style.display = DisplayStyle.None;
                return;
            }

            _sceneHelpBox.style.display = DisplayStyle.None;
        }

        private readonly struct ScenePickerStringPayload : IScenePickerPayload
        {
            public readonly bool IsNormalItem;

            public string Name { get; }

            private readonly bool _isFullPath;

            public bool IsSceneAsset(SceneAsset sceneAsset) =>
                Name == RuntimeUtil.TrimScenePath(AssetDatabase.GetAssetPath(sceneAsset), _isFullPath);

            public ScenePickerStringPayload(bool isFullPath, string scenePath)
            {
                IsNormalItem = true;
                _isFullPath = isFullPath;
                Name = scenePath;
            }
        }

        private readonly bool _fullPath;

        private string _errorEventString = "";
        private EditorBuildSettingsScene _errorEditorScene;
        private SceneAsset _errorSceneAsset;

        public ScenePickerStringElement(SceneAttribute sceneAttribute)
        {
            _fullPath = sceneAttribute.FullPath;
        }

        protected override bool AllowEmpty() => true;

        protected override IScenePickerPayload MakeEmpty() => new ScenePickerStringPayload(_fullPath, "");

        protected override bool CurrentIsPayload(IScenePickerPayload payload) => payload.Name == value;

        protected override void PostProcessDropdownList(AdvancedDropdownList<IScenePickerPayload> dropdown)
        {
            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", new ScenePickerStringPayload(), false, "d_editicon.sml");
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            HasCachedValue = true;
            CachedValue = newValue;
            foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
            {
                string trimPath = RuntimeUtil.TrimScenePath(editorScene.path, _fullPath);
                // Debug.Log($"{newValue} -> {trimPath}/{editorScene.enabled}");
                if (trimPath == newValue)
                {
                    if (!editorScene.enabled)
                    {
                        _errorEventString = newValue;
                        _errorEditorScene = editorScene;
                        UpdateHelpBoxError();
                        // Debug.Log("not enabled");
                        return;
                    }

                    // Debug.Log($"found {editorScene.path}");
                    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(editorScene.path);
                    // Debug.Log($"set {sceneAsset}");
                    _sceneField.SetValueWithoutNotify(sceneAsset);
                    _errorEventString = "";
                    _errorEditorScene = null;
                    _errorSceneAsset = null;
                    UpdateHelpBoxError();
                    return;
                }
            }

            // not found
            _errorEventString = newValue;
            _errorEditorScene = null;
            // Debug.Log("not found");
            UpdateHelpBoxError();
        }

        protected override IReadOnlyList<IScenePickerPayload> GetScenePickerPayloads()
        {
            List<IScenePickerPayload> result = new List<IScenePickerPayload>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
            {
                if (editorScene.enabled)
                {
                    result.Add(new ScenePickerStringPayload(_fullPath, RuntimeUtil.TrimScenePath(editorScene.path, _fullPath)));
                }
            }

            return result;
        }

        protected override void SetSelectedPayload(IScenePickerPayload scenePickerPayload)
        {
            if (!((ScenePickerStringPayload)scenePickerPayload).IsNormalItem)
            {
                SceneUtils.OpenBuildSettings();
            }
            else
            {
                value = scenePickerPayload.Name;
            }
        }

        protected override void OnSceneFieldChanged(ChangeEvent<Object> evt)
        {
            _errorSceneAsset = null;

            if (evt.newValue == null)
            {
                SetValueWithoutNotify(value);  // restore
                return;
            }
            SceneAsset sceneAsset = (SceneAsset)evt.newValue;
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            string toValue = RuntimeUtil.TrimScenePath(scenePath, _fullPath);

            foreach (EditorBuildSettingsScene inBuild in EditorBuildSettings.scenes)
            {
                if (inBuild.path == scenePath)
                {
                    if (!inBuild.enabled)
                    {
                        _errorEventString = toValue;
                        _errorEditorScene = inBuild;
                        UpdateHelpBoxError();
                        return;
                    }

                    value = toValue;  // let setter to handle the helpBox
                    return;
                }
            }

            // not found
            // Debug.Log($"OnSceneFieldChanged not found {toValue}");
            _errorEventString = toValue;
            _errorSceneAsset = sceneAsset;
            UpdateHelpBoxError();
            // value = SceneUtils.TrimScenePath(AssetDatabase.GetAssetPath(sceneAsset), _fullPath);
        }
    }

    public class ScenePickerStringField : BaseField<string>
    {
        public readonly ScenePickerStringElement ScenePickerStringElement;

        public ScenePickerStringField(string label, ScenePickerStringElement visualInput) : base(label, visualInput)
        {
            visualInput.DropdownRoot = this;
            ScenePickerStringElement = visualInput;
        }

        public override string value
        {
            get => ScenePickerStringElement.value;
            set => ScenePickerStringElement.value = value;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            ScenePickerStringElement.SetValueWithoutNotify(newValue);
        }
    }
}
