using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FolderDrawers.ResourcesFolderDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ResourceFolderAttribute), true)]
    public partial class ResourceFolderAttributeDrawer: FolderDrawerBase, IAutoRunnerFixDrawer
    {
        protected override (string error, string actualFolder) ValidateFullFolder(string folderValue)
        {
            (string error, string assetsFolder) = GetAssetsPath(folderValue);
            if (error != "")
            {
                return (error, assetsFolder);
            }

            return ValidateAssetFolder(assetsFolder);
        }

        private static (string error, string actualFolder) ValidateAssetFolder(string assetsFolder)
        {
            List<string> resourcePaths = new List<string>();
            bool found = false;
            foreach (string part in assetsFolder.Split('/'))
            {
                if (part == "")
                {
                    continue;
                }
                if(part.Equals("Resources", StringComparison.OrdinalIgnoreCase))
                {
                    resourcePaths.Clear();
                    found = true;
                    continue;
                }

                resourcePaths.Add(part);
            }

            return found
                ? ("", string.Join("/", resourcePaths))
                : ($"Resources folder not found in {assetsFolder}", "");
        }

        protected override string WrapFolderToOpen(string folder)
        {
            string projectPath = Directory.GetCurrentDirectory();
            string assetsPath = Path.Combine(projectPath, "Assets");
            string resultFolder;
            if (string.IsNullOrEmpty(folder))
            {
                resultFolder = FindFirstResourcesFolder(assetsPath);
            }
            else
            {
                resultFolder = FindFirstMatchedResourceFolder(assetsPath, folder) ?? FindFirstResourcesFolder(assetsPath);
            }

            return resultFolder is null ? "Assets" : ProcessResultFolder(projectPath, resultFolder);
        }

        private static string ProcessResultFolder(string root, string find)
        {
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string r = find.Substring(root.Length).Replace("\\", "/");
            if (r.StartsWith("/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                r = r.Substring(1);
            }

            return r;
        }

        private static string FindFirstResourcesFolder(string rootPath)
        {
            foreach (string directory in Directory.GetDirectories(rootPath))
            {
                if (Path.GetFileName(directory).Equals("Resources", StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
                string found = FindFirstResourcesFolder(directory);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static string FindFirstMatchedResourceFolder(string rootPath, string folder)
        {
            foreach (string directory in Directory.GetDirectories(rootPath))
            {
                if (Path.GetFileName(directory).Equals(folder, StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
                string found = FindFirstMatchedResourceFolder(directory, folder);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static IEnumerable<string> GetAllResourcesFolders()
        {
            return AssetDatabase.FindAssets("t:folder")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("/Resources"));
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new AutoRunnerFixerResult
                {
                    Error = $"{property.propertyPath}({property.propertyType}) is not string type",
                    ExecError = "",
                    CanFix = false,
                    Callback = null,
                };
            }
            string err = CheckFolder(property.stringValue);
            return new AutoRunnerFixerResult
            {
                Error = err,
                ExecError = "",
                CanFix = false,
                Callback = null,
            };
        }

        private static string CheckFolder(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            List<string> foundFolders = new List<string>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string resourcesFolder in GetAllResourcesFolders())
            {
                string folderPath = $"{resourcesFolder}/{value}";
                // ReSharper disable once InvertIf
                if (Directory.Exists(folderPath))
                {
                    return string.Empty;
                }
                foundFolders.Add($" - {resourcesFolder}");
            }
            foundFolders.Sort();

            return $"{value} not in any resources folder:\n{string.Join("\n", foundFolders)}";
        }
    }
}
