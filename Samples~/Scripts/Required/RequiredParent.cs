using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Required
{
    public class RequiredParent : MonoBehaviour
    {
        [SerializeField, Required] protected GameObject _gameObject;
    }
}
