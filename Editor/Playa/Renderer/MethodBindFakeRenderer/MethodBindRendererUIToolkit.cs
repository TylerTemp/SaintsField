#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Core;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.MethodBindFakeRenderer
{
    public partial class MethodBindRenderer
    {
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            CheckMethodBind(_methodBindAttribute, FieldWithInfo);

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(OnApplicationChanged);

            return (null, false);
        }
    }
}
#endif
