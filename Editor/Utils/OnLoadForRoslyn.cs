using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (!File.Exists(SaintsFieldConfigUtil.AssetPath))
            {
                return;
            }

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
            string saveToPath;
            if (SaintsFieldConfig.instance.overrideCodeParserFolder)
            {
                string tempPath = Path.GetTempPath().Replace("\\", "/").TrimEnd('/');
                saveToPath = StringFormatByName(SaintsFieldConfig.instance.codeParserFolder,
                    new Dictionary<string, string>
                    {
                        { "TEMP", tempPath},
                        { "PROJECT", projectRootPath },
                    }).Replace("\\", "/").Trim();
            }
            else
            {
                saveToPath = SaintsFieldConfig.CodeParserDefaultFolder;
            }
            string writeTempContent = $"path = {saveToPath}\n";
            bool checkIgnore = true;
            if (File.Exists(roslynTempFile))
            {
                checkIgnore = false;

                string oldContent = File.ReadAllText(roslynTempFile);
                if (oldContent == writeTempContent)
                {
                    writeTempContent = null;
                }
                else
                {
                    Debug.Log($"Generate config to {writeTempContent} from {oldContent}");
                }
                // Debug.Log($"Init for Temp.SaintsFieldSourceParser.temporaryfile");
            }

            if (writeTempContent != null)
            {
                Debug.Log($"Set generate parse path into {roslynTempFile} with {writeTempContent}");
                // TODO: undo this!
                // File.WriteAllText(roslynTempFile, writeTempContent);
            }

            if (checkIgnore)
            {
                const string ignoreFile = saintsFieldFolder + "/.gitignore";
                Debug.Log($"Write ignore file: {ignoreFile}");
                File.WriteAllText(ignoreFile, $"{roslynTempFileName}\n{roslynTempFileName}.meta\n");
            }
        }

        private static string StringFormatByName(string template, Dictionary<string, string> values)
        {
            // Dictionary<string, string> values = new Dictionary<string, string> { { "Name", "Alice" }, { "Age", "30" } };
            // string template = "Hello {Name}, you are {Age}.";

            return values.Aggregate(template, (current, value) =>
                current.Replace("{" + value.Key + "}", value.Value));
        }
    }
}
