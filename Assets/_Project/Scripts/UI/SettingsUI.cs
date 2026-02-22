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

        private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";
        private const float SENSITIVITY_MIN = 0.5f;
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

        #region Public API

        /// <summary>Override back action for use outside main menu (e.g. pause menu).</summary>
        public Action OnBackAction { get; set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
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
            float savedValue = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY, SENSITIVITY_DEFAULT);

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
            PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, value);
            PlayerPrefs.Save();

            // Apply live to camera if in-game
            var cam = FindFirstObjectByType<FirstPersonCamera>();
            if (cam != null)
                cam.SetSensitivity(value);
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
