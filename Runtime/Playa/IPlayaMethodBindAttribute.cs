namespace SaintsField.Playa
{
    public interface IPlayaMethodBindAttribute
    {
        MethodBind MethodBind { get; }
        string EventTarget { get; }
        object Value { get; }
        bool IsCallback { get; }
    }
}
