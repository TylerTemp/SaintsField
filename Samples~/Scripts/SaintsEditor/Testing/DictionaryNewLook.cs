using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class DictionaryNewLook : SaintsMonoBehaviour
    {
        [SaintsDictionary(numberOfItemsPerPage: 5), DefaultExpand] public SaintsDictionary<int, ScriptableObject> p;

        public SaintsDictionary<int, ScriptableObject> pNoConf;

        [ShowInInspector]
        private Dictionary<int, ScriptableObject> _showI;
    }
}
