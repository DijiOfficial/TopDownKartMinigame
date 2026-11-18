namespace Battlekart
{
    public interface ICarInputReceiver
    {
        void OverrideInputSource(ICarInputSource source);
        void ClearInputOverride();
    }
}
