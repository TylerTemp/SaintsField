using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaintsField.Samples.Scripts
{
    public class PrefabChangeTest : SaintsMonoBehaviour
    {
        public string justString;
        [ResizableTextArea] public string text;
        [PropRange(0, 100)] public int propRange;
        [Layer] public string layerString;
        [Layer] public int layerInt;

        [Scene] public int sceneInt;
        [Scene] public string sceneString;
        [Scene(fullPath: true)] public string sceneFullPathString;

        [SortingLayer] public string sortingLayerString;
        [SortingLayer] public int sortingLayerInt;

        [Tag] public string tagString;

        [InputAxis] public string inputAxisString;
#if UNITY_2021_2_OR_NEWER
        [ShaderParam] public string shaderParamString;
        [ShaderParam(ShaderPropertyType.Color)] public int shaderParamInt;

        [ShaderKeyword] public string shaderKeyword;
#endif

        [Rate(0, 5)] public int rate05;
        [Rate(1, 5)] public int rate15;
        [Rate(3, 5)] public int rate35;

        [MinMaxSlider(0, 10)] public Vector2Int minMaxInt;
        [MinMaxSlider(0, 10)] public Vector2 minMaxFloat;

        [ProgressBar(10)] public int progressBarInt;
        [ProgressBar(10)] public float progressBarFloat;


        [AnimatorParam] public string animParamName;
        [AnimatorParam] public int animParamHash;

        // Broken...
        [AnimatorState] public string stateName;
        [AnimatorState] public AnimatorStateBase  stateBase;
        [AnimatorState] public AnimatorState state;

        [CurveRange(EColor.Orange)]
        public AnimationCurve curve1;

        [Serializable]
        public enum EnumT
        {
            First,
            Second,
            Third,
        }

        [EnumToggleButtons] public EnumT enumT;

        [Serializable, Flags]
        public enum EnumF
        {
            None,
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
        }

        [EnumToggleButtons] public EnumF enumF;

        [FlagsDropdown] public EnumF enumFDrop;

        [LeftToggle] public bool leftToggle;

        // broken
        [ResourcePath(typeof(GameObject))] public string resourcePath;

        [FieldType(typeof(SpriteRenderer))] public GameObject go;

        // broken
        [ListDrawerSettings(searchable: true, numberOfItemsPerPage: 3)]
        public string[] myDataArr;

        [AdvancedDropdown] public Vector3 v3;

        [Serializable]
        public struct MyS
        {
            public string s;
            public int i;
        }

        [SaintsRow] public MyS myS;
        [SaintsRow(inline: true)] public MyS mySInline;

        public TypeReference typeReference;

        public SaintsObjInterface<IDummy> dm;

        public SaintsArray<GameObject>[] saintsArray;

        public SaintsDictionary<int, string> saintsDict;

        public SaintsHashSet<int> saintsHashSet;

        [Table] public MyS[] mySTable;
    }
}
