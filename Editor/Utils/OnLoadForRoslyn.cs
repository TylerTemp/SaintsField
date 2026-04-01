using System.IO;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class OnLoadForRoslyn
    {
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            string preParserRelativeFolder = SaintsFieldConfig.PreParserRelativeFolder;
            if (!Directory.Exists(preParserRelativeFolder))
            {
                Debug.Log($"Create folder: {preParserRelativeFolder}");
                Directory.CreateDirectory(preParserRelativeFolder);
            }

            string projectRootPath = Directory.GetCurrentDirectory();
            const string editorResFolder = "Assets/Editor Default Resources";
            if (!Directory.Exists(editorResFolder))
            {
                Debug.Log($"Create folder: {editorResFolder}");
                Directory.CreateDirectory(editorResFolder);
            }
            const string saintsFieldFolder = editorResFolder + "/SaintsField";
            if (!Directory.Exists(saintsFieldFolder))
            {
                Debug.Log($"Create folder: {saintsFieldFolder}");
                Directory.CreateDirectory(saintsFieldFolder);
            }

            const string roslynConfigFile = saintsFieldFolder + "/Config.SaintsFieldSourceParser.additionalfile";
            if (!File.Exists(roslynConfigFile))
            {
                Debug.Log($"Init for Config.SaintsFieldSourceParser.additionalfile");
                File.WriteAllText(roslynConfigFile, "debug = 0\ndisabled = 0\n");
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string configLine in File.ReadAllLines(roslynConfigFile))
            {
                string[] split = configLine.Split('=');
                // ReSharper disable once InvertIf
                if (split.Length == 2 && split[0].Trim() == "disabled" && split[1].Trim() != "0")
                {
                    Debug.LogWarning("Roslyn is disabled for SaintsField. Your field order might not be correct if you have SaintsEditor enabled");
                    return;
                }
            }

            const string roslynTempFileName = "Temp.SaintsFieldSourceParser.additionalfile";
            const string roslynTempFile = saintsFieldFolder + "/" + roslynTempFileName;
            string writeTempContent = $"path = {projectRootPath.Replace("\\", "/")}/{preParserRelativeFolder}\n";
            bool checkIgnore = true;
            if (File.Exists(roslynTempFile))
            {
                checkIgnore = false;

                string oldContent = File.ReadAllText(roslynTempFile);
                if (oldContent == writeTempContent)
                {
                    writeTempContent = null;
                }
                // Debug.Log($"Init for Temp.SaintsFieldSourceParser.temporaryfile");
            }

            if (writeTempContent != null)
            {
                Debug.Log($"Set generate parse path into {roslynTempFile}");
                File.WriteAllText(roslynTempFile, writeTempContent);
            }

            if (checkIgnore)
            {
                const string ignoreFile = saintsFieldFolder + "/.gitignore";
                Debug.Log($"Write ignore file: {ignoreFile}");
                File.WriteAllText(ignoreFile, $"{roslynTempFileName}\n{roslynTempFileName}.meta\n");
            }
        }
    }
}
