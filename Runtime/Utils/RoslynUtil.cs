#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEngine;

namespace SaintsField.Utils
{
    public static class RoslynUtil
    {
        public static (string error, string result) CheckChange(SaintsFieldConfig instance)
        {
            if (!File.Exists(SaintsFieldConfigUtil.AssetPath))
            {
                return ("Config not created", string.Empty);
            }

            string saveToPath = instance.GetParserSavePath();
            // string preParserRelativeFolder = SaintsFieldConfig.instance.GetParserSavePath();
            if (!Directory.Exists(saveToPath))
            {
                Debug.Log($"#Roslyn# Create folder: {saveToPath}");
                Directory.CreateDirectory(saveToPath);
            }

            // string projectRootPath = Directory.GetCurrentDirectory();
            const string editorResFolder = "Assets/Editor Default Resources";
            if (!Directory.Exists(editorResFolder))
            {
                Debug.Log($"#Roslyn# create folder: {editorResFolder}");
                Directory.CreateDirectory(editorResFolder);
            }
            const string saintsFieldFolder = editorResFolder + "/SaintsField";
            if (!Directory.Exists(saintsFieldFolder))
            {
                Debug.Log($"#Roslyn# create folder: {saintsFieldFolder}");
                Directory.CreateDirectory(saintsFieldFolder);
            }

            const string roslynConfigFile = saintsFieldFolder + "/Config.SaintsFieldSourceParser.additionalfile";
            if (!File.Exists(roslynConfigFile))
            {
                Debug.Log($"#Roslyn# init for Config.SaintsFieldSourceParser.additionalfile");
                File.WriteAllText(roslynConfigFile, "debug = 0\ndisabled = 0\n");
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string configLine in File.ReadAllLines(roslynConfigFile))
            {
                string[] split = configLine.Split('=');
                // ReSharper disable once InvertIf
                if (split.Length == 2 && split[0].Trim() == "disabled" && split[1].Trim() != "0")
                {
                    // Debug.LogWarning("#Roslyn# is disabled for SaintsField. Your field order might not be correct if you have SaintsEditor enabled");
                    return ($"SaintsField Roslyn disabled in {roslynConfigFile}",  string.Empty);
                }
            }

            const string roslynTempFileName = "Temp.SaintsFieldSourceParser.additionalfile";
            const string roslynTempFile = saintsFieldFolder + "/" + roslynTempFileName;

            // Debug.Log(SaintsFieldConfig.instance.overrideCodeParserFolder);
            // Debug.Log(SaintsFieldConfig.instance.codeParserFolder);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            string writeTempContent = $"path = {saveToPath}\n";
            bool checkIgnore = true;
            if (File.Exists(roslynTempFile))
            {
                checkIgnore = false;

                string oldContent = File.ReadAllText(roslynTempFile);
                if (oldContent == writeTempContent)
                {
                    // Debug.Log($"#Roslyn# no update for {Repr(oldContent)} on path {roslynTempFile}");
                    writeTempContent = null;
                }
                else
                {
                    Debug.Log($"#Roslyn# generate config to {Repr(writeTempContent)} from {Repr(oldContent)} on path {roslynTempFile}");
                }
                // Debug.Log($"Init for Temp.SaintsFieldSourceParser.temporaryfile");
            }

            if (writeTempContent != null)
            {
                Debug.Log($"#Roslyn# set generate parse path into {roslynTempFile} with {Repr(writeTempContent)}");
                File.WriteAllText(roslynTempFile, writeTempContent);
            }

            if (checkIgnore)
            {
                const string ignoreFile = saintsFieldFolder + "/.gitignore";
                Debug.Log($"#Roslyn# write ignore file: {ignoreFile}");
                File.WriteAllText(ignoreFile, $"{roslynTempFileName}\n{roslynTempFileName}.meta\n");
            }

            return (string.Empty, saveToPath);
        }



        private static string Repr(string s)
        {
            if (s == null) return "None";  // Python style

            StringBuilder sb = new StringBuilder();
            sb.Append('"');

            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (char.IsControl(ch) || ch < 0x20)
                        {
                            sb.Append("\\u");
                            sb.Append(((int)ch).ToString("x4"));
                        }
                        else if (ch > 0x7E) // non-ASCII
                        {
                            sb.Append("\\u");
                            sb.Append(((int)ch).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            sb.Append('"');
            return sb.ToString();
        }
    }
}
#endif
