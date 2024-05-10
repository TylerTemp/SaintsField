namespace SaintsField.Playa
{
    public interface IPlayaMethodBindAttribute
    {
        MethodBind MethodBind { get; }
        string ButtonTarget { get; }
        object Value { get; }
        bool IsCallback { get; }
    }
}
