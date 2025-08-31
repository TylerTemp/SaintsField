using UnityEngine;

namespace SaintsField.Editor.HeaderGUI
{
    public interface ISearchable
    {
        // ReSharper disable once InconsistentNaming
        Object target { get; }

        string GetRichLabel();
        void OnHeaderButtonClick();
    }
}
