using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Utils
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "SaintsFieldConfig", menuName = "ScriptableObject/SaintsFieldConfig", order = 20)]
#endif
    public class SaintsFieldConfig : ScriptableObject
    {
        [InfoBox("The minimum row of resizable text area", EMessageType.None)]
        [MinValue(1)] public int resizableTextAreaMinRow = 3;

        // [Space]
        //
        // [InfoBox("UI Toolkit: Aggressive OnValueChanged watcher", EMessageType.None)]
        // [InfoBox("This allows UI Toolkit to monitor changes inside fields of `SerializedReference` or a `Serializable` generic class. In some Unity versions, if the target is an array/list of SerializedReferences, it will give errors when removing an item from the list. Set it to `true` if you faces the error when removing items from list", EMessageType.None)]
        // [LeftToggle] public bool disableOnValueChangedWatchArrayFieldUIToolkit;

        [LayoutStart("Auto Getters Configs", ELayout.FoldoutBox, marginTop: 10)]

        [EnumFlags(defaultExpanded: false)] public EXP getComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInChildrenExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInParentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInParentsExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getComponentInSceneExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getPrefabWithComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getScriptableObjectExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [EnumFlags(defaultExpanded: false)] public EXP getByXPathExp = EXP.None;

        [LayoutStart("./Deprecated", ELayout.CollapseBox)]
        [InfoBox("Deprecated", EMessageType.Warning)]
        [EnumFlags(defaultExpanded: false)] public EXP getComponentByPathExp = EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker;
        [InfoBox("Deprecated", EMessageType.Warning)]
        [EnumFlags(defaultExpanded: false)] public EXP findComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
    }
}
