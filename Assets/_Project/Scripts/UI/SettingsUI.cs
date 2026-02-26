using System;
using TheOrder.PlayerCamera;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Settings panel UI for the pause menu. Manages mouse sensitivity slider with PlayerPrefs persistence.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        #region Constants

        private const float SENSITIVITY_MIN = 0.1f;
        private const float SENSITIVITY_MAX = 5f;
        private const float SENSITIVITY_DEFAULT = 2f;
        private const int TITLE_FONT_SIZE = 72;
        private const int BODY_FONT_SIZE = 48;

        #endregion

        #region Serialized Fields

        [Header("UI References")]
        [SerializeField] private Slider _sensitivitySlider;
        [SerializeField] private Text _sensitivityLabel;
        [SerializeField] private Button _backButton;

        #endregion

        #region Private Fields

        private static readonly Color TITLE_COLOR = new Color(0.85f, 0.12f, 0.1f, 1f);

        private FirstPersonCamera _camera;
        private bool _colorsApplied;

        #endregion

        #region Public API

        /// <summary>Callback invoked when back button is clicked.</summary>
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

            if (!_colorsApplied) ApplySettingsStyle();
        }

        private void ApplySettingsStyle()
        {
            _colorsApplied = true;

            // Force consistent sizes + colors regardless of HorrorFontApplier multiplier
            var parent = transform;
            if (parent.childCount > 0)
            {
                var titleText = parent.GetChild(0).GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.color = TITLE_COLOR;
                    titleText.fontSize = TITLE_FONT_SIZE;
                }
            }

            for (int i = 1; i < parent.childCount; i++)
            {
                foreach (var text in parent.GetChild(i).GetComponentsInChildren<Text>(true))
                {
                    text.color = Color.white;
                    text.fontSize = BODY_FONT_SIZE;
                }
            }

            if (_backButton != null)
            {
                var img = _backButton.GetComponent<Image>();
                if (img != null) img.color = new Color(1f, 1f, 1f, 0f);
            }
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
            OnBackAction?.Invoke();
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
