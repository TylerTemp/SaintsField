using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    public class SaintsArrayAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;

        public SaintsArrayAttribute(bool searchable = true, int numberOfItemsPerPage = 0)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Searchable = searchable;
        }
    }
}
