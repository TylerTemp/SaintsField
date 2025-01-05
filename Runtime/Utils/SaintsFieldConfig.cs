using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Utils
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "SaintsFieldConfig", menuName = "ScriptableObject/SaintsFieldConfig", order = 20)]
#endif
    public class SaintsFieldConfig : ScriptableObject
    {
        public const int UpdateLoopDefaultMs = 100;

        [DebugTool.WhichFramework]
        [Separator(10)]

        [InfoBox("The minimum row of resizable text area", EMessageType.None)]
        [MinValue(1)] public int resizableTextAreaMinRow = 3;

        [Space]

        [InfoBox("UI Toolkit: Aggressive OnValueChanged watcher\n\nThis allows UI Toolkit to monitor changes inside fields of `SerializedReference` or a `Serializable` generic class. In some Unity versions, if the target is an array/list of SerializedReferences, it will give errors when removing an item from the list. Set it to `true` if you faces the error when removing items from list", EMessageType.None)]
        [LeftToggle] public bool disableOnValueChangedWatchArrayFieldUIToolkit;

        [LayoutStart("Auto Getters Configs", ELayout.FoldoutBox, marginTop: 10)]

        [LayoutStart("./Bootstrap", ELayout.FoldoutBox, marginTop: 5, marginBottom: 5)]

        [InfoBox("How much delay should a getter wait until the first resource check?\n0 means as soon as possible. (but not right now, it'll still use some delay)", EMessageType.None)]
        [MinValue(0), AboveRichLabel("Delay (ms) <color=green><b>(UI Toolkit)"), RichLabel(null)]
        public int getByXPathDelayMs;

        [Space]

        [InfoBox("How often should a getter check the resource changes?\n0 means never check it", EMessageType.None)]
        [Separator(5)]
        [MinValue(0), AboveRichLabel("Update Interval (ms) <color=green><b>(UI Toolkit)"), RichLabel(null)]
        [InfoBox("Update Loop Disabled <color=green>(UI Toolkit)", show: nameof(GetByXPathDelayMsDisabled), below: true)]
        public int getByXPathLoopIntervalMs = GetByXPathLoopIntervalDefaultMs;
        public const int GetByXPathLoopIntervalDefaultMs = 1000;

        [MinValue(0), AboveRichLabel("Update Interval (ms) <color=brown><b>(IMGUI)"), RichLabel(null)]
        [InfoBox("Update Loop Disabled <color=red>(IMGUI)</color>", show: nameof(GetByXPathDelayMsDisabled), below: true)]
        public int getByXPathLoopIntervalMsIMGUI = GetByXPathLoopIntervalDefaultMsIMGUI;
        public const int GetByXPathLoopIntervalDefaultMsIMGUI = 2500;

        private bool GetByXPathDelayMsDisabled(int v) => v <= 0;

        [InfoBox("Init Update Pass. Unity can failed on setting/getting values in IMGUI, this helps to re-sign the value with some loops. Please don't set it less than 4.<color=red><b>(IMGUI)</b></color>")]
        [MinValue(1), AboveRichLabel("Field Pass Count<color=brown><b>(IMGUI)"), RichLabel(null)]
        public int getByXPathFieldPassIMGUI = GetByXPathDefaultFieldPassIMGUI;
        public const int GetByXPathDefaultFieldPassIMGUI = 20;
        [MinValue(1), AboveRichLabel("Array Pass Count<color=brown><b>(IMGUI)"), RichLabel(null)]
        public int getByXPathArrayPassIMGUI = GetByXPathDefaultArrayPassIMGUI;
        public const int GetByXPathDefaultArrayPassIMGUI = 10;

        [LayoutEnd(".")]

        [Space]

        [FlagsDropdown] public EXP getComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getComponentInChildrenExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getComponentInParentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getComponentInParentsExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getComponentInSceneExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getPrefabWithComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getScriptableObjectExp = EXP.NoPicker | EXP.NoAutoResignToNull;
        [FlagsDropdown] public EXP getByXPathExp = EXP.None;

        [LayoutStart("./Deprecated", ELayout.CollapseBox)]
        [InfoBox("Deprecated", EMessageType.Warning)]
        [FlagsDropdown] public EXP getComponentByPathExp = EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker;
        [InfoBox("Deprecated", EMessageType.Warning)]
        [FlagsDropdown] public EXP findComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;
    }
}
