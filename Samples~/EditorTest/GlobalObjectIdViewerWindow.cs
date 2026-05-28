#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.EditorTest
{
    public class GlobalObjectIdViewerWindow : EditorWindow
    {
        private Object targetObject;
        private string globalObjectIdText;
        private string convertIdText;

        [MenuItem("Tools/Global Object ID Viewer")]
        private static void Open()
        {
            GetWindow<GlobalObjectIdViewerWindow>("Global Object ID");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

            using (new EditorGUI.ChangeCheckScope())
            {
                targetObject = EditorGUILayout.ObjectField(
                    "Object",
                    targetObject,
                    typeof(Object),
                    true
                );

                if (GUI.changed)
                {
                    UpdateGlobalObjectId();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("GlobalObjectId", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    string.IsNullOrEmpty(globalObjectIdText)
                        ? "(none)"
                        : globalObjectIdText,
                    GUILayout.MinHeight(40)
                );
            }

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(globalObjectIdText)))
            {
                if (GUILayout.Button("Copy"))
                {
                    EditorGUIUtility.systemCopyBuffer = globalObjectIdText;
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    string.IsNullOrEmpty(convertIdText)
                        ? "(none)"
                        : convertIdText,
                    GUILayout.MinHeight(40)
                );
            }

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(convertIdText)))
            {
                if (GUILayout.Button("Copy"))
                {
                    EditorGUIUtility.systemCopyBuffer = convertIdText;
                }
            }
        }

        private void UpdateGlobalObjectId()
        {
            if (targetObject == null)
            {
                globalObjectIdText = null;
                return;
            }

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(targetObject);
            globalObjectIdText = id.ToString();
            convertIdText = ConvertPrefabGidToUnpackedGid(id).ToString();
        }

        private static GlobalObjectId ConvertPrefabGidToUnpackedGid(GlobalObjectId id)
        {
            ulong fileId = (id.targetObjectId ^ id.targetPrefabId) & 0x7fffffffffffffff;
            bool success = GlobalObjectId.TryParse(
                $"GlobalObjectId_V1-{id.identifierType}-{id.assetGUID}-{fileId}-0",
                out GlobalObjectId unpackedGid);
            // Assert.IsTrue(success);
            return unpackedGid;
        }
    }
}
#endif
