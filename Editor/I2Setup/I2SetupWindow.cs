using System.IO;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.I2Setup
{
    public class I2SetupWindow: SaintsEditorWindow
    {
        public static void OpenWindow()
        {
            SaintsEditorWindow window = GetWindow<I2SetupWindow>(false, "SaintsField I2 Setup");
            window.Show();
        }

        private const string DefaultI2InstallFolder = "Assets/I2";

        [FieldInfoBox("Please select the root folder of your I2 Localization plugin.")]
        // [ValidateInput(nameof(FolderExists))]
        [Required]
        [AssetFolder(DefaultI2InstallFolder, "Choose your I2 Localization folder")]
        public string i2LocFolder;

        [FieldInfoBox("asmdef file name, must endswith .asmdef")]
        [ValidateInput(nameof(ValidateAsmdefName))]
        public string i2LocAsdmefName = "I2AsmDef.asmdef";

        [FieldReadOnly, Required("ASDMEF config not found")]
        public string i2LocAsmdefPath;

        [FieldReadOnly, Required("ASDMEF meta config not found")]
        public string i2LocAsmdefMetaPath;

        private string ValidateAsmdefName(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.EndsWith(".asmdef"))
            {
                return "asmdef file name must end with .asmdef";
            }

            return "";
        }

        [EnableIf(nameof(i2LocAsmdefPath), nameof(i2LocAsmdefMetaPath))]
        [Button("Copy I2 Asmdef")]
        // ReSharper disable once UnusedMember.Local
        private void CopyI2LocAsmdef()
        {
            File.Copy(i2LocAsmdefPath, $"{i2LocFolder}/{i2LocAsdmefName}", true);
            File.Copy(i2LocAsmdefMetaPath, $"{i2LocFolder}/{i2LocAsdmefName}.meta", true);
            AssetDatabase.Refresh();
            SaintsMenu.AddCompileDefine(SaintsMenu.EnableI2LocalizationSupportMarco);
            Close();
        }

        // private bool FolderExists(string folder) => Directory.Exists(folder);

        public override void OnEditorEnable()
        {
            if (Directory.Exists(DefaultI2InstallFolder))
            {
                i2LocFolder = DefaultI2InstallFolder;
            }

            i2LocAsmdefPath = i2LocAsmdefMetaPath = "";
            foreach (string folder in Util.ResourceSearchFolder)
            {
                string asmdefPath = $"{folder}/I2/I2AsmDef.asmdef.txt";
                // Debug.LogError(asmdefPath);
                string asmdefMetaPath = $"{folder}/I2/I2AsmDef.asmdef.meta.txt";

                if (File.Exists(asmdefPath))
                {
                    i2LocAsmdefPath = asmdefPath;
                }
                if (File.Exists(asmdefMetaPath))
                {
                    i2LocAsmdefMetaPath = asmdefMetaPath;
                }
            }
        }
    }
}
