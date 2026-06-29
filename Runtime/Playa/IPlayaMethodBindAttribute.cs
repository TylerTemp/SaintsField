using System;

namespace SaintsField.Playa
{
    public interface IPlayaMethodBindAttribute
    {
        MethodBind MethodBind { get; }
        string EventTarget { get; }
        Type ComponentTypeOrNull { get; }
        string ComponentEventName { get; }

        object Value { get; }
        bool IsCallback { get; }
    }
}
