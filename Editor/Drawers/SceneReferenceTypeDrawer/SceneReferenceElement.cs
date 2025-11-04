using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SceneReferenceTypeDrawer
{
    public class SceneReferenceElement: ScenePickerBaseElement<string>
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

                    string newGuid = AssetDatabase.AssetPathToGUID(path);
                    if (value == newGuid)
                    {
                        SetValueWithoutNotify(newGuid);
                    }
                    else
                    {
                        value = newGuid;
                    }
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

                string newGuid = AssetDatabase.AssetPathToGUID(newPath);
                // Debug.Log($"get guid {newGuid} from {newPath}");
                if (CachedValue == newGuid)
                {
                    SetValueWithoutNotify(newGuid);
                }
                else
                {
                    value = newGuid;
                }
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
                _sceneHelpBox.text = $"{_errorEventString} not in build list";
                _sceneHelpBox.AddButton.style.display = DisplayStyle.Flex;

                _sceneHelpBox.style.display = DisplayStyle.Flex;
                _sceneHelpBox.EnableButton.style.display = DisplayStyle.None;
                return;
            }

            _sceneHelpBox.style.display = DisplayStyle.None;
        }

        private void SetHelpBoxErrorText(string text)
        {
            if (_sceneHelpBox == null)
            {
                return;
            }

            _sceneHelpBox.text = text;
            _sceneHelpBox.style.display = DisplayStyle.Flex;
            _sceneHelpBox.EnableButton.style.display = DisplayStyle.None;
            _sceneHelpBox.AddButton.style.display = DisplayStyle.None;
        }

        private readonly struct SceneReferencePayload: IScenePickerPayload
        {
            public string Name { get; }

            public readonly string Guid;
            public readonly int Index;

            public bool IsSceneAsset(SceneAsset sceneAsset)
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sceneAsset, out string guid, out long _))
                {
                    return false;
                }

                return guid == Guid;
            }

            public SceneReferencePayload(string guid, string name, int index)
            {
                Name = name;
                Guid = guid;
                Index = index;
            }
        }

        private string _errorEventString = "";
        private EditorBuildSettingsScene _errorEditorScene;
        private SceneAsset _errorSceneAsset;

        protected override bool AllowEmpty() => false;

        protected override IScenePickerPayload MakeEmpty()
        {
            throw new NotSupportedException("Should not go here");
        }

        protected override bool CurrentIsPayload(IScenePickerPayload payload)
        {
            return ((SceneReferencePayload)payload).Guid == value;
        }

        protected override void PostProcessDropdownList(AdvancedDropdownList<IScenePickerPayload> dropdown)
        {
            dropdown.AddSeparator();
            dropdown.Add("Edit Scenes In Build...", new SceneReferencePayload("", "", -1), false, "d_editicon.sml");
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                SetHelpBoxErrorText("Guid is empty");
                return;
            }

            // Debug.Log($"SetValueWithoutNotify {value} -> {newValue}");
            if (!GUID.TryParse(newValue, out GUID guidResult))
            {
                // Debug.Log($"SetValueWithoutNotify failed to parse {newValue}");
                SetHelpBoxErrorText($"Invalid guid {newValue}");
                return;
            }

#if UNITY_6000_2_OR_NEWER
            SceneAsset asset = AssetDatabase.LoadAssetByGUID<SceneAsset>(guidResult);
#else
            SceneAsset asset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guidResult));
#endif

            if (asset == null)
            {
                SetHelpBoxErrorText($"Guid {guidResult} does not exists or is not SceneAsset");
                return;
            }

            if (_sceneField.value != asset)
            {
                _sceneField.SetValueWithoutNotify(asset);
            }

            string scenePath = AssetDatabase.GetAssetPath(asset);
            string toValue = RuntimeUtil.TrimScenePath(scenePath, true);
            foreach (EditorBuildSettingsScene inBuild in EditorBuildSettings.scenes)
            {
                if (inBuild.path == scenePath)
                {
                    if (!inBuild.enabled)
                    {
                        _errorEventString = toValue;
                        _errorEditorScene = inBuild;
                        _errorSceneAsset = null;
                        UpdateHelpBoxError();
                        return;
                    }

                    HasCachedValue = true;
                    CachedValue = newValue;
                    _sceneField.SetValueWithoutNotify(asset);
                    _errorEventString = "";
                    _errorSceneAsset = null;
                    _errorEditorScene = null;
                    UpdateHelpBoxError();
                    return;
                }
            }

            _errorEventString = toValue;
            _errorSceneAsset = asset;
            _errorEditorScene = null;
            UpdateHelpBoxError();
        }

        protected override IReadOnlyList<IScenePickerPayload> GetScenePickerPayloads()
        {
            List<IScenePickerPayload> result = new List<IScenePickerPayload>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach ((EditorBuildSettingsScene editorScene, int index) in EditorBuildSettings.scenes.Where(each => each.enabled).WithIndex())
            {
                result.Add(new SceneReferencePayload(
                    AssetDatabase.GUIDFromAssetPath(editorScene.path).ToString(),
                    RuntimeUtil.TrimScenePath(editorScene.path, true),
                    index));
            }

            return result;
        }

        protected override void SetSelectedPayload(IScenePickerPayload scenePickerPayload)
        {
            SceneReferencePayload payload = (SceneReferencePayload)scenePickerPayload;
            if (payload.Index == -1)
            {
                SceneUtils.OpenBuildSettings();
            }
            else
            {
                // Debug.Log($"Set guid to {payload.Guid}");
                value = payload.Guid;
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
            string toValue = RuntimeUtil.TrimScenePath(scenePath, true);

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
        }
    }

    public class SceneReferenceField : BaseField<string>
    {
        public readonly SceneReferenceElement SceneReferenceElement;
        public SceneReferenceField(string label, SceneReferenceElement visualInput) : base(label, visualInput)
        {
            SceneReferenceElement = visualInput;
            visualInput.DropdownRoot = this;
        }

        public override string value
        {
            get => SceneReferenceElement.value;
            set => SceneReferenceElement.value = value;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            SceneReferenceElement.SetValueWithoutNotify(newValue);
        }
    }
}
