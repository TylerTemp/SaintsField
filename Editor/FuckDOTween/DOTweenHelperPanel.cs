#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.FuckDOTween
{
    // ReSharper disable once InconsistentNaming
    public class DOTweenHelperPanel: EditorWindow
    {

        private static DOTweenHelperPanel _doTweenHelperPanel;

        [InitializeOnLoadMethod]
        // [MenuItem("Window/Saints/Help Panel")]
        public static void ShowHelpPanel()
        {
            if (DoTweenAllGood())
            {
                return;
            }

            // Debug.Log("Popup?");
            if(_doTweenHelperPanel == null)
            {
                _doTweenHelperPanel = GetWindow<DOTweenHelperPanel>(title: "SaintsField DOTween Helper");
            }
            _doTweenHelperPanel.Show();
        }

        private static bool DoTweenAllGood()
        {
            ScriptableObject doTweenSettings = Resources.Load<ScriptableObject>("DOTweenSettings");
            if (doTweenSettings is null)  // bypass life circle check
            {
                return true;
            }
            // Debug.Log(doTweenSettings);
            Type doTweenSettingsType = doTweenSettings.GetType();
            FieldInfo fieldInfo = doTweenSettingsType.GetField("createASMDEF", BindingFlags.Instance | BindingFlags.Public);
            bool createAsmdef = (bool) fieldInfo.GetValue(doTweenSettings);
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
                AddCompileDefine("SAINTSFIELD_DOTWEEN_DISABLED");
                Close();
            }
        }

        private void OnDestroy()
        {
            _doTweenHelperPanel = null;
        }

        private static void AddCompileDefine(string newDefineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
        {
            IEnumerable<BuildTargetGroup> targets = targetGroups ?? Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>();

            foreach (BuildTargetGroup grp in targets.Where(each => each != BuildTargetGroup.Unknown))
            {
                string defines;
                try
                {
                    defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                if (!defines.Contains(newDefineCompileConstant))
                {
                    if (defines.Length > 0)
                        defines += ";";

                    defines += newDefineCompileConstant;
                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
#endif
