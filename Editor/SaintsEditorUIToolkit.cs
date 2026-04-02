#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor
{
    public partial class SaintsEditor
    {
        private void OnHeaderButtonClickUIToolkit()
        {
            _coreEditor.OnHeaderButtonClick();
            // _toolbarSearchField.style.display = _searchableShown ? DisplayStyle.Flex : DisplayStyle.None;
            // if(_searchableShown)
            // {
            //     _toolbarSearchField.Focus();
            // }
        }

        private ToolbarSearchField _toolbarSearchField;

        private SaintsEditorCore _coreEditor;

        public override VisualElement CreateInspectorGUI()
        {
            _saintsEditorIMGUI = false;
            _coreEditor = new SaintsEditorCore(this, EditorShowMonoScript, this);
            VisualElement root = new VisualElement();
            VisualElement content = _coreEditor.CreateInspectorGUI();

            #region SAINTSBUILD
#if SAINTSBUILD
            if (EditorApplication.isPlayingOrWillChangePlaymode && target != null)
            {
                string assetPath = string.Empty;
                switch (target)
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

            root.Add(content);

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            root.RegisterCallback<AttachToPanelEvent>(_ => AddInstance(this));
            root.RegisterCallback<DetachFromPanelEvent>(_ => RemoveInstance(this));
#endif
            return root;
        }

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

        private void OnDestroyUIToolkit()
        {
            foreach (ISaintsRenderer saintsRenderer in _coreEditor?.AllRenderersUIToolkit ?? Array.Empty<ISaintsRenderer>())
            {
                saintsRenderer.OnDestroy();
            }
        }

        private void ResetSearchUIToolkit()
        {
            _coreEditor.ResetSearchUIToolkit();
            // // ReSharper disable once InvertIf
            // if (_toolbarSearchField.parent != null)
            // {
            //     _toolbarSearchField.parent.Focus();
            //     _toolbarSearchField.value = "";
            // }
        }
    }
}
#endif
