using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.UnitDrawer;
using SaintsField.Editor.Drawers.SaintsDecimalType;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AdaptDrawer
{
    public partial class AdaptAttributeDrawer
    {
        protected override string GetImGuiError(SerializedProperty property, UnitAttribute unitAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info)
        {
            string stateError = GetState(unitAttribute).Error;
            if (!string.IsNullOrEmpty(stateError))
            {
                return stateError;
            }

            return allAttributes.Any(each => each is PropRangeAttribute || each is MinMaxSliderAttribute) ||
                   property.type == nameof(SaintsDecimal) || SaintsDecimalDrawer.IsSerializedActualDecimal(property)
                ? ""
                : "Adapt requires PropRange, MinMaxSlider, or SaintsDecimal.";
        }
    }
}
