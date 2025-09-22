using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue304IMGUIGetter : SaintsMonoBehaviour
    {
        [SerializeField, GetComponentInChildren] private List<Collider> _colliders;
    }
}
