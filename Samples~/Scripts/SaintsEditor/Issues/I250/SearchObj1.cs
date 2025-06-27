using System;
using SaintsField.Playa;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.I250
{
    // [CreateAssetMenu(fileName = "SearchObj1", menuName = "Scriptable Objects/SearchObj1")]
    public class SearchObj1 : SaintsScriptableObject
    {
        [SerializeField, Expandable] private SearchObj2 _searchObj2;

        [Serializable]
        public class ReferenceLooped
        {
            public ReferenceLooped refLooped;
            public string loopedText;
        }

        [SerializeField] private ReferenceLooped _loopedRef;

        [Button]
        private void LoopIt()
        {
            ReferenceLooped looped = new ReferenceLooped
            {
                loopedText = "LOOPEDTEXT",
            };
            looped.refLooped = looped;
#if UNITY_EDITOR
            Undo.RecordObject(this, name);
#endif
            _loopedRef = looped;
        }
    }
}
