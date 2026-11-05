namespace SaintsField.Playa
{
    /// <summary>
    /// Downpour system will only let SaintsEditorCore to consume it, without down-passing
    /// Typical like InfoBox will only be rendered once thus only one consumer when down-passing
    /// </summary>
    public interface IPlayaSaintsEditorOnlyAttribute: IPlayaAttribute
    {

    }
}
