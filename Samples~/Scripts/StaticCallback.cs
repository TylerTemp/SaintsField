#if UNITY_EDITOR
using UnityEditor;
#endif
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class StaticCallback : SaintsMonoBehaviour
    {
        private static readonly string StaticString = "This is a static string";
        private const string ConstString = "This is a constant string";

        // using full type name
        [AboveRichLabel("$:SaintsField.Samples.Scripts." + nameof(StaticCallback) + "." + nameof(StaticString))]
        // using only type name. This is slow and might find the incorrect target.
        // We'll first search the assembly of this object. If not found, then search all assemblies.
        [InfoBox("$:" + nameof(StaticCallback) + "." + nameof(ConstString))]
        public int field;

#if UNITY_EDITOR
        private static Texture2D ImageCallback(string name) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"Assets/SaintsField/Editor/Editor Default Resources/SaintsField/{name}.png");
#endif

#if UNITY_EDITOR
        // use only field/method name. This will only try to search on the current target.
        [BelowImage("$:" + nameof(ImageCallback), maxWidth: 20)]
#endif
        public string imgName;

        [ShowInInspector] private static bool _disableMe = true;

#if UNITY_EDITOR
        [DisableIf("$:" + nameof(_disableMe))]
        [RequiredIf("$:" + nameof(_disableMe), false)]
#endif
        public string disableIf;
    }
}
