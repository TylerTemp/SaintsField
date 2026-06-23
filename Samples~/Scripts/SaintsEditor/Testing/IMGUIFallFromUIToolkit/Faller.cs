using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.IMGUIFallFromUIToolkit
{
    public class Faller : SaintsMonoBehaviour
    {
        // public IMGUIType imguiType;

        [Header("Unity header attribute")]
        [IMGUIExtraDec] public string upDecStr;
        // [IMGUIExtraDec] public IMGUIType upDecCustomDraw;
    }
}
