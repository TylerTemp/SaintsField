using System;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public readonly struct GUIEnabledScoop: IDisposable
    {
        private readonly bool _defaultValue;

        public GUIEnabledScoop(bool value)
        {
            _defaultValue = GUI.enabled;
            GUI.enabled = value;
        }

        public void Dispose()
        {
            GUI.enabled = _defaultValue;
        }
    }
}
