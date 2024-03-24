using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    public class RequireTypeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        // ReSharper disable InconsistentNaming
        public readonly IReadOnlyList<Type> RequiredTypes;
        public readonly EPick EditorPick;
        public readonly bool CustomPicker;
        public readonly bool FreeSign;
        // ReSharper enable InconsistentNaming

        public RequireTypeAttribute(EPick editorPick = EPick.Assets | EPick.Scene,
            bool freeSign = false, bool customPicker = true, params Type[] requiredTypes)
        {
            Debug.Assert(requiredTypes.Length > 0, "You need to specific at least one required type");
            RequiredTypes = requiredTypes;
            EditorPick = editorPick == 0
                ? EPick.Assets | EPick.Scene
                : editorPick;
            CustomPicker = customPicker;
            FreeSign = freeSign;
        }

        public RequireTypeAttribute(bool freeSign = false, bool customPicker = true, params Type[] requiredTypes): this(EPick.Assets | EPick.Scene, freeSign, customPicker, requiredTypes)
        {
        }

        public RequireTypeAttribute(bool freeSign, params Type[] requiredTypes): this(EPick.Assets | EPick.Scene, freeSign, true, requiredTypes)
        {
        }

        public RequireTypeAttribute(EPick editorPick, params Type[] requiredTypes) :this(editorPick, false, true, requiredTypes)
        {
        }

        public RequireTypeAttribute(params Type[] requiredTypes) :this(EPick.Assets | EPick.Scene, false, true, requiredTypes)
        {
        }
    }
}
