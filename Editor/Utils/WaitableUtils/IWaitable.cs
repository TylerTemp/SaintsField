namespace SaintsField.Editor.Utils.WaitableUtils
{
    public interface IWaitable
    {
        bool Done { get; }
        float Progress { get; }
        void Update();
    }
}
