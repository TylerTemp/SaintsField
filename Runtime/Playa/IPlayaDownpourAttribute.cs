namespace SaintsField.Playa
{
    /// <summary>
    /// Downpour system will make it down-pass to its element render, while also let SaintsEditorCore to consume it
    /// Typical like OnValueChanged will watch both array & single field
    /// </summary>
    public interface IPlayaDownpourAttribute: IPlayaAttribute
    {

    }
}
