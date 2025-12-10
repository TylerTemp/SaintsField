using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if SAINTSFIELD_NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using SaintsField.Editor.Linq;
using SaintsField.Playa;
using UnityEditor;
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

        // ReSharper disable InconsistentNaming
        private struct ScopedRegistries
        {
            public string name;
            public string url;
            public List<string> scopes;
        }

        private class ManifestBase
        {
            public Dictionary<string, string> dependencies;
        }

        private class Manifest: ManifestBase
        {
            public List<ScopedRegistries> scopedRegistries;
        }
        // ReSharper enable InconsistentNaming

        private bool _loadingSaintsEditor;
        private bool _loadingCodeAnalysis;
        private const string ManifestFile = "Packages/manifest.json";


        private const string scopeUrl = "https://package.openupm.com";
        private static readonly ICollection<string> scopeScopes = new List<string>
        {
            "org.nuget.microsoft.codeanalysis.analyzers",
            "org.nuget.microsoft.codeanalysis.common",
            "org.nuget.microsoft.codeanalysis.csharp",
            "org.nuget.system.buffers",
            "org.nuget.system.collections.immutable",
            "org.nuget.system.memory",
            "org.nuget.system.numerics.vectors",
            "org.nuget.system.reflection.metadata",
            "org.nuget.system.runtime.compilerservices.unsafe",
            "org.nuget.system.text.encoding.codepages",
            "org.nuget.system.threading.tasks.extensions",
        };

        public override void OnEditorEnable()
        {
            _loadingSaintsEditor = false;
            _loadingCodeAnalysis = false;
        }

        [Ordered]

        [LayoutStart("Saints Editor", ELayout.TitleBox)]

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
        private void EnableSaintsEditor()
        {
            _loadingSaintsEditor = true;
            SaintsMenu.AddCompileDefine(SaintsMenu.SAINTSFIELD_SAINTS_EDITOR_APPLY);
        }

        [Ordered]
#if !SAINTSFIELD_SAINTS_EDITOR_APPLY
        [PlayaDisableIf(true)]
#endif
        [Button("Disable")]
        private void DisableSaintsEditor()
        {
            _loadingSaintsEditor = true;
            SaintsMenu.RemoveCompileDefine(SaintsMenu.SAINTSFIELD_SAINTS_EDITOR_APPLY);
        }

        [LayoutEnd]

        [Ordered]
        [Separator(10)]
#if !SAINTSFIELD_NEWTONSOFT_JSON
        [LayoutDisableIf(true)]
        [InfoBox("Package com.unity.nuget.newtonsoft-json not installed", EMessageType.Error)]
#endif
        [LayoutStart("Code Analysis", ELayout.TitleBox)]
        [AboveText("<u>Code Analysis</u> allows layout system to function more preciously on field orders", 5, 5)]
        [Separator(5)]
        [InfoBox("Loading, please wait...", show: nameof(_loadingCodeAnalysis))]

        private static bool CodeAnalysisInstalled() => AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName.StartsWith("Microsoft.CodeAnalysis.CSharp,"));

        private string CodeAnalysisIntallInfo() => "<u>Code Analysis</u> is " +
                                                   (CodeAnalysisInstalled()? "<color=green>installed</color>": "<color=brown>not installed</color>")
                                                   + " in this project";

        [Ordered]
        [AboveText("$" + nameof(CodeAnalysisIntallInfo), 5, 5)]
        [LayoutStart("./Code Analysis Install Buttons", ELayout.Horizontal)]
        [PlayaDisableIf(nameof(CodeAnalysisInstalled))]
        [Button("Install")]
        private void InstallCodeAnalysis()
        {
#if SAINTSFIELD_NEWTONSOFT_JSON
            string content = File.ReadAllText(ManifestFile);
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(content);
            bool foundDependencies = false;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (KeyValuePair<string, string> manifestDependency in manifest.dependencies)
            {
                // Debug.Log($"dependencies: {manifestDependency.Key}={manifestDependency.Value}");
                // ReSharper disable once InvertIf
                if (manifestDependency.Key == "org.nuget.microsoft.codeanalysis.csharp")
                {
                    foundDependencies = true;
                    Debug.Log($"Already found dependencies {manifestDependency.Key}");
                }
            }

            if (!foundDependencies)
            {
                manifest.dependencies["org.nuget.microsoft.codeanalysis.csharp"] = "4.14.0";
                Debug.Log($"Add dependencies org.nuget.microsoft.codeanalysis.csharp={manifest.dependencies["org.nuget.microsoft.codeanalysis.csharp"]}");
            }

            const string scopeName = "package.openupm.com";

            if (manifest.scopedRegistries == null)
            {
                manifest = new Manifest
                {
                    dependencies = manifest.dependencies,
                    scopedRegistries = new List<ScopedRegistries>
                    {
                        new ScopedRegistries
                        {
                            name = scopeName,
                            url = scopeUrl,
                            scopes = scopeScopes.ToList(),
                        },
                    },
                };
            }
            else
            {
                int foundScopedRegistriesIndex = -1;
                ScopedRegistries foundScopedRegistriesValue = default;
                foreach ((ScopedRegistries scopedRegistry, int index) in manifest.scopedRegistries.WithIndex())
                {
                    if (scopedRegistry.name == scopeName)
                    {
                        foundScopedRegistriesIndex = index;
                        foundScopedRegistriesValue = scopedRegistry;
                    }
                }

                if (foundScopedRegistriesIndex == -1)
                {
                    ScopedRegistries newCreated = new ScopedRegistries
                    {
                        name = scopeName,
                        url = scopeUrl,
                        scopes = scopeScopes.ToList(),
                    };
                    Debug.Log($"ScopedRegistries not found, add {newCreated.name}, {newCreated.url}, {string.Join(":", newCreated.scopes)}");
                    manifest.scopedRegistries.Add(newCreated);
                }
                else
                {
                    ScopedRegistries oldUpdate = new ScopedRegistries
                    {
                        name = scopeName,
                        url = scopeUrl,
                        scopes = MergeList(foundScopedRegistriesValue.scopes, scopeScopes),
                    };
                    Debug.Log($"ScopedRegistries updated at {foundScopedRegistriesIndex} with result {oldUpdate.name}, {oldUpdate.url}, {string.Join(":", oldUpdate.scopes)}");
                    manifest.scopedRegistries[foundScopedRegistriesIndex] = oldUpdate;
                }
            }

            string jsonResult = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            Debug.Log(jsonResult);
            _loadingCodeAnalysis = true;
            File.WriteAllText(ManifestFile, jsonResult + "\n");
#endif
        }

        private static List<string> MergeList(List<string> scopes, ICollection<string> toAdd)
        {
            List<string> result = new List<string>(scopes);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (string each in toAdd)
            {
                if (!result.Contains(each))
                {
                    Debug.Log($"add {each}");
                    result.Add(each);
                }
            }

            return result;
        }

        private readonly struct ProcessScopedRegisty
        {
            public readonly bool Delete;
            public readonly int Index;
            public readonly ScopedRegistries CleanedScopedRegistries;

            public ProcessScopedRegisty(bool delete, int index, ScopedRegistries cleanedScopedRegistries)
            {
                Delete = delete;
                Index = index;
                CleanedScopedRegistries = cleanedScopedRegistries;
            }
        }

        [Ordered]
        [Button("Uninstall")]
        [PlayaEnableIf(nameof(CodeAnalysisInstalled))]
        private void UninstallCodeAnalysis()
        {
#if SAINTSFIELD_NEWTONSOFT_JSON
            string content = File.ReadAllText(ManifestFile);
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(content);

            manifest.dependencies.Remove("org.nuget.microsoft.codeanalysis.csharp");

            List<ProcessScopedRegisty> results = new List<ProcessScopedRegisty>();

            ManifestBase result = manifest;
            if (manifest.scopedRegistries != null)
            {
                foreach ((ScopedRegistries scopedRegistry, int index) in manifest.scopedRegistries.WithIndex().Reverse())
                {
                    if (scopedRegistry.url == scopeUrl)
                    {
                        var oldScopes = scopedRegistry.scopes;
                        var newScopes = oldScopes.Except(scopeScopes).ToList();
                        if (newScopes.Count == 0)
                        {
                            Debug.Log($"Delete {scopedRegistry.name}({scopedRegistry.url}) at {index}");
                            results.Add(new ProcessScopedRegisty(true, index, default));
                        }
                        else
                        {
                            Debug.Log($"Update {scopedRegistry.name}({scopedRegistry.url}) at {index} with {string.Join(":", newScopes)}");
                            results.Add(new ProcessScopedRegisty(false, index, new ScopedRegistries
                            {
                                name = scopedRegistry.name,
                                url = scopedRegistry.url,
                                scopes = newScopes,
                            }));
                        }
                    }
                }

                foreach (ProcessScopedRegisty processScopedRegisty in results)
                {
                    if (processScopedRegisty.Delete)
                    {
                        manifest.scopedRegistries.RemoveAt(processScopedRegisty.Index);
                    }
                    else
                    {
                        manifest.scopedRegistries[processScopedRegisty.Index] = processScopedRegisty.CleanedScopedRegistries;
                    }
                }

                if (manifest.scopedRegistries.Count == 0)
                {
                    Debug.Log("No scopedRegistries found, delete");
                    result = new ManifestBase
                    {
                        dependencies = manifest.dependencies,
                    };
                }
            }

            string jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented);
            Debug.Log(jsonResult);
            _loadingCodeAnalysis = true;
            File.WriteAllText(ManifestFile, jsonResult + "\n");
            SaintsMenu.RemoveCompileDefine(SaintsMenu.SAINTSFIELD_CODE_ANALYSIS);
#endif
        }

        [LayoutEnd(".")]
        [AboveText(
            "<u>Code Analysis</u> is " +
#if SAINTSFIELD_CODE_ANALYSIS
            "<color=green>enabled</color>"
#else
            "<color=brown>not enabled</color>"
#endif
            + " in this project", 5, 5)]
        [LayoutStart("./Code Analysis Enable Buttons", ELayout.Horizontal)]

        [Ordered]
#if SAINTSFIELD_CODE_ANALYSIS
        [PlayaDisableIf(true)]
#endif
        [Button("Force Enable")]
        private void EnableCodeAnalysis()
        {
            bool codeAnalysisFound = CodeAnalysisInstalled();
            if (codeAnalysisFound)
            {
                SaintsMenu.AddCompileDefine(SaintsMenu.SAINTSFIELD_CODE_ANALYSIS);
                _loadingCodeAnalysis = true;
                return;
            }

            if (EditorUtility.DisplayDialog("Code Analysis Not Found",
                    "Microsoft.CodeAnalysis.CSharp not found in your project. This will break your script compilation if it's not installed at all.\n" +
                    "If you believe this is a detection mistake, you can enable it anyway",
                    "Enable Anyway",
                    "How To Install?"))
            {
                SaintsMenu.AddCompileDefine(SaintsMenu.SAINTSFIELD_CODE_ANALYSIS);
                _loadingCodeAnalysis = true;
            }
            else
            {
                Application.OpenURL("https://github.com/TylerTemp/SaintsField/?tab=readme-ov-file#setup");
            }
        }
    }
}
