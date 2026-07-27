using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue407
{
    public class Issue407Dropdown: SaintsMonoBehaviour
    {
        [Serializable]
        public struct AudioGroup
        {
            public string name;

            public override string ToString()
            {
                return $"Audio Group: {name}";
            }
        }

        public bool HasNoAudioLib;

        //again all fine here
        [HideIf(nameof(HasNoAudioLib)), SerializeField, Dropdown(nameof(AudioGroupDrop))]
        private AudioGroup _audioGroup;
        public AudioGroup GetAudioGroup() { return _audioGroup; }

        public bool HasNoAudioGroup;

        //and here the console spit out the error when I try to select something
        [HideIf(nameof(HasNoAudioGroup)), SerializeField, Dropdown(nameof(AudioClipDrop))]
        private GameObject _audioClip;


        private static readonly AudioGroup[] options = new[]
        {
            new AudioGroup { name = "Audio Clip 1" },
            new AudioGroup { name = "Audio Clip 2" },
        };

        //this is all fine
        private IEnumerable<AudioGroup> AudioGroupDrop()
        {
            return options;
        }

        [SerializeField, GetInChildren] private GameObject[] _goOptions;

        //this causes a Compiler Error
        private Dropdown<GameObject> AudioClipDrop()
        {
            Dropdown<GameObject> dropdownList = new Dropdown<GameObject>
            {
                {"Null", null},
            };
            foreach (GameObject entry in _goOptions)
            {
                dropdownList.Add(entry.GetComponent<AudioSource>().clip.name,  entry);
            }
            return dropdownList;
        }
    }
}
