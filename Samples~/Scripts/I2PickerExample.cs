#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.IO;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class I2PickerExample : MonoBehaviour
    {
#if SAINTSFIELD_I2_LOC
        // public I2.Loc.LanguageSource _languageSource;

        [I2Loc.LocalizedStringPicker] public I2.Loc.LocalizedString ls1;

        [PostFieldButton(nameof(SetLang))] public string lang;

        private void SetLang(string newLang) => I2.Loc.LocalizationManager.CurrentLanguage = newLang;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void EditorLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            I2.Loc.LocalizationManager.InitializeIfNeeded();

            // I2.Loc.LanguageSource
            I2.Loc.LanguageSourceAsset languageSource =  AssetDatabase.LoadAssetAtPath<I2.Loc.LanguageSourceAsset>(
                "Packages/today.comes.saintsfield/Samples/RawResources/I2Languages.asset");

            EditorApplication.delayCall += () =>
            {
                try
                {
                    I2.Loc.LocalizationManager.Sources.Add(languageSource.SourceData);
                    Debug.Log($"Load {languageSource} into LocalizationManager.Sources");
                }
                catch (Exception)
                {
                    Debug.Log($"Failed to load {languageSource} into LocalizationManager.Sources, retry later");
                    EditorLoad();
                }
            };

        }
#endif

#endif
    }
}
