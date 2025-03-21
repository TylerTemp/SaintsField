#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        private PropertyField _result;

        private VisualElement _fieldElement;
        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            VisualElement result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = FieldWithInfo.SerializedProperty.propertyPath,
            };
            return (result, false);

            #region Don't Delete
            // On Hold, cuz SaintsPropertyDraw does not support copy/paste yet. Should fix that first
            // if(ReflectCache.GetCustomAttributes<ISaintsAttribute>(FieldWithInfo.FieldInfo).Any())
            // {
            //     VisualElement result = new PropertyField(FieldWithInfo.SerializedProperty)
            //     {
            //         style =
            //         {
            //             flexGrow = 1,
            //         },
            //         name = FieldWithInfo.SerializedProperty.propertyPath,
            //     };
            //     return (result, false);
            // }
            //
            // SaintsPropertyDrawer saintsPropertyDrawer = (SaintsPropertyDrawer) SaintsPropertyDrawer.MakePropertyDrawer(typeof(SaintsPropertyDrawer), FieldWithInfo.FieldInfo, null, FieldWithInfo.SerializedProperty.displayName);
            // return (saintsPropertyDrawer.CreatePropertyGUI(FieldWithInfo.SerializedProperty), false);
            #endregion
        }
    }
}
#endif
