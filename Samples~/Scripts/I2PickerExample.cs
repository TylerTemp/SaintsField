#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class I2PickerExample : MonoBehaviour
    {
#if SAINTSFIELD_I2_LOC
        // public I2.Loc.LanguageSource _languageSource;

        [I2Loc.LocalizedStringPicker] public I2.Loc.LocalizedString ls1;

        [OnValueChanged(nameof(SetLang)), OptionsValueButtons("English", "Chinese")] public string lang;

        private void SetLang(string newLang)
        {
            I2.Loc.LocalizationManager.CurrentLanguage = newLang;
            // Debug.Log($"set to {I2.Loc.LocalizationManager.CurrentLanguage}");
        }

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

                // I2.Loc.LocalizationManager.OnLocalizeEvent += () => Debug.Log("OnLocalizeEvent on script");
            };

        }
#endif

#endif
    }
}
