using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.I250
{
    // [CreateAssetMenu(fileName = "SearchObj2", menuName = "Scriptable Objects/SearchObj2")]
    public class SearchObj2 : ScriptableObject
    {
        [SerializeField, Expandable] private SearchObj1 _searchObj1;

        [SerializeField] private string _searchString;
    }
}
