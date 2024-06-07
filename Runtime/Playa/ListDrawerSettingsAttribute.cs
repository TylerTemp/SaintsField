using System;
using System.Diagnostics;


namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class ListDrawerSettingsAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable once InconsistentNaming
        public readonly int NumberOfItemsPerPage;

        public ListDrawerSettingsAttribute(int numberOfItemsPerPage = 0)
        {
            NumberOfItemsPerPage = numberOfItemsPerPage;
        }

    }
}
