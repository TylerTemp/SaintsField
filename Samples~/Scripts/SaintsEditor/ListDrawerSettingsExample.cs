using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ListDrawerSettingsExample : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyData
        {
            public int myInt;
            public string myString;
            public GameObject myGameObject;
            public string[] myStrings;
        }

        public string above;

        [ListDrawerSettings(
             searchable: true
             , numberOfItemsPerPage: 3
         ),
         LabelText("<color=green><icon=star.png/><label/>"),
        ]
        public MyData[] myDataArr;

        public string below;


        [ListDrawerSettings(searchable: true), Expandable] public Scriptable[] searchScriptable;


        [ListDrawerSettings(searchable: true), Expandable]
        [OnArraySizeChanged(nameof(SizeChanged))]
        public Scriptable[] searchDelayScriptable;

        private void SizeChanged(IReadOnlyList<Scriptable> scripts) => Debug.Log(scripts.Count);
    }
}
