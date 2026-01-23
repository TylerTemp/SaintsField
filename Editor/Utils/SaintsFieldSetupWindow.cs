using System;
using System.Collections;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class SaintsFieldSetupWindow: SaintsEditorWindow
    {
#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/SaintsField Setup")]
#endif
        public static void Open()
        {
            GetWindow<SaintsFieldSetupWindow>("SaintsField Setup").Show();
        }

        [InitializeOnLoadMethod]
        private static void WatchConfigLoad()
        {

            if (SaintsFieldConfigUtil.Config != null)
            {
                CheckConfigOpen(SaintsFieldConfigUtil.Config);
            }
            else
            {
                SaintsFieldConfigUtil.OnConfigLoaded.AddListener(CheckConfigOpen);
            }
        }

        // Note: if user installed the package with unity closed, then open the UnityEditor
        // the window will not pop up no matter what
        // This is acceptable, because without this static, the window will pop up many times with blank window
        // as the UnityEditor processing
        private static bool _pop;

        private static void CheckConfigOpen(SaintsFieldConfig config)
        {
            if (_pop)
            {
                return;
            }

            _pop = true;
            if (!config.GetSetupWindowPopOnce())
            {
                Debug.Log($"setupWindowPopOnce false, pop setup window for SaintsField");
                EditorApplication.delayCall += () =>
                {
                    GetWindow<SaintsFieldSetupWindow>("SaintsField Setup").Show();
                };
            }
        }

        // ReSharper disable InconsistentNaming
        // private struct ScopedRegistries
        // {
        //     public string name;
        //     public string url;
        //     public List<string> scopes;
        // }
        //
        // private class ManifestBase
        // {
        //     public Dictionary<string, string> dependencies;
        // }

        // private class Manifest: ManifestBase
        // {
        //     public List<ScopedRegistries> scopedRegistries;
        // }
        // ReSharper enable InconsistentNaming

#pragma warning disable CS0414 // Type or member is only assigned
        private bool _loadingSaintsEditor;
        private bool _loadingCodeAnalysis;
#pragma warning restore CS0414 // Type or member is only assigned
        // private bool _loadingUnitySerialization;
        // private const string ManifestFile = "Packages/manifest.json";
        //
        //
        // private const string scopeUrl = "https://package.openupm.com";
        // private static readonly ICollection<string> scopeScopes = new List<string>
        // {
        //     "org.nuget.microsoft.codeanalysis.analyzers",
        //     "org.nuget.microsoft.codeanalysis.common",
        //     "org.nuget.microsoft.codeanalysis.csharp",
        //     "org.nuget.system.buffers",
        //     "org.nuget.system.collections.immutable",
        //     "org.nuget.system.memory",
        //     "org.nuget.system.numerics.vectors",
        //     "org.nuget.system.reflection.metadata",
        //     "org.nuget.system.runtime.compilerservices.unsafe",
        //     "org.nuget.system.text.encoding.codepages",
        //     "org.nuget.system.threading.tasks.extensions",
        // };

        public override void OnEditorEnable()
        {
            _loadingSaintsEditor = false;
            _loadingCodeAnalysis = false;
            // _loadingUnitySerialization = false;
        }

        public override void OnEditorDestroy()
        {
            SaintsFieldConfig config = SaintsFieldConfigUtil.Config;
            if(config != null && !config.setupWindowPopOnce)
            {
                EditorUtility.SetDirty(config);
                config.setupWindowPopOnce = true;
            }
        }

        #region Saints Editor

        [LayoutStart("SaintsEditor", ELayout.TitleBox)]

        [AboveText(
            "<u>SaintsEditor</u> enables many functions for this plugin. <color=yellow>Note</color>: if you have other inspector like OdinInspector, Tri-Inspector, EditorAttributes enabled, only one will actually work",
            5, 5)]
        [Separator(5)]
        [AboveText(
            "<u>SaintsEditor</u> is " +
#if SAINTSFIELD_SAINTS_EDITOR_APPLY
            "<color=green>enabled</color>"
#else
            "<color=brown>not enabled</color>"
#endif
        + " in this project", 5, 5)]

        [InfoBox("Loading, please wait...", show: nameof(_loadingSaintsEditor))]

        [LayoutStart("./SaintsEditor Buttons", ELayout.Horizontal)]
#if SAINTSFIELD_SAINTS_EDITOR_APPLY
        [PlayaDisableIf(true)]
#endif
        [Button("Enable")]
        // ReSharper disable once UnusedMember.Local
        private void EnableSaintsEditor()
        {
            _loadingSaintsEditor = true;
            SaintsMenu.AddCompileDefine(SaintsMenu.SAINTSFIELD_SAINTS_EDITOR_APPLY);
        }


#if !SAINTSFIELD_SAINTS_EDITOR_APPLY
        [DisableIf(true)]
#endif
        [Button("Disable")]
        // ReSharper disable once UnusedMember.Local
        private void DisableSaintsEditor()
        {
            _loadingSaintsEditor = true;
            SaintsMenu.RemoveCompileDefine(SaintsMenu.SAINTSFIELD_SAINTS_EDITOR_APPLY);
        }

        [LayoutEnd]

        #endregion

        #region Unity Serialization


        private (EMessageType, string) InstallUnitySerializationStatus = (EMessageType.None, "");


        [Separator(10)]
        [LayoutStart("SaintsEvent", ELayout.TitleBox)]
        [AboveText("SaintsEvent requires <u>Unity Serialization</u> to be installed in this project.", 5, 5)]
        [Separator(5)]
        [AboveText(
            "<u>SaintsEvent</u> is " +
#if SAINTSFIELD_SERIALIZATION
            "<color=green>enabled</color>"
#else
            "<color=brown>not enabled</color>"
#endif
            + " in this project", 5, 5)]
        [Separator(5)]

        [InfoBox("$" + nameof(InstallUnitySerializationStatus))]

        // [InfoBox("Loading, please wait...", show: nameof(_loadingUnitySerialization))]

        [LayoutStart("./SaintsEvent Install Buttons", ELayout.Horizontal)]
#if SAINTSFIELD_SERIALIZATION
        [DisableIf(true)]
#endif
        [Button("Install")]
        // ReSharper disable once UnusedMember.Local
        private IEnumerator InstallUnitySerialization()
        {
            const string packageName = "com.unity.serialization";
            AddRequest _addRequest = Client.Add(packageName);
            int counter = 0;
            bool wait = true;
            while (wait)
            {
                counter = (counter + 1) % 4;
                switch (_addRequest.Status)
                {
                    case StatusCode.InProgress:
                        InstallUnitySerializationStatus = (EMessageType.Warning, $"Installing {packageName}, please wait{new string('.', counter)}");
                        break;
                    case StatusCode.Success:
                        InstallUnitySerializationStatus = (EMessageType.Info, $"{packageName} Installed");
                        wait = false;
                        break;
                    case StatusCode.Failure:
                        InstallUnitySerializationStatus = (EMessageType.Error, $"{packageName} install failed: {_addRequest.Error.message}");
                        wait = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                yield return null;
            }
        }


        [Button("Uninstall")]
#if !SAINTSFIELD_SERIALIZATION
        [PlayaDisableIf(true)]
#endif
        // ReSharper disable once UnusedMember.Local
        private IEnumerator UninstallUnitySerialization()
        {
            const string packageName = "com.unity.serialization";
            RemoveRequest removeRequest = Client.Remove(packageName);
            int counter = 0;
            bool wait = true;
            while (wait)
            {
                counter = (counter + 1) % 4;
                switch (removeRequest.Status)
                {
                    case StatusCode.InProgress:
                        InstallUnitySerializationStatus = (EMessageType.Warning, $"Uninstalling {packageName}, please wait{new string('.', counter)}");
                        break;
                    case StatusCode.Success:
                        InstallUnitySerializationStatus = (EMessageType.Info, $"{packageName} Uninstalled");
                        wait = false;
                        break;
                    case StatusCode.Failure:
                        InstallUnitySerializationStatus = (EMessageType.Error, $"{packageName} uninstall failed: {removeRequest.Error.message}");
                        wait = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                yield return null;
            }
        }

        #endregion
    }
}
