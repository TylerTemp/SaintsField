#if UNITY_2021_3_OR_NEWER

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Units;
using SaintsField.Editor.Drawers.SaintsDecimalType;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AdaptDrawer
{
    public partial class AdaptAttributeDrawer
    {
        protected override bool UseCreateFieldUIToolKit => false;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            UnitState state = GetState((AdaptAttribute)saintsAttribute);
            if (!string.IsNullOrEmpty(state.Error))
            {
                return new HelpBox(state.Error, HelpBoxMessageType.Error)
                {
                    style = { flexGrow = 1 },
                };
            }

            bool hasSupportedFieldDrawer = allAttributes.Any(each =>
                each is PropRangeAttribute || each is MinMaxSliderAttribute) ||
                property.type == nameof(SaintsDecimal) || SaintsDecimalDrawer.IsSerializedActualDecimal(property);
            if (hasSupportedFieldDrawer)
            {
                return null;
            }

            return new HelpBox("Adapt requires PropRange, MinMaxSlider, or SaintsDecimal.",
                HelpBoxMessageType.Error)
            {
                style = { flexGrow = 1 },
            };
        }
    }
}

#endif
