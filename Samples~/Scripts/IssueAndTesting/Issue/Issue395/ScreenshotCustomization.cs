using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue395
{
    [Serializable]
    public class ScreenshotCustomization
    {
        public Sprite TopImage;
        public Sprite BottomImage;
        public Sprite LeftImage;
        public Sprite RightImage;
        public List<SpineMoment> SpineMoments;
    }
}
