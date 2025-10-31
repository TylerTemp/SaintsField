using UnityEditor;

namespace SaintsField.Editor.UIToolkitElements
{
    public interface IScenePickerPayload
    {
        string Name { get; }
        bool IsSceneAsset(SceneAsset sceneAsset);
    }
}
