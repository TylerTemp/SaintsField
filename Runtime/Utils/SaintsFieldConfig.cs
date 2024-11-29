using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Utils
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "SaintsFieldConfig", menuName = "ScriptableObject/SaintsFieldConfig", order = 20)]
#endif
    public class SaintsFieldConfig : ScriptableObject
    {
        public const int UpdateLoopDefaultMs = 100;

        [InfoBox("The minimum row of resizable text area", EMessageType.None)]
        [MinValue(1)] public int resizableTextAreaMinRow = 3;

        // [Space]
        //
        [InfoBox("UI Toolkit: Aggressive OnValueChanged watcher", EMessageType.None)]
        [InfoBox("This allows UI Toolkit to monitor changes inside fields of `SerializedReference` or a `Serializable` generic class. In some Unity versions, if the target is an array/list of SerializedReferences, it will give errors when removing an item from the list. Set it to `true` if you faces the error when removing items from list", EMessageType.None)]
        [LeftToggle] public bool disableOnValueChangedWatchArrayFieldUIToolkit;

        [LayoutStart("Auto Getters Configs", ELayout.FoldoutBox, marginTop: 10)]

        [LayoutStart("./Bootstrap", ELayout.FoldoutBox, marginTop: 5, marginBottom: 5)]
        [InfoBox("How much delay should a getter wait until the first resource check. 0 means as soon as possible. (but not right now, it'll still use some delay)", EMessageType.None)]
        [MinValue(0), RichLabel("Delay (ms) (UI Toolkit)")]
        public int getByXPathDelayMs;

        [InfoBox("How often should a getter check the resource changes. 0 means never check it", EMessageType.None)]
        [LeftToggle, RichLabel("Disable Update Loop"), OnValueChanged(nameof(GetByXPathLoopDisabledChanged))]
        public bool getByXPathLoopDisabled;

        private void GetByXPathLoopDisabledChanged(bool disabled)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, name);
            getByXPathLoopIntervalMs = disabled ? 0 : GetByXPathLoopIntervalDefaultMs;
#endif
        }

        [MinValue(0), RichLabel("Update Interval (ms)"), ReadOnly(nameof(getByXPathLoopDisabled))]
        public int getByXPathLoopIntervalMs = GetByXPathLoopIntervalDefaultMs;

        public const int GetByXPathLoopIntervalDefaultMs = 1000;

        [LayoutEnd(".")]

        [Space]

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
