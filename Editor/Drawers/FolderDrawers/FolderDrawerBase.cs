using System.IO;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FolderDrawers
{
    public abstract class FolderDrawerBase: SaintsPropertyDrawer
    {
        protected static (string error, string actualFolder) GetAssetsPath(string folderValue)
        {
            string cwd = Directory.GetCurrentDirectory().Replace("\\", "/");
            if (!folderValue.StartsWith(cwd))
            {
                return ($"selected folder ({folderValue}) is not in project directory ({cwd})", "");
            }

            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string subPath = folderValue.Substring(cwd.Length);
            if(subPath.StartsWith("/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                subPath = subPath.Substring(1);
            }

            if (!subPath.ToLower().StartsWith("assets"))
            {
                return ($"failed to extract assets folder from {folderValue} in project directory {cwd}", "");
            }

            return ("", subPath);
        }

        protected (string error, string actualFolder) OnClick(SerializedProperty property, FolderAttribute folderAttribute)
        {
            string oriValue = property.stringValue;
            string passValue = string.IsNullOrEmpty(oriValue) ? folderAttribute.Folder : oriValue;
            string wrapValue = WrapFolderToOpen(passValue);

            string path = EditorUtility.OpenFolderPanel(folderAttribute.Title, wrapValue, "");
            return string.IsNullOrEmpty(path)
                ? ("", "")
                // DirectoryInfo d = new DirectoryInfo(path);
                // Debug.Log(path);
                // Debug.Log(d.FullName);
                // Debug.Log(Path.GetFullPath(path));
                // return ValidateFolder(d.FullName.Replace("\\", "/"));
                : ValidateFolder(path.Replace("\\", "/"));
        }

        protected abstract (string error, string actualFolder) ValidateFolder(string folderValue);

        protected abstract string WrapFolderToOpen(string folder);
    }
}
