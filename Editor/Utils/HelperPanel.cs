using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class HelperPanel: EditorWindow
    {
#if DOTWEEN
        private static HelperPanel _helperPanel;

        [InitializeOnLoadMethod]
        [MenuItem("Window/Saints/Help Panel")]
        public static void ShowHelpPanel()
        {
            ScriptableObject doTweenSettings = Resources.Load<ScriptableObject>("DOTweenSettings");
            Debug.Log(doTweenSettings);

            Type doTweenSettingsType = doTweenSettings.GetType();
            FieldInfo fieldInfo = doTweenSettingsType.GetField("createASMDEF", BindingFlags.Instance | BindingFlags.Public);
            bool createAsmdef = (bool) fieldInfo!.GetValue(doTweenSettings);
            Debug.Log(createAsmdef);
            if (createAsmdef)
            {
                return;
            }

            // Debug.Log("Popup?");
            if(_helperPanel == null)
            {
                _helperPanel = CreateInstance<HelperPanel>();
            }
            _helperPanel.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("DOTween ASMDEF not created, please create it or disable SaintsField's DOTween ability", MessageType.Error);
        }

        private void OnDestroy()
        {
            _helperPanel = null;
        }
#endif
    }
}
