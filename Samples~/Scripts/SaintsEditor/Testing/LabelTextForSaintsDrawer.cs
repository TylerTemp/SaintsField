#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
using SaintsField.AiNavigation;
#endif
using SaintsField.Events;
using SaintsField.SaintsSerialization;
#if SAINTSFIELD_SPINE_CSHARP && SAINTSFIELD_SPINE_UNITY && !SAINTSFIELD_SPINE_DISABLE
using SaintsField.Spine;
#endif
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LabelTextForSaintsDrawer : SaintsMonoBehaviour
    {
        private const string RichLabel = "<color=orange>[<label/>]<icon=star.png/>";

        public enum PlainEnum
        {
            One,
            Two,
            Three,
        }

        [System.Flags]
        public enum FlagsEnum
        {
            One = 1,
            Two = 1 << 1,
            Three = 1 << 2,
        }

        public interface ILabelTextInterface
        {
        }

        public class LabelTextInterfaceComponent : MonoBehaviour, ILabelTextInterface
        {
        }

        [System.Serializable]
        public struct RowData
        {
            public int intValue;
            public string stringValue;
        }

        [System.Serializable]
        public class ReferenceBase
        {
            public int value;
        }

        [System.Serializable]
        public class ReferenceChild : ReferenceBase
        {
            public string childValue;
        }

        public Animator animator;
        public Material material;

        [LabelText(RichLabel)]
        public SaintsDictionary<string, int> dict;

        [LabelText(RichLabel)]
        [ProgressBar(0, 10)]
        public int prog;

        [LabelText(RichLabel)]
        [AdvancedDropdown(nameof(AdvancedDropdownOptions))]
        public int advancedDropdown;

        [LabelText(RichLabel)]
        [MenuDropdown(nameof(MenuDropdownOptions))]
        public int menuDropdown;

        [LabelText(RichLabel)]
        [TreeDropdown(nameof(AdvancedDropdownOptions))]
        public int treeDropdown;

        [LabelText(RichLabel)]
        [ValueButtons(nameof(AdvancedDropdownOptions))]
        public int valueButtons;

        [LabelText(RichLabel)]
        [FlagsDropdown]
        public FlagsEnum flagsDropdown;

        [LabelText(RichLabel)]
        [EnumToggleButtons]
        public PlainEnum enumToggleButtons;

        [LabelText(RichLabel)]
        [EnumToggleButtons]
        public FlagsEnum enumToggleButtonsFlags;

#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [LabelText(RichLabel)]
        [NavMeshArea]
        public int navMeshArea;

        [LabelText(RichLabel)]
        [NavMeshAreaMask]
        public int navMeshAreaMask;
#endif

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [LabelText(RichLabel)]
        [AddressableAddress]
        public string addressableAddress;

        [LabelText(RichLabel)]
        [AddressableLabel]
        public string addressableLabel;

        [LabelText(RichLabel)]
        [AddressableScene]
        public string addressableScene;
#endif

        [LabelText(RichLabel)]
        [AnimatorParam(nameof(animator))]
        public string animatorParam;

        [LabelText(RichLabel)]
        [AnimatorState(nameof(animator))]
        public string animatorState;

        [LabelText(RichLabel)]
        [InputAxis]
        public string inputAxis;

        [LabelText(RichLabel)]
        [Layer]
        public int layer;

        [LabelText(RichLabel)]
        [Rate(0, 5)]
        public int rate;

        [LabelText(RichLabel)]
        [SortingLayer]
        public string sortingLayer;

        [LabelText(RichLabel)]
        [Scene]
        public string scene;

        [LabelText(RichLabel)]
        public SceneReference sceneReference;

#if UNITY_2021_2_OR_NEWER
        [LabelText(RichLabel)]
        [ShaderKeyword(nameof(material))]
        public string shaderKeyword;

        [LabelText(RichLabel)]
        [ShaderParam(nameof(material))]
        public string shaderParam;
#endif

#if SAINTSFIELD_SPINE_CSHARP && SAINTSFIELD_SPINE_UNITY && !SAINTSFIELD_SPINE_DISABLE
        [LabelText(RichLabel)]
        [SpineAttachmentPicker]
        public string spineAttachment;

        [LabelText(RichLabel)]
        [SpineSkinPicker]
        public string spineSkin;

        [LabelText(RichLabel)]
        [SpineSlotPicker]
        public string spineSlot;

        [LabelText(RichLabel)]
        [SpineBonePicker]
        public string spineBone;

        [LabelText(RichLabel)]
        [SpineEventPicker]
        public string spineEvent;

        [LabelText(RichLabel)]
        [SpinePathConstraintPicker]
        public string spinePathConstraint;

        [LabelText(RichLabel)]
        [SpineIkConstraintPicker]
        public string spineIkConstraint;

        [LabelText(RichLabel)]
        [SpineTransformConstraintPicker]
        public string spineTransformConstraint;
#endif

        [LabelText(RichLabel)]
        [TypeReference]
        public TypeReference typeReference;

        [LabelText(RichLabel)]
        [DateTime]
        public long dateTimeTicks = 638000000000000000L;

        [LabelText(RichLabel)]
        [TimeSpan]
        public long timeSpanTicks = 36000000000L;

        [LabelText(RichLabel)]
        [Guid]
        public string guid = "00000000-0000-0000-0000-000000000000";

        [LabelText(RichLabel)]
        [CurveRange(EColor.Orange)]
        public AnimationCurve curveRange = AnimationCurve.Linear(0, 0, 1, 1);

        [LabelText(RichLabel)]
        [FieldType(typeof(GameObject))]
        public GameObject fieldType;

        [LabelText(RichLabel)]
        [ResourcePath(EStr.Resource, typeof(GameObject))]
        public string resourcePath;

        [LabelText(RichLabel)]
        [MinMaxSlider(0f, 1f, 0.1f)]
        public Vector2 minMaxSlider = new Vector2(0.25f, 0.75f);

        [LabelText(RichLabel)]
        [PropRange(0, 10)]
        public int propRange;

        [LabelText(RichLabel)]
        [ResizableTextArea]
        public string resizableTextArea;

        [LabelText(RichLabel)]
        [Tag]
        public string tagName;

#if UNITY_2021_3_OR_NEWER
        [LabelText(RichLabel)]
        [SerializeReference]
        [ReferencePicker]
        public ReferenceBase referencePicker = new ReferenceChild();
#endif

        [LabelText(RichLabel)]
        public Placeholder placeholder;

        [LabelText(RichLabel)]
        public SaintsDecimal saintsDecimal = new SaintsDecimal(12.5m);

        [LabelText(RichLabel)]
        [SaintsRow]
        public RowData saintsRow;

        [LabelText(RichLabel)]
        [SaintsArray(numberOfItemsPerPage: 5)]
        public SaintsArray<int> saintsArray;

        [LabelText(RichLabel)]
        [SaintsHashSet(numberOfItemsPerPage: 5)]
        public SaintsHashSet<int> saintsHashSet = new SaintsHashSet<int> { 1, 2, 3 };

        [LabelText(RichLabel)]
        public SaintsInterface<Component, ILabelTextInterface> saintsInterface;

        [LabelText(RichLabel)]
        public SaintsEvent<int> saintsEvent;

        [SaintsSerializedActual(nameof(serializedActualDateTime))]
        [LabelText(RichLabel)]
        [SaintsSerializedActualDrawer]
        public SaintsSerializedProperty serializedActualDrawer = new SaintsSerializedProperty
        {
            propertyType = SaintsPropertyType.DateTime,
            longValue = 638000000000000000L,
        };

        public long serializedActualDateTime = 638000000000000000L;

        private DropdownList<int> MenuDropdownOptions() => new DropdownList<int>
        {
            { "One", 1 },
            { "Two", 2 },
            { "Nested/Three", 3 },
        };

        private AdvancedDropdownList<int> AdvancedDropdownOptions() => new AdvancedDropdownList<int>("Numbers")
        {
            new AdvancedDropdownList<int>("Small")
            {
                new AdvancedDropdownList<int>("One", 1),
                new AdvancedDropdownList<int>("Two", 2),
            },
            new AdvancedDropdownList<int>("Three", 3),
        };
    }
}
