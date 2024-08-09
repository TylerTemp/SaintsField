using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class PR69: MonoBehaviour
    {
        [ResourcePath(EStr.AssetDatabase, compType: typeof(AudioClip))] public string clipResourcePath;
        [ResourcePath(EStr.AssetDatabase, compType: typeof(Texture))] public string texture;
        [ResourcePath(EStr.AssetDatabase, compType: typeof(Sprite))] public string sprite;
        [ResourcePath(EStr.AssetDatabase, compType: typeof(Material))] public string material;
        [ResourcePath(EStr.AssetDatabase, compType: typeof(Mesh))] public string mesh;
        [ResourcePath(EStr.AssetDatabase, compType: typeof(Motion))] public string motion;
    }
}
