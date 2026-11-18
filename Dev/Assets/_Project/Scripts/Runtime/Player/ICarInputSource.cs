namespace Battlekart
{
    public interface ICarInputSource
    {
        float Steer { get; }
        bool Accelerate { get; }
        bool Brake { get; }
        bool Handbrake { get; }
    }
}
