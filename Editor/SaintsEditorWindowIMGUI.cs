using System;
using SaintsField.Editor.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    public partial class SaintsEditorWindow
    {
        [NonSerialized] private SaintsEditorWindowSpecialEditor _saintsEditorWindowSpecialEditor;

        // public void OnGUI () {
        //     // // The actual window code goes here
        //     // if(_saintsEditorWindowSpecialEditor == null)
        //     // {
        //     //     Debug.Log("Create Editor for IMGUI");
        //     //     _saintsEditorWindowSpecialEditor = (SaintsEditorWindowSpecialEditor)UnityEditor.Editor.CreateEditor(this,
        //     //         typeof(SaintsEditorWindowSpecialEditor));
        //     //     // _saintsEditorWindowSpecialEditor.RequiresConstantRepaint();
        //     // }
        //     // _saintsEditorWindowSpecialEditor.OnInspectorGUI();
        //     // OnEditorUpdateInternal();
        //     // EditorApplication.delayCall += Repaint;
        // }
    }
}
