using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// First-person character movement — walk and sprint only.
    /// Communicates via GameEvents. No direct references to other systems.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStamina))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 3.0f;
        [SerializeField] private float _sprintSpeed = 5.5f;
        [SerializeField] private float _gravity = -15f;

        #endregion

        #region Private Fields

        private CharacterController _controller;
        private PlayerInputHandler _input;
        private PlayerStamina _stamina;
        private Vector3 _velocity;
        private bool _wasSprinting;

        #endregion

        #region Public API

        /// <summary>True if the player is currently sprinting.</summary>
        public bool IsSprinting => _stamina.IsSprinting;

        /// <summary>Current movement speed.</summary>
        public float CurrentSpeed { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputHandler>();
            _stamina = GetComponent<PlayerStamina>();
        }

        private void Update()
        {
            HandleGravity();
            HandleMovement();
        }

        #endregion

        #region Movement

        private void HandleGravity()
        {
            if (_controller.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            _velocity.y += _gravity * Time.deltaTime;
        }

        private void HandleMovement()
        {
            Vector2 moveInput = _input.MoveInput;
            bool wantsSprint = _input.SprintHeld && moveInput.y > 0f;

            // Update stamina sprint state
            _stamina.SetSprinting(wantsSprint && _stamina.CanSprint);

            // Determine speed
            CurrentSpeed = _stamina.IsSprinting ? _sprintSpeed : _walkSpeed;

            // Calculate movement direction relative to player facing
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            _controller.Move(move * CurrentSpeed * Time.deltaTime);

            // Apply gravity
            _controller.Move(_velocity * Time.deltaTime);

            // Fire events
            float horizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;

            if (horizontalSpeed > 0.1f)
            {
                GameEvents.PlayerMoved(transform.position, horizontalSpeed);
            }

            // Sprint started event
            if (_stamina.IsSprinting && !_wasSprinting)
            {
                GameEvents.PlayerSprinted();
            }
            _wasSprinting = _stamina.IsSprinting;
        }

        #endregion
    }
}
