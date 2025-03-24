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
            PropertyField result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = FieldWithInfo.SerializedProperty.propertyPath,
            };
            result.Bind(FieldWithInfo.SerializedProperty.serializedObject);
            return (result, false);

            #region Don't Delete

            // About letting SaintsPropertyDrawer fallback:
            // SaintsPropertyDrawer relys on PropertyField to fallback. Directly hi-jiacking the drawer with SaintsPropertyDrawer
            // the workflow will still get into the PropertyField flow, then SaintsField will fail to decide when the
            // fallback should stop.

            // ISaintsAttribute saintsAttr = ReflectCache.GetCustomAttributes<ISaintsAttribute>(FieldWithInfo.FieldInfo)
            //     .FirstOrDefault();
            // Type drawerType = saintsAttr != null
            //     ? SaintsPropertyDrawer.GetSaintsDrawerTypeByAttr(saintsAttr)
            //     : typeof(SaintsPropertyDrawer);
            // SaintsPropertyDrawer saintsPropertyDrawer = (SaintsPropertyDrawer) SaintsPropertyDrawer.MakePropertyDrawer(drawerType, FieldWithInfo.FieldInfo, (Attribute)saintsAttr, FieldWithInfo.SerializedProperty.displayName);
            // Debug.Log(saintsPropertyDrawer);
            // return (saintsPropertyDrawer.CreatePropertyGUI(FieldWithInfo.SerializedProperty), false);

            #endregion
        }
    }
}
#endif
