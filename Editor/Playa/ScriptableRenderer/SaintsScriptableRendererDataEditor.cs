using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.ScriptableRenderer;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
    [CustomEditor(typeof(SaintsScriptableRendererData), true)]
    public class SaintsScriptableRendererDataEditor:
        ScriptableRendererDataEditor
        // SaintsEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new ScriptableRendererDataCore(this).CreateInspectorGUI();
        }
    }
}
