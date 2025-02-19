using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue147
{
    public class Issue147Main : MonoBehaviour
    {
        [FieldType(typeof(MCPrefabEntity)), OnValueChanged(nameof(ExtractPrefabEntitySO))]
        public MCPrefab_SO prefab;

        public void ExtractPrefabEntitySO(MCPrefab_SO prefabSo)
        {
            Debug.Log(prefabSo);
        }
    }
}
