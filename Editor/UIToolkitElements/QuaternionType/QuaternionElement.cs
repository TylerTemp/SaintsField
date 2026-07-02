#if UNITY_2021_3_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.QuaternionType
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class QuaternionElement: BindableElement, INotifyValueChanged<Quaternion>
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<QuaternionElement, UxmlTraits> { }
#endif

        private readonly Vector3Field _vector3Field;

        public QuaternionElement()
        {
            hierarchy.Add(_vector3Field = new Vector3Field(null));
        }

        public void SetValueWithoutNotify(Quaternion newValue)
        {
            _vector3Field.SetValueWithoutNotify(newValue.eulerAngles);
        }

        public Quaternion value
        {
            get => Quaternion.Euler(_vector3Field.value);
            set => _vector3Field.value = value.eulerAngles;
        }
    }
}
#endif
