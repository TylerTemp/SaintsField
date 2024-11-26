using System;
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
         PlayaRichLabel("<color=green><icon=star.png/><label/>"),
        ]
        public MyData[] myDataArr;

        public string below;


        [ListDrawerSettings(searchable: true), GetScriptableObject, Expandable] public Scriptable[] searchScriptable;
        [ListDrawerSettings(searchable: true, delayedSearch: true), GetScriptableObject, Expandable] public Scriptable[] searchDelayScriptable;
    }
}
