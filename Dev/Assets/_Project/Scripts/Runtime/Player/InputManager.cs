using System;
using BattleKart.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlekart
{
    public class InputManager : SingletonBehaviour<InputManager>
    {
        [SerializeField] private eInputMode _defaultInputMode = eInputMode.None;

        public enum eInputMode
        {
            None = 0,
            Player = 1,
            UI = 2
        }

        public InputValue<Vector2> MoveAxis { get; private set; }
        public InputValue<bool> Accelerate { get; private set; }
        public InputValue<bool> Brake { get; private set; }
        public InputValue<bool> Handbrake { get; private set; }

        private Action _disposeInputValues;

        public PlayerInputActions PlayerActions { get; private set; }
        private eInputMode _activeInputMode = eInputMode.None;
        public eInputMode ActiveInputMode
        {
            get => _activeInputMode;
            set
            {
                if(_activeInputMode == value)
                    return;

                _activeInputMode = value;
                switch(_activeInputMode)
                {
                    case eInputMode.None:
                        PlayerActions.Player.Disable();
                        // PlayerActions.UI.Disable();
                        break;
                    case eInputMode.Player:
                        // PlayerActions.UI.Disable();
                        PlayerActions.Player.Enable();

                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        break;
                    case eInputMode.UI:
                        PlayerActions.Player.Disable();
                        // PlayerActions.UI.Enable();

                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        break;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            PlayerActions = new PlayerInputActions();

            // Can we use reflection to automatically fill this up?
            MoveAxis = new InputValue<Vector2>(PlayerActions.Player.Move, _disposeInputValues);
            Accelerate = new InputValue<bool>(PlayerActions.Player.Accelerate, _disposeInputValues);
            Brake = new InputValue<bool>(PlayerActions.Player.BrakeReverse, _disposeInputValues);
            Handbrake = new InputValue<bool>(PlayerActions.Player.Handbrake, _disposeInputValues);

            _activeInputMode = _defaultInputMode;
        }

        private void OnDestroy()
        {
            _disposeInputValues?.Invoke();
            PlayerActions.Dispose();
        }

        private void OnEnable()
        {
            PlayerActions.Enable();
        }

        private void OnDisable()
        {
            PlayerActions.Disable();
        }
    }

    public sealed class InputValue<T> where T : struct
    {
        private readonly InputAction _inputAction;

        public T Value { get; private set; }

        public InputValue()
        {
            Value = default;
        }

        public InputValue(InputAction inputAction, Action disposeAction)
        {
            _inputAction = inputAction;
            disposeAction += Dispose;
            _inputAction.performed += UpdateValue;
            _inputAction.canceled += UpdateValue;

            void Dispose()
            {
                disposeAction -= Dispose;
                _inputAction.performed -= UpdateValue;
                _inputAction.canceled -= UpdateValue;
            }
        }

        private void UpdateValue(InputAction.CallbackContext context)
        {
            if(typeof(T) == typeof(bool))
                Value = (T)(object)context.ReadValueAsButton();
            else
                Value = context.ReadValue<T>();
        }

        public T ConsumeValue()
        {
            T temp = Value;
            Value = default;
            return temp;
        }

        public void Disable()
        {
            if(_inputAction.enabled == false)
                return;

            Value = default;
            _inputAction.Disable();
            _inputAction.performed -= UpdateValue;
            _inputAction.canceled -= UpdateValue;
        }

        public void Enable()
        {
            if(_inputAction.enabled)
                return;

            _inputAction.Enable();
            _inputAction.performed += UpdateValue;
            _inputAction.canceled += UpdateValue;
        }
    }
}
