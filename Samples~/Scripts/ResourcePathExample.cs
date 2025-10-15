using System;
using SaintsField.Samples.Scripts.RequiredTypeExample;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResourcePathExample : MonoBehaviour
    {
        // resource: display as a MonoScript, requires a BoxCollider
        [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
        [FieldLabelText("<icon=star.png /><label />")]
        [FieldBelowText("<color=gray>resource: <field />")]
        public string myResource;

        // AssetDatabase path
        [Space]
        [ResourcePath(EStr.AssetDatabase, typeof(Dummy), typeof(BoxCollider))]
        [FieldBelowText("<color=gray>assets: <field />")]
        public string myAssetPath;

        // GUID
        [Space]
        [ResourcePath(EStr.Guid, typeof(Dummy), typeof(BoxCollider))]
        [FieldBelowText("<color=gray>guid: <field />")]
        public string myGuid;

        // prefab resource
        [ResourcePath(typeof(GameObject))]
        [FieldBelowText("<color=gray>resource: <field />")]
        public string resourceNoRequire;

        // requires to have a Dummy script attached, and has interface IMyInterface
        [ResourcePath(typeof(Dummy), typeof(IMyInterface))]
        [FieldBelowText("<color=gray>interface: <field />")]
        public string myInterface;

        [Serializable]
        public struct MyStruct
        {
            [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
            [FieldInfoBox(nameof(myResource), true)]
            [FieldBelowText(nameof(myResource), true)]
            [OnValueChanged(nameof(OnChange))]
            public string myResource;

            [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
            [OnValueChanged(nameof(OnChange))]
            [FieldInfoBox(nameof(OnLabel), true)]
            [FieldBelowText(nameof(OnLabel), true)]
            public string[] myResources;

            [ReadOnly]
            [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
            public string myResourceDisabled;

            public string OnLabel(int index)
            {
                return myResources[index];
            }

            public void OnChange(string value, int index=-1) => Debug.Log($"Value: {value}, Index: {index}");
        }

        public MyStruct myStruct;
        public MyStruct[] myStructs;
    }
}
