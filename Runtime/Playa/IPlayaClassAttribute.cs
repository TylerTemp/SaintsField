// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    public interface IPlayaClassAttribute: IPlayaSaintsEditorOnlyAttribute
    {
        bool EndDecorator { get; }
    }
}
