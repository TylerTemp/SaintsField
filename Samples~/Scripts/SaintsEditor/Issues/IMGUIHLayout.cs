using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class IMGUIHLayout : SaintsMonoBehaviour
    {
        [LayoutStart("IMGUI", ELayout.Horizontal)]

        [NoLabel] public string a;
        [NoLabel] public string b;
        [NoLabel] public string c;

        [Serializable]
        public struct StructLayout
        {
            [LayoutStart("IMGUI", ELayout.Horizontal)]
            [NoLabel] public string a;
            [NoLabel] public string b;
            [NoLabel] public string c;
        }

        [LayoutEnd]
        [SaintsRow] public StructLayout structLayout;
    }
}
