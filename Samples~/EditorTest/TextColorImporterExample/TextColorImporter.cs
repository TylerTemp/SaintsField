// Assets/Editor/TextColorImporter.cs

using System.IO;
using SaintsField.Playa;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SaintsField.Samples.EditorTest.TextColorImporterExample
{
    [ScriptedImporter(1, "saintsfieldexampleformatimporter")]
    public class TextColorImporter : ScriptedImporter
    {
        [LayoutStart("H", ELayout.Horizontal)]

        [NoLabel]
        public string useName;
        [NoLabel]
        public Color fallbackColor = Color.white;

        [Button]
        public void MyButton() {
            //...
        }
        [LayoutEnd]

        public override void OnImportAsset(AssetImportContext ctx)
        {
        }
    }
}
