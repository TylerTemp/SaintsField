using System;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
    public class HandlesBeginGUIScoop: IDisposable
    {
        public HandlesBeginGUIScoop()
        {
            Handles.BeginGUI();
        }

        public void Dispose()
        {
            Handles.EndGUI();
        }
    }
}
