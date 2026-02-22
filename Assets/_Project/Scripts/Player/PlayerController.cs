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

        [Header("Crouch")]
        [SerializeField] private float _crouchSpeed = 1.5f;
        [SerializeField] private float _crouchHeight = 1.0f;
        [SerializeField] private float _crouchTransitionSpeed = 8f;

        #endregion

        #region Private Fields

        private CharacterController _controller;
        private PlayerInputHandler _input;
        private PlayerStamina _stamina;
        private Transform _cameraTransform;
        private Vector3 _velocity;
        private float _standHeight;
        private float _standCameraY;
        private float _targetHeight;

        #endregion

        #region Public API

        /// <summary>True if the player is currently sprinting.</summary>
        public bool IsSprinting => _stamina.IsSprinting;

        /// <summary>True if the player is currently crouching.</summary>
        public bool IsCrouching { get; private set; }

        /// <summary>Current movement speed.</summary>
        public float CurrentSpeed { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputHandler>();
            _stamina = GetComponent<PlayerStamina>();
            _cameraTransform = GetComponentInChildren<UnityEngine.Camera>()?.transform;

            _standHeight = _controller.height;
            _standCameraY = _cameraTransform != null ? _cameraTransform.localPosition.y : 0f;
            _targetHeight = _standHeight;
        }

        private void Update()
        {
            if (!_controller.enabled) return;

            HandleCrouch();
            HandleGravity();
            HandleMovement();
        }

        #endregion

        #region Crouch

        private void HandleCrouch()
        {
            if (_input.CrouchPressed)
                IsCrouching = !IsCrouching;

            _targetHeight = IsCrouching ? _crouchHeight : _standHeight;

            float currentHeight = _controller.height;
            if (!Mathf.Approximately(currentHeight, _targetHeight))
            {
                float newHeight = Mathf.MoveTowards(currentHeight, _targetHeight, _crouchTransitionSpeed * Time.deltaTime);
                _controller.height = newHeight;
                _controller.center = new Vector3(0f, _controller.height / 2f, 0f);

                // Adjust camera position to match
                if (_cameraTransform != null)
                {
                    float ratio = newHeight / _standHeight;
                    Vector3 camPos = _cameraTransform.localPosition;
                    camPos.y = _standCameraY * ratio;
                    _cameraTransform.localPosition = camPos;
                }
            }
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

            // Can't sprint while crouching
            _stamina.SetSprinting(wantsSprint && _stamina.CanSprint && !IsCrouching);

            // Determine speed
            if (IsCrouching)
                CurrentSpeed = _crouchSpeed;
            else
                CurrentSpeed = _stamina.IsSprinting ? _sprintSpeed : _walkSpeed;

            // Calculate movement direction relative to player facing
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            _controller.Move(move * CurrentSpeed * Time.deltaTime);

            // Apply gravity
            _controller.Move(_velocity * Time.deltaTime);

            // Fire events — always send position so Hunter can detect stationary players
            // Use intended speed when actively moving, actual velocity when idle
            // CharacterController.velocity can underreport due to collisions/slopes
            float horizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            float reportedSpeed = (moveInput.sqrMagnitude > 0.01f) ? CurrentSpeed : horizontalSpeed;
            GameEvents.PlayerMoved(transform.position, reportedSpeed);
            GameEvents.PlayerFacingChanged(transform.forward);

        }

        #endregion
    }
}
