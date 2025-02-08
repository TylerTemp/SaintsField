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

        [InfoBox("Please select the root folder of your I2 Localization plugin.")]
        [Required]
        [AssetFolder("Assets/I2", "Choose your I2 Localization folder")]
        public string i2LocFolder;

        [InfoBox("asmdef file name, must endswith .asmdef")]
        [ValidateInput(nameof(ValidateAsmdefName))]
        public string i2LocAsdmefName = "I2AsmDef.asmdef";

        [ReadOnly, Required("ASDMEF config not found")]
        public string i2LocAsmdefPath;

        [ReadOnly, Required("ASDMEF meta config not found")]
        public string i2LocAsmdefMetaPath;

        private string ValidateAsmdefName(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.EndsWith(".asmdef"))
            {
                return "asmdef file name must end with .asmdef";
            }

            return "";
        }

        [PlayaEnableIf(nameof(i2LocAsmdefPath), nameof(i2LocAsmdefMetaPath))]
        [Button("Copy I2 Asmdef")]
        // ReSharper disable once UnusedMember.Local
        private void CopyI2LocAsmdef()
        {
            File.Copy(i2LocAsmdefPath, $"{i2LocFolder}/{i2LocAsdmefName}", true);
            File.Copy(i2LocAsmdefMetaPath, $"{i2LocFolder}/{i2LocAsdmefName}.meta", true);
            AssetDatabase.Refresh();
            SaintsMenu.AddCompileDefine("SAINTSFIELD_I2_LOC");
            Close();
        }

        public override void OnEditorEnable()
        {
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
