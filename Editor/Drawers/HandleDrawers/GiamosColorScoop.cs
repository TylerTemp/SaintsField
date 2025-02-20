using System;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    public class GiamosColorScoop: IDisposable
    {
        private readonly Color _oldColor;

        public GiamosColorScoop(Color newColor)
        {
            _oldColor = Gizmos.color;
            Gizmos.color = newColor;
        }

        public void Dispose()
        {
            Gizmos.color = _oldColor;
        }
    }
}
