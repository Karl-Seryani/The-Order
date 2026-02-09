using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Makes a light flicker randomly between on and off states.
    /// Attach to any GameObject with a Light component.
    /// The light stays enabled in the scene — RuntimeLightManager skips tagged flickering lights.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class FlickeringLight : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Flicker Timing")]
        [SerializeField] private float _minOnTime = 2f;
        [SerializeField] private float _maxOnTime = 8f;
        [SerializeField] private float _minOffTime = 0.05f;
        [SerializeField] private float _maxOffTime = 0.3f;

        [Header("Flicker Burst")]
        [SerializeField] private int _minBurstFlickers = 1;
        [SerializeField] private int _maxBurstFlickers = 4;

        [Header("Intensity")]
        [SerializeField] private float _baseIntensity = 1.5f;
        [SerializeField] private float _flickerIntensityMin = 0.3f;

        #endregion

        #region Private Fields

        private Light _light;
        private float _timer;
        private bool _isOn = true;
        private int _remainingBurstFlickers;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _light = GetComponent<Light>();
            _light.intensity = _baseIntensity;
            _timer = Random.Range(_minOnTime, _maxOnTime);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                if (_isOn)
                {
                    // Start a flicker burst
                    if (_remainingBurstFlickers <= 0)
                    {
                        _remainingBurstFlickers = Random.Range(_minBurstFlickers, _maxBurstFlickers + 1);
                    }

                    // Flicker off
                    _isOn = false;
                    _light.intensity = Random.Range(_flickerIntensityMin, _baseIntensity * 0.5f);
                    _timer = Random.Range(_minOffTime, _maxOffTime);
                    _remainingBurstFlickers--;
                }
                else
                {
                    // Flicker back on
                    _isOn = true;
                    _light.intensity = _baseIntensity;

                    if (_remainingBurstFlickers > 0)
                    {
                        // More flickers in this burst — short on time
                        _timer = Random.Range(0.05f, 0.15f);
                    }
                    else
                    {
                        // Burst done — wait before next flicker
                        _timer = Random.Range(_minOnTime, _maxOnTime);
                    }
                }
            }
        }

        #endregion
    }
}