using System.Runtime.CompilerServices;
using SaintsField;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace Samples.Scripts.SaintsEditor.Issues.Issue377.Editor
{
    // ReSharper disable once InconsistentNaming
    public class BVCWindow: SaintsEditorWindow
    {
        private SerializedProperty _serProp;

        [OnValueChanged(nameof(ValueChanged))]
        [SaintsRow(inline: true)]  // Optional; You can remove this
        public BVC bvcEditing;

        private void ValueChanged(BVC bvc)
        {
            Debug.Log($"valuec changed here: {bvc}");

            // You can save here (real-time saving)

            // For shadow editing
            // ApplyOriginalChange();

            // For copy editing
            // SaveBack();
        }

        [ShowInInspector]
        // [Button] use `Button` instead if the real-time computing is too heavy
        public float Preview()
        {
            return bvcEditing.number * bvcEditing.percent;
        }

        [Button]
        private void SaveBack()
        {
            // If you're using a copy
            using (SerializedObject window = new SerializedObject(this))
            {
                _serProp.boxedValue = window.FindProperty(nameof(bvcEditing)).boxedValue;
            }

            // either way, you'll always need to apply the save
            ApplyOriginalChange();
        }

        private void ApplyOriginalChange()
        {
            _serProp.serializedObject.ApplyModifiedProperties();
        }

        // ReSharper disable once InconsistentNaming
        public static void OpenNew(BVC originalBVC, SerializedProperty serProp)
        {
            BVCWindow window = CreateWindow<BVCWindow>("BVC");

            window._serProp = serProp;
            Debug.Log(window._serProp.propertyPath);

            // use this if you want to real-time editing(shadow editing), because class is a shared reference
            // window.bvcEditing = originalBVC;

            // use this if you want to use a copy editing, and manually write back
            using(SerializedObject thisSo = new SerializedObject(window))
            {
                SerializedProperty thisEditing = thisSo.FindProperty(nameof(bvcEditing));
                thisEditing.boxedValue = serProp.boxedValue;
                thisSo.ApplyModifiedPropertiesWithoutUndo();
            }

            window.Show();
        }
    }
}
