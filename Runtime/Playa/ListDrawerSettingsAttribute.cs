using System;
using System.Diagnostics;


namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ListDrawerSettingsAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable InconsistentNaming
        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;

        public readonly bool Delayed;
        // ReSharper enable InconsistentNaming

        public ListDrawerSettingsAttribute(bool searchable = false, int numberOfItemsPerPage = 0, bool delayedSearch = false)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Delayed = delayedSearch;
            Searchable = Delayed || searchable;
        }

    }
}
