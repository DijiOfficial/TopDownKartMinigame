using System;

namespace Battlekart
{
    public static class MapElements
    {
        public static event Action OnReset;

        public static bool Active { get; private set; } = true;

        public static void SetActive(bool active)
        {
            Active = active;
            ResetAll();
        }

        public static void ResetAll()
        {
            foreach (PressurePlate plate in PressurePlate.All)
            {
                plate.ResetPlate();
            }

            foreach (RotatingGate gate in RotatingGate.All)
            {
                gate.ResetGate();
            }

            OnReset?.Invoke();
        }
    }
}
