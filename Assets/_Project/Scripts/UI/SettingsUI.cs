using System;
using TheOrder.PlayerCamera;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Settings panel UI. Manages mouse sensitivity slider with PlayerPrefs persistence.
    /// Works in both main menu and pause menu contexts via OnBackAction callback.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        #region Constants

        private const float SENSITIVITY_MIN = 0.1f;
        private const float SENSITIVITY_MAX = 5f;
        private const float SENSITIVITY_DEFAULT = 2f;

        #endregion

        #region Serialized Fields

        [Header("UI References")]
        [SerializeField] private Slider _sensitivitySlider;
        [SerializeField] private Text _sensitivityLabel;
        [SerializeField] private Button _backButton;

        [Header("References")]
        [SerializeField] private MainMenuUI _mainMenuUI;

        #endregion

        #region Private Fields

        private FirstPersonCamera _camera;

        #endregion

        #region Public API

        /// <summary>Override back action for use outside main menu (e.g. pause menu).</summary>
        public Action OnBackAction { get; set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _camera = FindFirstObjectByType<FirstPersonCamera>();

            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.minValue = SENSITIVITY_MIN;
                _sensitivitySlider.maxValue = SENSITIVITY_MAX;
                _sensitivitySlider.wholeNumbers = false;
                _sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            if (_camera == null) _camera = FindFirstObjectByType<FirstPersonCamera>();
            float savedValue = PlayerPrefs.GetFloat("MouseSensitivity", SENSITIVITY_DEFAULT);

            if (_sensitivitySlider != null)
                _sensitivitySlider.value = savedValue;

            UpdateLabel(savedValue);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                OnBackClicked();
        }

        private void OnDestroy()
        {
            if (_sensitivitySlider != null)
                _sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Handlers

        private void OnSensitivityChanged(float value)
        {
            UpdateLabel(value);

            // Apply live to camera if in-game (SetSensitivity handles PlayerPrefs persistence)
            if (_camera != null)
                _camera.SetSensitivity(value);
        }

        private void OnBackClicked()
        {
            if (OnBackAction != null)
                OnBackAction();
            else if (_mainMenuUI != null)
                _mainMenuUI.ShowMainMenu();
        }

        #endregion

        #region Helpers

        private void UpdateLabel(float value)
        {
            if (_sensitivityLabel != null)
                _sensitivityLabel.text = value.ToString("F1");
        }

        #endregion
    }
}
