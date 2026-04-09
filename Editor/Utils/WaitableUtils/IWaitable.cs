namespace SaintsField.Editor.Utils.WaitableUtils
{
    public interface IWaitable
    {
        bool Done { get; }
        void Update();
    }
}
