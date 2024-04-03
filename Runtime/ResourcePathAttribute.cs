using System;
using System.Diagnostics;
using System.Linq;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class ResourcePathAttribute: RequireTypeAttribute
    {
        public override SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public override string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly EStr EStr;
        // ReSharper enable InconsistentNaming

        public Type CompType => RequiredTypes[0];

        public ResourcePathAttribute(EStr eStr, bool freeSign, bool customPicker, Type compType, params Type[] requiredTypes)
            : base(EPick.Assets, freeSign, customPicker, requiredTypes.Prepend(compType).ToArray())
        {
            EStr = eStr;
        }

        public ResourcePathAttribute(bool freeSign, bool customPicker, Type compType, params Type[] requiredTypes)
            : this(EStr.Resource, freeSign, customPicker, compType, requiredTypes)
        {
        }

        public ResourcePathAttribute(bool freeSign, Type compType, params Type[] requiredTypes)
            : this(EStr.Resource, freeSign, true, compType, requiredTypes)
        {
        }

        public ResourcePathAttribute(EStr eStr, bool freeSign, Type compType, params Type[] requiredTypes)
            : this(eStr, freeSign, true, compType, requiredTypes)
        {
        }

        public ResourcePathAttribute(EStr eStr, Type compType, params Type[] requiredTypes)
            : this(eStr, false, true, compType, requiredTypes)
        {
        }

        public ResourcePathAttribute(Type compType, params Type[] requiredTypes)
            : this(EStr.Resource, false, true, compType, requiredTypes)
        {
        }
    }
}
