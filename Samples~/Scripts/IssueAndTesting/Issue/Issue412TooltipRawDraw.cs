#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
using SaintsField.AiNavigation;
#endif
#if SAINTSFIELD_SERIALIZATION
using SaintsField.Events;
#endif
#if SAINTSFIELD_SPINE_CSHARP && SAINTSFIELD_SPINE_UNITY && !SAINTSFIELD_SPINE_DISABLE
using SaintsField.Spine;
#endif
using System;
using System.Collections.Generic;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
#if SAINTSFIELD_UNITY_MATHEMATICS && !SAINTSFIELD_UNITY_MATHEMATICS_DISABLE
using Unity.Mathematics;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue412TooltipRawDraw : SaintsMonoBehaviour
    {
        [Serializable]
        public enum Mode
        {
            LocalPivotPlusOffset, TwoLocalPivots,
            WorldPivotPlusOffset, TwoWorldPivots
        }

        [Flags]
        public enum FlagMode
        {
            One = 1,
            Two = 1 << 1,
            Three = 1 << 2,
        }

        public interface IIssue412TooltipInterface
        {
        }

        public class Issue412TooltipInterfaceComponent : MonoBehaviour, IIssue412TooltipInterface
        {
        }

        [Serializable]
        public struct RowData
        {
            public int intValue;
            public string stringValue;
        }

        [Serializable]
        public class ReferenceBase
        {
            public int value;
        }

        [Serializable]
        public class ReferenceChild : ReferenceBase
        {
            public string childValue;
        }

        public Animator animator;
        public Material material;

        [SerializeField, Tooltip("How to specify the pivots")]
        private Mode _mode = Mode.LocalPivotPlusOffset;

        [SerializeField, Tooltip("Tooltip for AdvancedDropdown"), AdvancedDropdown(nameof(AdvancedDropdownOptions))]
        private int _advancedDropdown;

        [SerializeField, Tooltip("Tooltip for MenuDropdown"), MenuDropdown(nameof(MenuDropdownOptions))]
        private int _menuDropdown;

        [SerializeField, Tooltip("Tooltip for TreeDropdown"), TreeDropdown(nameof(AdvancedDropdownOptions))]
        private int _treeDropdown;

        [SerializeField, Tooltip("Tooltip for ValueButtons"), ValueButtons(nameof(AdvancedDropdownOptions))]
        private int _valueButtons;

        [SerializeField, Tooltip("Tooltip for FlagsDropdown"), FlagsDropdown]
        private FlagMode _flagsDropdown;

        [SerializeField, Tooltip("Tooltip for FlagsTreeDropdown"), FlagsTreeDropdown]
        private FlagMode _flagsTreeDropdown;

        [SerializeField, Tooltip("Tooltip for EnumToggleButtons"), EnumToggleButtons]
        private Mode _enumToggleButtons;

        [SerializeField, Tooltip("Tooltip for EnumToggleButtons flags"), EnumToggleButtons]
        private FlagMode _enumToggleButtonsFlags;

#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [SerializeField, Tooltip("Tooltip for NavMeshArea"), NavMeshArea]
        private int _navMeshArea;

        [SerializeField, Tooltip("Tooltip for NavMeshAreaMask"), NavMeshAreaMask]
        private int _navMeshAreaMask;
#endif

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [SerializeField, Tooltip("Tooltip for AddressableAddress"), AddressableAddress]
        private string _addressableAddress;

        [SerializeField, Tooltip("Tooltip for AddressableLabel"), AddressableLabel]
        private string _addressableLabel;

        [SerializeField, Tooltip("Tooltip for AddressableScene"), AddressableScene]
        private string _addressableScene;
#endif

        [SerializeField, Tooltip("Tooltip for AnimatorParam"), AnimatorParam(nameof(animator))]
        private string _animatorParam;

        [SerializeField, Tooltip("Tooltip for AnimatorState"), AnimatorState(nameof(animator))]
        private string _animatorState;

        [SerializeField, Tooltip("Tooltip for InputAxis"), InputAxis]
        private string _inputAxis;

        [SerializeField, Tooltip("Tooltip for Layer int"), Layer]
        private int _layerInt;

        [SerializeField, Tooltip("Tooltip for Layer string"), Layer]
        private string _layerString;

        [SerializeField, Tooltip("Tooltip for LayerMask"), Layer]
        private LayerMask _layerMask;

        [SerializeField, Tooltip("Tooltip for LeftToggle"), LeftToggle]
        private bool _leftToggle;

        [SerializeField, Tooltip("Tooltip for Rate"), Rate(0, 5)]
        private int _rate;

        [SerializeField, Tooltip("Tooltip for SortingLayer string"), SortingLayer]
        private string _sortingLayerString;

        [SerializeField, Tooltip("Tooltip for SortingLayer int"), SortingLayer]
        private int _sortingLayerInt;

        [SerializeField, Tooltip("Tooltip for Scene string"), Scene]
        private string _sceneString;

        [SerializeField, Tooltip("Tooltip for Scene int"), Scene]
        private int _sceneIndex;

        [SerializeField, Tooltip("Tooltip for SceneReference")]
        private SceneReference _sceneReference;

#if UNITY_2021_2_OR_NEWER
        [SerializeField, Tooltip("Tooltip for ShaderKeyword"), ShaderKeyword(nameof(material))]
        private string _shaderKeyword;

        [SerializeField, Tooltip("Tooltip for ShaderParam string"), ShaderParam(nameof(material))]
        private string _shaderParamString;

        [SerializeField, Tooltip("Tooltip for ShaderParam int"), ShaderParam(nameof(material))]
        private int _shaderParamInt;
#endif

#if SAINTSFIELD_SPINE_CSHARP && SAINTSFIELD_SPINE_UNITY && !SAINTSFIELD_SPINE_DISABLE
        [SerializeField, Tooltip("Tooltip for SpineAttachmentPicker"), SpineAttachmentPicker]
        private string _spineAttachment;

        [SerializeField, Tooltip("Tooltip for SpineSkinPicker"), SpineSkinPicker]
        private string _spineSkin;

        [SerializeField, Tooltip("Tooltip for SpineSlotPicker"), SpineSlotPicker]
        private string _spineSlot;

        [SerializeField, Tooltip("Tooltip for SpineBonePicker"), SpineBonePicker]
        private string _spineBone;

        [SerializeField, Tooltip("Tooltip for SpineEventPicker"), SpineEventPicker]
        private string _spineEvent;

        [SerializeField, Tooltip("Tooltip for SpinePathConstraintPicker"), SpinePathConstraintPicker]
        private string _spinePathConstraint;

        [SerializeField, Tooltip("Tooltip for SpineIkConstraintPicker"), SpineIkConstraintPicker]
        private string _spineIkConstraint;

        [SerializeField, Tooltip("Tooltip for SpineTransformConstraintPicker"), SpineTransformConstraintPicker]
        private string _spineTransformConstraint;
#endif

        [SerializeField, Tooltip("Tooltip for TypeReference"), TypeReference]
        private TypeReference _typeReference;

        [SerializeField, Tooltip("Tooltip for DateTime"), DateTime]
        private long _dateTimeTicks = 638000000000000000L;

        [SerializeField, Tooltip("Tooltip for TimeSpan"), TimeSpan]
        private long _timeSpanTicks = 36000000000L;

        [SerializeField, Tooltip("Tooltip for Guid"), Guid]
        private string _guid = "00000000-0000-0000-0000-000000000000";

        [SerializeField, Tooltip("Tooltip for CurveRange"), CurveRange(EColor.Orange)]
        private AnimationCurve _curveRange = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField, Tooltip("Tooltip for FieldType"), FieldType(typeof(GameObject))]
        private GameObject _fieldType;

        [SerializeField, Tooltip("Tooltip for ResourcePath"), ResourcePath(EStr.Resource, typeof(GameObject))]
        private string _resourcePath;

        [SerializeField, Tooltip("Tooltip for MinMaxSlider Vector2"), MinMaxSlider(0f, 1f, 0.1f)]
        private Vector2 _minMaxSlider = new Vector2(0.25f, 0.75f);

        [SerializeField, Tooltip("Tooltip for MinMaxSlider Vector2Int"), MinMaxSlider(0, 10, 1)]
        private Vector2Int _minMaxSliderInt = new Vector2Int(2, 8);

        [SerializeField, Tooltip("Tooltip for PropRange int"), PropRange(0, 10)]
        private int _propRangeInt;

        [SerializeField, Tooltip("Tooltip for PropRange float"), PropRange(0f, 1f)]
        private float _propRangeFloat;

        [SerializeField, Tooltip("Tooltip for ProgressBar int"), ProgressBar(0, 10)]
        private int _progressBarInt = 5;

        [SerializeField, Tooltip("Tooltip for ProgressBar float"), ProgressBar(0f, 1f)]
        private float _progressBarFloat = 0.5f;

        [SerializeField, Tooltip("Tooltip for ResizableTextArea"), ResizableTextArea]
        private string _resizableTextArea;

        [SerializeField, Tooltip("Tooltip for Tag"), Tag]
        private string _tagName;

        [SerializeField, Tooltip("Tooltip for ReferencePicker"), SerializeReference, ReferencePicker]
        private ReferenceBase _referencePicker = new ReferenceChild();

        [SerializeField, Tooltip("Tooltip for Placeholder")]
        private Placeholder _placeholder;

        [SerializeField, Tooltip("Tooltip for SaintsDecimal")]
        private SaintsDecimal _saintsDecimal = new SaintsDecimal(12.5m);

        [SerializeField, Tooltip("Tooltip for SaintsRow"), SaintsRow]
        private RowData _saintsRow;

        [SerializeField, Tooltip("Tooltip for SaintsArray"), SaintsArray(numberOfItemsPerPage: 5)]
        private SaintsArray<int> _saintsArray = new SaintsArray<int>(new[] { 1, 2, 3 });

        [SerializeField, Tooltip("Tooltip for SaintsHashSet"), SaintsHashSet(numberOfItemsPerPage: 5)]
        private SaintsHashSet<int> _saintsHashSet = new SaintsHashSet<int> { 1, 2, 3 };

        [SerializeField, Tooltip("Tooltip for SaintsDictionary"), SaintsDictionary(numberOfItemsPerPage: 5)]
        private SaintsDictionary<string, int> _saintsDictionary = new SaintsDictionary<string, int> { { "one", 1 } };

        [SerializeField, Tooltip("Tooltip for SaintsInterface")]
        private SaintsInterface<Component, IIssue412TooltipInterface> _saintsInterface;

#if SAINTSFIELD_UNITY_MATHEMATICS && !SAINTSFIELD_UNITY_MATHEMATICS_DISABLE
        [SerializeField, Tooltip("Tooltip for half")]
        private half _half;

        [SerializeField, Tooltip("Tooltip for half2")]
        private half2 _half2;

        [SerializeField, Tooltip("Tooltip for half3")]
        private half3 _half3;

        [SerializeField, Tooltip("Tooltip for half4")]
        private half4 _half4;
#endif

#if SAINTSFIELD_SERIALIZATION
        [SerializeField, Tooltip("Tooltip for SaintsEvent")]
        private SaintsEvent<int> _saintsEvent;
#endif

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
