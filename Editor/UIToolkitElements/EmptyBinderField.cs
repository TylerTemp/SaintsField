using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class EmptyBinderField<T>: BaseField<T>
    {
        public EmptyBinderField(VisualElement visualInput) : base(null, visualInput)
        {
            style.marginLeft = style.marginRight = 0;
        }
    }
}
