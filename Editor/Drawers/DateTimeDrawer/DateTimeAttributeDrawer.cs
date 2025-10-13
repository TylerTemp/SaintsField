#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    [CustomPropertyDrawer(typeof(DateTimeAttribute), true)]
    public class DateTimeAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            DateTimeElement dateTimeElement = new DateTimeElement();
            dateTimeElement.BindPath(property.propertyPath);
            dateTimeElement.Bind(property.serializedObject);

            return new DateTimeField(GetPreferredLabel(property), dateTimeElement);
            // VisualElement root = new VisualElement();
            //
            // VisualElement yearMonth = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = FlexDirection.Row,
            //     },
            // };
            // root.Add(yearMonth);
            //
            // YearInputElement yearInputElement = new YearInputElement
            // {
            //     bindingPath = property.propertyPath,
            // };
            // yearMonth.Add(yearInputElement);
            //
            // MonthInputElement monthInputElement = new MonthInputElement
            // {
            //     bindingPath = property.propertyPath,
            // };
            // yearMonth.Add(monthInputElement);
            //
            // NextMonthButtonElement preMonthButtonElement = new NextMonthButtonElement(true)
            // {
            //     bindingPath = property.propertyPath,
            // };
            // yearMonth.Add(preMonthButtonElement);
            // NextMonthButtonElement nextMonthButtonElement = new NextMonthButtonElement
            // {
            //     bindingPath = property.propertyPath,
            // };
            // yearMonth.Add(nextMonthButtonElement);
            //
            // DateTimeYearPanel yearPanel = new DateTimeYearPanel
            // {
            //     bindingPath = property.propertyPath,
            //     style =
            //     {
            //         height = 120,
            //     },
            // };
            // root.Add(yearPanel);
            //
            // DateTimeDayPanel dayPanel = new DateTimeDayPanel
            // {
            //     bindingPath = property.propertyPath,
            // };
            // root.Add(dayPanel);
            //
            // return root;
        }
    }
}
#endif
