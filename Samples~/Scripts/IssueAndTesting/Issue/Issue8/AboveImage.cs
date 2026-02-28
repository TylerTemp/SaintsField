using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class AboveImage : AboveImageBase
    {
        [AboveImage(nameof(_sprite), maxWidth: 15)] public string showImage;

        [Serializable]
        public struct MyStruct
        {
            public Sprite mySprite;

            [AboveImage(nameof(mySprite), maxWidth: 15)] public string showImage;
            [AboveImage(nameof(_sprite), maxWidth: 15)] public string escapedImage;
        }

        public MyStruct myStruct;

        public Sprite multi1;
        public Sprite multi2;

        [AboveImage(nameof(multi1)), AboveImage(nameof(multi2))] public string multi;
    }
}
