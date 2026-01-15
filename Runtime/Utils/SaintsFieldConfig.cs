using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(fileName = "SaintsFieldConfig", menuName = "ScriptableObject/SaintsFieldConfig", order = 20)]
#endif
    public class SaintsFieldConfig : ScriptableObject
    {
        public const int PreParserVersion = 0;
        public const string PreParserRelativeFolder = "Library/SaintsFieldTemp";

        public const int UpdateLoopDefaultMs = 100;

        [LayoutStart("MonoBehavior Default Searchable", ELayout.TitleOut)]
        [AboveText("This will add [Searchable] to all MonoBehavior")]
        [LayoutStart("./Configs", ELayout.Horizontal)]
        [FieldLabelText("Override Searchable")] public bool monoBehaviorSearchableOverride;
        [ShowIf(nameof(monoBehaviorSearchableOverride)), FieldLabelText("Always Searchable")] public bool monoBehaviorSearchable = MonoBehaviorSearchableDefault;
        [ShowInInspector, HideIf(nameof(monoBehaviorSearchableOverride)), LabelText("Always Searchable")] public const bool MonoBehaviorSearchableDefault = true;
        [LayoutEnd]

        // [FieldInfoBox("The minimum row of resizable text area", EMessageType.None)]
        [LayoutStart("The minimum row of resizable text area", ELayout.Horizontal | ELayout.TitleOut)]
        [FieldLabelText("Override ResizableTextArea.MinRow")] public bool resizableTextAreaMinRowOverride;
        [ShowInInspector, PlayaHideIf(nameof(resizableTextAreaMinRowOverride)), LabelText(null)] public const int ResizableTextAreaMinRowDefault = 3;
        [LayoutEnd]
        [ShowIf(nameof(resizableTextAreaMinRowOverride)), NoLabel] public int resizableTextAreaMinRow = ResizableTextAreaMinRowDefault;

        [LayoutStart("Should the ValidateInput use loop check?", ELayout.TitleOut)]
        [LeftToggle] public bool validateInputLoopCheckUIToolkit = ValidateInputLoopCheckDefault;
        public const bool ValidateInputLoopCheckDefault = false;

        [Space]

        [LayoutStart("Aggressive OnValueChanged watcher", ELayout.TitleOut)]
        [AboveText("This allows UI Toolkit to monitor changes inside fields of `SerializedReference` or a `Serializable` generic class. In some Unity versions, if the target is an array/list of SerializedReferences, it will give errors when removing an item from the list. Set it to `true` if you faces the error when removing items from list")]
        [LeftToggle] public bool disableOnValueChangedWatchArrayFieldUIToolkit;

        [LayoutStart("Auto Getters Configs", ELayout.FoldoutBox, marginTop: 10)]

        [LayoutStart("./GetComponent.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetComponent.EXP")] public bool getComponentExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getComponentExpOverride)), LabelText(null)] public const EXP GetComponentExpDefault = GetComponentAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getComponentExpOverride)), NoLabel] public EXP getComponentExp = GetComponentExpDefault;

        [Separator]

        [LayoutStart("./GetComponentInChildren.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetComponentInChildren.EXP")] public bool getComponentInChildrenExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getComponentInChildrenExpOverride)), LabelText(null)] public const EXP GetComponentInChildrenExpDefault = GetComponentInChildrenAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getComponentInChildrenExpOverride)), NoLabel] public EXP getComponentInChildrenExp = GetComponentInChildrenExpDefault;

        [Separator]

        [LayoutStart("./GetComponentInParent.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetComponentInParent.EXP")] public bool getComponentInParentExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getComponentInParentExpOverride)), LabelText(null)] public const EXP GetComponentInParentExpDefault = GetComponentInParentAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getComponentInParentExpOverride)), NoLabel] public EXP getComponentInParentExp = GetComponentInParentExpDefault;

        [Separator]

        [LayoutStart("./GetComponentInParents.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetComponentInParent.EXP")] public bool getComponentInParentsExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getComponentInParentsExpOverride)), LabelText(null)] public const EXP GetComponentInParentsExpDefault = GetComponentInParentsAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getComponentInParentsExpOverride)), NoLabel] public EXP getComponentInParentsExp = GetComponentInParentsExpDefault;

        [Separator]

        [LayoutStart("./GetComponentInScene.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetComponentInParent.EXP")] public bool getComponentInSceneExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getComponentInSceneExpOverride)), LabelText(null)] public const EXP GetComponentInSceneExpDefault = FindObjectsByTypeAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getComponentInSceneExpOverride)), NoLabel] public EXP getComponentInSceneExp = GetComponentInSceneExpDefault;

        [Separator]

        [LayoutStart("./GetPrefabWithComponent.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetPrefabWithComponent.EXP")] public bool getPrefabWithComponentExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getPrefabWithComponentExpOverride)), LabelText(null)] public const EXP GetPrefabWithComponentExpDefault = GetPrefabWithComponentAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getPrefabWithComponentExpOverride)), NoLabel] public EXP getPrefabWithComponentExp = GetPrefabWithComponentExpDefault;

        [Separator]

        [LayoutStart("./GetScriptableObject.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetScriptableObject.EXP")] public bool getScriptableObjectExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getScriptableObjectExpOverride)), LabelText(null)] public const EXP GetScriptableObjectExpDefault = GetScriptableObjectAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getScriptableObjectExpOverride)), NoLabel] public EXP getScriptableObjectExp = GetScriptableObjectExpDefault;

        [Separator]

        [LayoutStart("./GetByXPath.EXP", ELayout.Horizontal)]
        [FieldLabelText("Override GetByXPath.EXP")] public bool getByXPathExpOverride;
        [ShowInInspector, PlayaHideIf(nameof(getByXPathExpOverride)), LabelText(null)] public const EXP GetByXPathExpDefault = GetByXPathAttribute.DefaultEXP;
        [LayoutEnd(".")]
        [EnumToggleButtons, FieldDefaultExpand, ShowIf(nameof(getByXPathExpOverride)), NoLabel] public EXP getByXPathExp = GetByXPathExpDefault;

        // [EnumToggleButtons, FieldDefaultExpand] public EXP getByXPathExp = EXP.None;

        [LayoutStart("Deprecated", ELayout.CollapseBox)]

        [FieldInfoBox("Deprecated", EMessageType.Warning)]
        [FieldInfoBox("IMGUI: Space for foldout. If you see overlap in expandable, set this value to 13, otherwise 0")]
        [MinValue(0)] public int foldoutSpaceImGui = FoldoutSpaceImGuiDefault;
        public const int FoldoutSpaceImGuiDefault = 13;

        [FieldInfoBox("Deprecated", EMessageType.Warning)]
        [EnumToggleButtons, FieldDefaultExpand] public EXP getComponentByPathExp = EXP.NoAutoResignToValue | EXP.NoAutoResignToNull | EXP.NoPicker;
        [FieldInfoBox("Deprecated", EMessageType.Warning)]
        [EnumToggleButtons, FieldDefaultExpand] public EXP findComponentExp = EXP.NoPicker | EXP.NoAutoResignToNull;

        [LayoutStart("Hidden Configs", ELayout.Collapse)]
        public bool setupWindowPopOnce;
        public bool GetSetupWindowPopOnce()
        {
#if SAINTSFIELD_SAINTS_EDITOR_APPLY
            return true;
#else
            return setupWindowPopOnce;
#endif
        }

// #if UNITY_EDITOR
//         public readonly UnityEvent OnDeleteConfig = new UnityEvent();
//
//         [LayoutEnd]
//         [Button("Delete Config File"), Ordered]
//         private void DeleteConfig()
//         {
//             DestroyImmediate(this, true);
//             AssetDatabase.DeleteAsset(SaintsFieldConfigUtil.ConfigAssetPath);
//             OnDeleteConfig.Invoke();
//         }
// #endif
    }
}
