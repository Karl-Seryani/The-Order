using UnityEngine;
using UnityEngine.InputSystem;

namespace TheOrder.Player
{
    /// <summary>
    /// Caches input values from the New Input System for other player scripts to read.
    /// Disables player input when the game is paused; re-enables on Playing.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        #region Cached Input Values

        /// <summary>Current movement input (WASD / left stick).</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Current look input (mouse delta / right stick).</summary>
        public Vector2 LookInput { get; private set; }

        /// <summary>True while the sprint button is held.</summary>
        public bool SprintHeld { get; private set; }

        /// <summary>True on the frame the interact button is pressed.</summary>
        public bool InteractPressed { get; private set; }

        /// <summary>True on the frame the flashlight button is pressed.</summary>
        public bool FlashlightPressed { get; private set; }

        /// <summary>True on the frame the pause button is pressed.</summary>
        public bool PausePressed { get; private set; }

        /// <summary>True on the frame the journal button is pressed.</summary>
        public bool JournalPressed { get; private set; }

        /// <summary>True on the frame the drop button is pressed.</summary>
        public bool DropPressed { get; private set; }

        /// <summary>True on the frame the crouch button is pressed (toggle).</summary>
        public bool CrouchPressed { get; private set; }

        #endregion

        #region Private Fields

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _sprintAction;
        private InputAction _interactAction;
        private InputAction _flashlightAction;
        private InputAction _pauseAction;
        private InputAction _journalAction;
        private InputAction _dropAction;
        private InputAction _crouchAction;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();

            var playerMap = _playerInput.actions.FindActionMap("Player");
            _moveAction = playerMap.FindAction("Move");
            _lookAction = playerMap.FindAction("Look");
            _sprintAction = playerMap.FindAction("Sprint");
            _interactAction = playerMap.FindAction("Interact");
            _flashlightAction = playerMap.FindAction("Flashlight");
            _pauseAction = playerMap.FindAction("Pause");
            _journalAction = playerMap.FindAction("Journal");
            _dropAction = playerMap.FindAction("Drop");
            _crouchAction = playerMap.FindAction("Crouch");
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnWakeUpStarted += HandleWakeUpStarted;
            GameEvents.OnWakeUpCompleted += HandleWakeUpCompleted;
            GameEvents.OnDeathCinematicStarted += HandleDeathCinematicStarted;
            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnWakeUpStarted -= HandleWakeUpStarted;
            GameEvents.OnWakeUpCompleted -= HandleWakeUpCompleted;
            GameEvents.OnDeathCinematicStarted -= HandleDeathCinematicStarted;
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();
            SprintHeld = _sprintAction.IsPressed();
            InteractPressed = _interactAction.WasPressedThisFrame();
            FlashlightPressed = _flashlightAction.WasPressedThisFrame();
            PausePressed = _pauseAction.WasPressedThisFrame();
            JournalPressed = _journalAction.WasPressedThisFrame();
            DropPressed = _dropAction != null && _dropAction.WasPressedThisFrame();
            CrouchPressed = _crouchAction != null && _crouchAction.WasPressedThisFrame();
        }

        #endregion

        #region Event Handlers

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    _playerInput.actions.FindActionMap("Player").Enable();
                    break;
                case GameState.Paused:
                    _playerInput.actions.FindActionMap("Player").Disable();
                    // Keep pause and journal actions available while paused
                    _pauseAction.Enable();
                    _journalAction.Enable();
                    break;
                case GameState.Death:
                case GameState.MainMenu:
                case GameState.Ending:
                    _playerInput.actions.FindActionMap("Player").Disable();
                    break;
            }
        }

        private void HandleWakeUpStarted()
        {
            _playerInput.actions.FindActionMap("Player").Disable();
        }

        private void HandleWakeUpCompleted()
        {
            _playerInput.actions.FindActionMap("Player").Enable();
        }

        private void HandleDeathCinematicStarted()
        {
            _playerInput.actions.FindActionMap("Player").Disable();
        }

        #endregion
    }
}
