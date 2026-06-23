using System;
using System.Collections.Generic;
using SaintsField.Editor.HeaderGUI;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor
{
    public partial class SaintsEditorCore
    {
        private UnityEngine.Object Target => _editor.target;

        private ToolbarSearchField _toolbarSearchField;
        private IReadOnlyList<ISaintsRenderer> _hasElementRenderersUIToolkit = Array.Empty<ISaintsRenderer>();
        public IReadOnlyList<ISaintsRenderer> AllRenderersUIToolkit { get; private set; } = Array.Empty<ISaintsRenderer>();
#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateInspectorGUI()
        {
            if (!Target)
            {
                return new HelpBox("The target object is null. Check for missing scripts.", HelpBoxMessageType.Error);
            }

            VisualElement root = new VisualElement();

            Type objectType = Target.GetType();
            IPlayaClassAttribute[] playaClassAttributes = ReflectCache.GetCustomAttributes<IPlayaClassAttribute>(objectType);

            // foreach (ISaintsRenderer saintsRenderer in GetClassStructRenderer(objectType, playaClassAttributes, serializedObject, targets))
            // {
            //     VisualElement ve = saintsRenderer.CreateVisualElement();
            //     if(ve != null)
            //     {
            //         root.Add(ve);
            //     }
            // }

            MonoScript monoScript = SaintsEditor.GetMonoScript(Target);
            if(monoScript && _editorShowMonoScript)
            {
                ObjectField objectField = new ObjectField("Script")
                {
                    bindingPath = "m_Script",
                    value = monoScript,
                    allowSceneObjects = false,
                    objectType = typeof(MonoScript),
                };
                objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                objectField.Bind(SerializedObject);
                objectField.SetEnabled(false);
                objectField.AddManipulator(new ContextualMenuManipulator(evt =>
                        evt.menu.AppendAction("Edit Script", _ => AssetDatabase.OpenAsset(monoScript))
                    ));

                root.Add(objectField);
            }

            SearchableAttribute searchableAttribute = null;
            if (SaintsFieldConfigUtil.GetMonoBehaviorSearchable())
            {
                searchableAttribute = new SearchableAttribute();
            }
            else
            {
                foreach (IPlayaClassAttribute playaClassAttribute in playaClassAttributes)
                {
                    if (playaClassAttribute is SearchableAttribute sa)
                    {
                        searchableAttribute = sa;
                        break;
                    }
                }
            }

            if (searchableAttribute != null && _editor is ISearchable iSearchable)
            {
                _toolbarSearchField = new ToolbarSearchField
                {
                    style =
                    {
                        // flexGrow = 1,
                        display = DisplayStyle.None,
                        // width = Length.Percent(100),
                        width = StyleKeyword.None,
                    },
#if UNITY_6000_0_OR_NEWER
                    placeholderText = "Search Field Name",
#endif
                };
                root.Add(_toolbarSearchField);

                _toolbarSearchField.RegisterValueChangedCallback(evt =>
                {
                    string searchRaw = evt.newValue;
                    string searchUse = searchRaw.Contains(' ') ? searchRaw : ObjectNames.NicifyVariableName(searchRaw);
                    OnSearchUIToolkit(searchUse);
                });
                DrawHeaderGUI.SaintsEditorEnqueueSearchable(iSearchable);
            }

            VisualElement content = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            root.Add(content);

            // Debug.Log($"ser={serializedObject.targetObject}, target={target}");

            AllRenderersUIToolkit = SaintsEditor.Setup(Array.Empty<string>(), SerializedObject, GetMakeRender(), Targets);

            // Debug.Log($"renderers.Count={renderers.Count}");
            List<ISaintsRenderer> usedRenderers = new List<ISaintsRenderer>();
            foreach (ISaintsRenderer saintsRenderer in AllRenderersUIToolkit)
            {
                // Debug.Log($"renderer={saintsRenderer}");
                VisualElement ve = saintsRenderer.CreateVisualElement(root);
                if(ve != null)
                {
                    usedRenderers.Add(saintsRenderer);
                    content.Add(ve);
                }
            }

            _hasElementRenderersUIToolkit = usedRenderers;

            // root.Add(CreateVisualElement(renderers));

            root.schedule.Execute(DrawHeaderGUI.HelperUpdate).Every(1);

            // ReSharper disable once InvertIf
            if (_toolbarSearchField != null)
            {
                root.focusable = true;
                root.RegisterCallback<KeyUpEvent>(evt =>
                {
                    if(evt.keyCode == KeyCode.F && evt.actionKey)
                    {
                        OnHeaderButtonClick();
                    }
                }, TrickleDown.TrickleDown);
            }

            #region SAINTSBUILD
#if SAINTSBUILD
            if (EditorApplication.isPlayingOrWillChangePlaymode && Target != null)
            {
                string assetPath = string.Empty;
                switch (Target)
                {
                    case Component component:
                    {
                        GameObject go = component.gameObject;
                        assetPath = GetPrefabAssetPathIfAsset(go);
                    }
                        break;
                    case GameObject go:
                    {
                        assetPath = GetPrefabAssetPathIfAsset(go);
                    }
                        break;
                    case ScriptableObject so:
                    {
                        assetPath = AssetDatabase.GetAssetPath(so);
                    }
                        break;
                }

                if (!string.IsNullOrEmpty(assetPath))
                {
                    SaintsBuild.Editor.Utils.AssetPostprocessorWatcherList saintsBuildList = SaintsBuild.Editor.Utils.AssetPostprocessorWatcherList.instance;
                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    foreach (SaintsBuild.Editor.Utils.BackupInfo backupInfo in saintsBuildList.backupInfos)
                    {
                        // ReSharper disable once InvertIf
                        if (backupInfo.assetPath == assetPath)
                        {
                            content.SetEnabled(false);
                            VisualElement disabler = new VisualElement();
                            HelpBox helpBox =
                                new HelpBox(
                                    "<size=+1>This asset is modified by SaintsBuild, any change will be restored once you exit the play mode</size>",
                                    HelpBoxMessageType.Warning)
                                {
                                    style =
                                    {
                                        marginBottom = 0,
                                        borderBottomWidth = 0,
                                        borderBottomLeftRadius = 0,
                                        borderBottomRightRadius = 0,
                                    },
                                };
                            disabler.Add(helpBox);
                            Button button = new Button
                            {
                                text = "Do Not Restore This Asset",
                                style =
                                {
                                    marginTop = 0,
                                    borderTopWidth = 0,
                                    borderTopLeftRadius = 0,
                                    borderTopRightRadius = 0,
                                    borderLeftColor = new Color(0.1019608f, 0.1019608f, 0.1019608f),
                                    borderBottomColor = new Color(0.1019608f, 0.1019608f, 0.1019608f),
                                    borderRightColor = new Color(0.1019608f, 0.1019608f, 0.1019608f),
                                    minHeight = 25,
                                },
                            };
                            button.clicked += () =>
                            {
                                bool found = false;
                                for (int index = 0; index < saintsBuildList.backupInfos.Count; index++)
                                {
                                    if (saintsBuildList.backupInfos[index].assetPath == assetPath)
                                    {
                                        saintsBuildList.backupInfos.RemoveAt(index);
                                        disabler.RemoveFromHierarchy();
                                        found = true;
                                        break;
                                    }
                                }

                                content.SetEnabled(true);
                                if (!found)
                                {
                                    helpBox.text = "Asset not found in SaintsBuild list";
                                    button.RemoveFromHierarchy();
                                }
                            };
                            disabler.Add(button);
                            root.Add(disabler);
                        }
                    }

                }
            }
#endif
            #endregion

            return root;
        }
#endif

        private static string GetPrefabAssetPathIfAsset(GameObject go)
        {

            // --- CASE 1: Prefab Mode (opened prefab) ---
            PrefabStage stage = PrefabStageUtility.GetPrefabStage(go);
            if (stage != null)
            {
                return stage.assetPath;  // works for root & nested
            }

            // --- CASE 2: Prefab instance in scene ---
            PrefabInstanceStatus instanceStatus = PrefabUtility.GetPrefabInstanceStatus(go);
            if (instanceStatus != PrefabInstanceStatus.NotAPrefab)
            {
                return string.Empty; // prefab instance => empty
            }

            // --- CASE 3: Pure scene object ---
            PrefabAssetType assetType = PrefabUtility.GetPrefabAssetType(go);
            if (assetType == PrefabAssetType.NotAPrefab)
            {
                return string.Empty;
            }

            // --- CASE 4: Prefab asset (including nested prefab asset objects) ---
            // For nested prefab assets, Unity resolves the outermost prefab root here:
            GameObject root = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
            if (!root)
            {
                root = go;
            }

            return AssetDatabase.GetAssetPath(root);
        }


        private void OnSearchUIToolkit(string search)
        {
            foreach (ISaintsRenderer saintsRenderer in _hasElementRenderersUIToolkit)
            {
                saintsRenderer.OnSearchField(search);
            }
        }

        private bool _searchableShown;

        public void OnHeaderButtonClick()
        {
            _searchableShown = !_searchableShown;
            OnHeaderButtonClickUIToolkit();

            if (!_searchableShown)
            {
                ResetSearchUIToolkit();
            }
        }

        private void OnHeaderButtonClickUIToolkit()
        {
            _toolbarSearchField.style.display = _searchableShown ? DisplayStyle.Flex : DisplayStyle.None;
            if(_searchableShown)
            {
                _toolbarSearchField.Focus();
            }
        }

        public void ResetSearchUIToolkit()
        {
            // ReSharper disable once InvertIf
            if (_toolbarSearchField.parent != null)
            {
                _toolbarSearchField.parent.Focus();
                _toolbarSearchField.value = "";
            }
        }
    }
}
