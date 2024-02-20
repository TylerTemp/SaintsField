using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaintsField
{
    public class GetComponentByPathAttribute: PropertyAttribute
    {

        // ReSharper disable once InconsistentNaming
        public readonly IReadOnlyList<string> Paths;

        public GetComponentByPathAttribute(string path, params string[] paths)
        {
            Paths = paths.Prepend(path).ToArray();
        }
    }
}
