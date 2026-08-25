using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class DictionaryNewLook : SaintsMonoBehaviour
    {
        [SaintsDictionary(numberOfItemsPerPage: 5, extraSearch: nameof(DictSearch)),
         DefaultExpand,
         ValueAttribute(typeof(FieldDefaultExpandAttribute))]
        public SaintsDictionary<int, ScriptableObject> p;

        private bool DictSearch(KeyValuePair<int, ScriptableObject> pair, IReadOnlyList<ListSearchToken> keyTokens,
            IReadOnlyList<ListSearchToken> valueTokens)
        {
            bool keyFound = RuntimeUtil.SimpleSearch($"{pair.Key}", keyTokens);
            bool valueFound = RuntimeUtil.SimpleSearch(pair.Value?.name ?? "", valueTokens)  // search name
                || RuntimeUtil.SimpleSearch((pair.Value as Scriptable)?.noLabel ?? "", valueTokens);  // search `noLabel` field

            return keyFound && valueFound;
        }
    }
}
