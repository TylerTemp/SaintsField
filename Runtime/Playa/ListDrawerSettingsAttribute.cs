using System;
using System.Diagnostics;
using SaintsField.Utils;


namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ListDrawerSettingsAttribute: Attribute, IPlayaAttribute
    {
        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;

        public readonly bool Delayed;

        public readonly string ExtraSearch;
        public readonly string OverrideSearch;

        public ListDrawerSettingsAttribute(bool searchable = false, int numberOfItemsPerPage = 0, bool delayedSearch = false, string extraSearch = null, string overrideSearch = null)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
            Delayed = delayedSearch;
            Searchable = Delayed || searchable;

            ExtraSearch = RuntimeUtil.ParseCallback(extraSearch).content;
            OverrideSearch = RuntimeUtil.ParseCallback(overrideSearch).content;

            if (ExtraSearch != null || OverrideSearch != null)
            {
                Searchable = true;
            }
        }

    }
}
