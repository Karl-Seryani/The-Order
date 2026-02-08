using UnityEngine;

namespace TheOrder.PlayerCamera
{
    /// <summary>
    /// Manual first-person mouse look. Pitch rotates the camera locally,
    /// yaw rotates the player body. No Cinemachine dependency.
    /// </summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Settings")]
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private float _pitchClamp = 85f;

        [Header("References")]
        [SerializeField] private Transform _playerBody;

        #endregion

        #region Private Fields

        private Player.PlayerInputHandler _input;
        private float _pitch;
        private bool _isEnabled = true;

        #endregion

        #region Public API

        /// <summary>Enable or disable camera look. Used during wake-up sequence.</summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_playerBody != null)
            {
                _input = _playerBody.GetComponent<Player.PlayerInputHandler>();
            }
        }

        private void LateUpdate()
        {
            if (_input == null || !_isEnabled) return;

            Vector2 lookInput = _input.LookInput;

            // Yaw — rotate the player body
            float yaw = lookInput.x * _mouseSensitivity;
            _playerBody.Rotate(Vector3.up * yaw);

            // Pitch — rotate the camera locally
            _pitch -= lookInput.y * _mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, -_pitchClamp, _pitchClamp);
            transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        #endregion
    }
}
