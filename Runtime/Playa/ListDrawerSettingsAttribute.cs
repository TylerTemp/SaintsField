using System;
using System.Diagnostics;
using SaintsField.Utils;
using Debug = UnityEngine.Debug;


namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ListDrawerSettingsAttribute: Attribute, IPlayaAttribute
    {
        public readonly int NumberOfItemsPerPage;
        public readonly bool Searchable;

        // public readonly bool Delayed;

        public readonly string ExtraSearch;
        // public readonly string OverrideSearch;

        public ListDrawerSettingsAttribute(bool searchable = true, int numberOfItemsPerPage = 0, string extraSearch = null, string overrideSearch = null)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
            // Delayed = delayedSearch;
            Searchable = searchable;

            ExtraSearch = RuntimeUtil.ParseCallback(extraSearch).content;
            // OverrideSearch = RuntimeUtil.ParseCallback(overrideSearch).content;

            if (!string.IsNullOrEmpty(overrideSearch))
            {
                Debug.LogWarning("`overrideSearch` is no longer supported and will be ignored");
            }

            if (ExtraSearch != null)
            {
                Searchable = true;
            }
        }

    }
}
