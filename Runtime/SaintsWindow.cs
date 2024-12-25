using System;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField
{
    public class SaintsWindow: ScriptableObject
    {
        [NonSerialized]
        public readonly UnityEvent<ScriptableObject> EditorOnSourceChanged = new UnityEvent<ScriptableObject>();

        public void EditorSourceChange(ScriptableObject so) => EditorOnSourceChanged.Invoke(so);


    }
}
