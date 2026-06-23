using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.OnValueChangedCollectionFakeRenderer
{
    public partial class OnValueChangedCollectionRenderer
    {
        private string _errorIMGUI = "";

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return 0f;
            }

            (bool changed, string error) = CheckCollectionLengthChanged();
            if (changed)
            {
                _errorIMGUI = error;
            }

            return _errorIMGUI == ""
                ? 0f
                : ImGuiHelpBox.GetHeight(_errorIMGUI, width, MessageType.Error);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown || _errorIMGUI == "")
            {
                return;
            }

            ImGuiHelpBox.Draw(position, _errorIMGUI, MessageType.Error);
        }
    }
}
