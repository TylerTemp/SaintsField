using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Utils
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "SaintsFieldConfig", menuName = "ScriptableObject/SaintsFieldConfig", order = 20)]
#endif
    public class SaintsFieldConfig : ScriptableObject
    {
        [LayoutStart("Auto Getters Configs", ELayout.FoldoutBox)]

        [EnumFlags(defaultExpanded: false)] public EXP getComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInChildrenExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInParentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInParentsExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInSceneExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getPrefabWithComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getScriptableObjectExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getByXPathExp = EXP.None;

        [InfoBox("Deprecated", EMessageType.Warning)]
        [EnumFlags(defaultExpanded: false)] public EXP getComponentByPathExp = EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker;
        [InfoBox("Deprecated", EMessageType.Warning)]
        [EnumFlags(defaultExpanded: false)] public EXP findComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
    }
}
