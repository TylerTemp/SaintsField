// #if UNITY_2022_2_OR_NEWER || SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Diagnostics;
using SaintsField.Playa;
using SaintsField.Utils;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class TableAttribute: Attribute, IPlayaAttribute
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool HideAddButton;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool HideRemoveButton;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool DefaultCollapse;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int NumberOfItemsPerPage;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool SearchableAll;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool SearchableCols;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public string ExtraSearch;

        public TableAttribute(bool hideAddButton=false, bool hideRemoveButton=false, bool defaultCollapse=false,
            bool searchable=false, int numberOfItemsPerPage=0, string extraSearch=null,
            bool searchableAll=false, bool searchableCols=true)
        {
            // DefaultExpanded = defaultExpanded;
            HideAddButton = hideAddButton;
            HideRemoveButton = hideRemoveButton;
            DefaultCollapse = defaultCollapse;
            NumberOfItemsPerPage = numberOfItemsPerPage;
            ExtraSearch = RuntimeUtil.ParseCallback(extraSearch).content;
            bool hasExtraSearch = !string.IsNullOrEmpty(ExtraSearch);
            SearchableAll = searchable || searchableAll;
            SearchableCols = searchable || searchableCols || hasExtraSearch;
        }
    }
}
// #endif
