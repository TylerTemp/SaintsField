using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResourcesFolderExample : MonoBehaviour
    {
        [ResourcesFolder] public string resourcesFolder;
        [ResourcesFolder] public string[] resourcesFolders;
    }
}
