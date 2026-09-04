#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections;
using SaintsField.Editor;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Editor
{
    public class ToasterTestWindow: SaintsEditorWindow
    {
#if SAINTSFIELD_DEBUG
        [MenuItem(RuntimeUtil.MenuRoot + "DEBUG Toaster")]
#endif
        private static void OpenWindow()
        {
            ToasterTestWindow window = GetWindow<ToasterTestWindow>(false, "DEBUG Toaster");
            // window.minSize = new Vector2(400, 470);
            window.Show();
        }

        [Button]
        private IEnumerator Btn()
        {
            for (int i = 0; i < 10; i++)
            {
                if (i % 3 == 0)
                {
                    EditorToastWarning($"Failed to save {i}");
                }

                yield return new WaitForSeconds(0.2f);
            }

            EditorToastSuccess("Save Finished!");
        }
    }
}
#endif
