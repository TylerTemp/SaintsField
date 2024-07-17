using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class CustomEventChild : MonoBehaviour
    {
        [field: SerializeField] private UnityEvent<int> _intEvent;
    }
}
