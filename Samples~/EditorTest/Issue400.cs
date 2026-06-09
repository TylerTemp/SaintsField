using System;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.EditorTest
{
    public class Issue400 : SaintsEditorWindow
    {
        [MenuItem("Saints Field/Issue 400")]
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<Issue400>(false, "My Panel");
            window.Show();
        }

        [Serializable]
        public enum Type
        {
            AddToResult,
        }

        [Serializable]
        public enum ApplyType
        {
            None,
            First,
            Second,
        }

        [LayoutStart("H", ELayout.Horizontal)]
        [EnumButtons]
        public ApplyType applyType;

        // [Serializable]
        // public class Effect
        // {
        //     public string name;
        //     public Type type;
        //
        //     [ShowIf(nameof(type), Type.AddToResult)] [EnumButtons]
        //     public ApplyType applyType;
        //
        //     public float value;
        // }
        //
        // [Serializable]
        // public class TabledInfo
        // {
        //     public Effect effect;
        // }
        //
        // [Table] public TabledInfo[] tabledInfo;
    }
}
