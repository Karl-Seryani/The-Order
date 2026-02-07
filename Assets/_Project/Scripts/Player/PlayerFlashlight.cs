using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Toggles a spotlight on/off with F key.
    /// Fires GameEvents.FlashlightToggled so the Hunter can react.
    /// Flashlight being on massively increases player visibility to the Hunter.
    /// </summary>
    public class PlayerFlashlight : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Light _spotLight;

        #endregion

        #region Private Fields

        private PlayerInputHandler _input;
        private bool _isOn;

        #endregion

        #region Public API

        /// <summary>True if the flashlight is currently on.</summary>
        public bool IsOn => _isOn;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();

            if (_spotLight == null)
            {
                _spotLight = GetComponentInChildren<Light>();
            }
        }

        private void Start()
        {
            // Flashlight starts off
            _isOn = false;
            if (_spotLight != null)
            {
                _spotLight.enabled = false;
            }
        }

        private void Update()
        {
            if (_input.FlashlightPressed)
            {
                Toggle();
            }
        }

        #endregion

        #region Flashlight Control

        private void Toggle()
        {
            _isOn = !_isOn;

            if (_spotLight != null)
            {
                _spotLight.enabled = _isOn;
            }

            GameEvents.FlashlightToggled(_isOn);
        }

        #endregion
    }
}
