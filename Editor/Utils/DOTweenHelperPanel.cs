#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class DOTweenHelperPanel: EditorWindow
    {

        private static DOTweenHelperPanel _doTweenHelperPanel;

        [InitializeOnLoadMethod]
        [MenuItem("Window/Saints/Help Panel")]
        public static void ShowHelpPanel()
        {
            if (DoTweenAllGood())
            {
                return;
            }

            // Debug.Log("Popup?");
            if(_doTweenHelperPanel == null)
            {
                _doTweenHelperPanel = GetWindow<DOTweenHelperPanel>();
                _doTweenHelperPanel.titleContent = new GUIContent("SaintsField DOTween Helper");
            }
            _doTweenHelperPanel.Show();
        }

        private static bool DoTweenAllGood()
        {
            ScriptableObject doTweenSettings = Resources.Load<ScriptableObject>("DOTweenSettings");
            // Debug.Log(doTweenSettings);
            Type doTweenSettingsType = doTweenSettings.GetType();
            FieldInfo fieldInfo = doTweenSettingsType.GetField("createASMDEF", BindingFlags.Instance | BindingFlags.Public);
            bool createAsmdef = (bool) fieldInfo!.GetValue(doTweenSettings);
            // Debug.Log(createAsmdef);
            return createAsmdef;
        }

        private void OnGUI()
        {
            if (DoTweenAllGood())
            {
                Close();
            }

            EditorGUILayout.HelpBox("DOTween ASMDEF not created, please create it or disable SaintsField's DOTween ability", MessageType.Error);
            if (GUILayout.Button("Open DOTween Panel"))
            {
                EditorApplication.ExecuteMenuItem("Tools/Demigiant/DOTween Utility Panel");
            }

            if (GUILayout.Button("Disable SaintsField's DOTween utility"))
            {
                SaintsMenu.AddCompileDefine("SAINTSFIELD_DOTWEEN_DISABLED");
                Close();
            }
        }

        private void OnDestroy()
        {
            _doTweenHelperPanel = null;
        }
    }
}
#endif
