using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlekart
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerCarInput : MonoBehaviour, ICarInputSource
    {
        private InputAction _move;
        private InputAction _accelerate;
        private InputAction _brake;
        private InputAction _handbrake;

        public float Steer => _move is not null ? _move.ReadValue<Vector2>().x : 0.0f;
        public bool Accelerate => _accelerate is not null && _accelerate.IsPressed();
        public bool Brake => _brake is not null && _brake.IsPressed();
        public bool Handbrake => _handbrake is not null && _handbrake.IsPressed();

        private void Start()
        {
            InputActionMap map = GetComponent<PlayerInput>().actions.FindActionMap("Player", true);
            _move = map.FindAction("Move", true);
            _accelerate = map.FindAction("Accelerate", true);
            _brake = map.FindAction("Brake/Reverse", true);
            _handbrake = map.FindAction("Handbrake", true);
        }
    }
}
