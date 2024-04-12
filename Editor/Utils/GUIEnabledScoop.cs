using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class GUIEnabledScoop: IDisposable
    {
        private readonly bool defaultValue;

        public GUIEnabledScoop(bool value)
        {
            defaultValue = GUI.enabled;
            GUI.enabled = value;
        }

        public void Dispose()
        {
            GUI.enabled = defaultValue;
        }
    }
}
