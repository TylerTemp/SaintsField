using System.Collections.Generic;
using UnityEditor;

namespace SaintsField.Editor.Utils.RuntimeSave
{
    [FilePath("Library/SaintsFieldRuntimeSaver.asset", FilePathAttribute.Location.ProjectFolder)]
    public class RuntimeSaver: ScriptableSingleton<RuntimeSaver>
    {
        public List<PathSaver> pathSavers = new List<PathSaver>();

        public void SaveToDisk()
        {
            Save(true);
        }
    }
}
