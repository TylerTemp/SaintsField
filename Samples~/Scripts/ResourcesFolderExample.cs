using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ResourcesFolderExample : MonoBehaviour
    {
        [ResourceFolder] public string resourcesFolder;
        [ResourceFolder] public string[] resourcesFolders;
    }
}
