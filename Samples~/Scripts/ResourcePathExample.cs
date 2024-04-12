using System;
using SaintsField.Samples.Scripts.RequiredTypeExample;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResourcePathExample : MonoBehaviour
    {
        // resource: display as a MonoScript, requires a BoxCollider
        [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
        [InfoBox(nameof(myResource), true)]
        [RichLabel("<icon=star.png /><label />")]
        public string myResource;

        // AssetDatabase path
        [Space]
        [ResourcePath(EStr.AssetDatabase, typeof(Dummy), typeof(BoxCollider))]
        [InfoBox(nameof(myAssetPath), true)]
        public string myAssetPath;

        // GUID
        [Space]
        [ResourcePath(EStr.Guid, typeof(Dummy), typeof(BoxCollider))]
        [InfoBox(nameof(myGuid), true)]
        public string myGuid;

        // prefab resource
        [ResourcePath(typeof(GameObject))]
        [InfoBox(nameof(resourceNoRequire), true)]
        public string resourceNoRequire;

        // requires to have a Dummy script attached, and has interface IMyInterface
        [ResourcePath(typeof(Dummy), typeof(IMyInterface))]
        [InfoBox(nameof(myInterface), true)]
        public string myInterface;

        [Serializable]
        public struct MyStruct
        {
            [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
            [InfoBox(nameof(myResource), true)]
            [BelowRichLabel(nameof(myResource), true)]
            [OnValueChanged(nameof(OnChange))]
            public string myResource;

            [ResourcePath(typeof(Dummy), typeof(BoxCollider))]
            [OnValueChanged(nameof(OnChange))]
            [InfoBox(nameof(OnLabel), true)]
            [BelowRichLabel(nameof(OnLabel), true)]
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
