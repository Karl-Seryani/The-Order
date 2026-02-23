using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Oscillates text alpha between min and max with random speed changes
    /// and occasional instant-drop glitches. Uses unscaled time for menu context.
    /// Only modifies alpha — preserves RGB set by scene/HorrorFontApplier.
    /// </summary>
    public class FlickeringText : MonoBehaviour
    {
        [Header("Flicker Settings")]
        [SerializeField] private float _minAlpha = 0.3f;
        [SerializeField] private float _maxAlpha = 1.0f;
        [SerializeField] private float _minSpeed = 4f;
        [SerializeField] private float _maxSpeed = 15f;
        [SerializeField] private float _glitchChance = 0.02f;

        private Text _text;
        private float _flickerSpeed;
        private float _phase;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _flickerSpeed = Random.Range(_minSpeed, _maxSpeed);
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (_text == null) return;

            _phase += Time.unscaledDeltaTime * _flickerSpeed;

            // Occasionally change speed for irregular feel
            if (Random.value < 0.01f)
                _flickerSpeed = Random.Range(_minSpeed, _maxSpeed);

            float alpha;

            // Random instant-drop glitch
            if (Random.value < _glitchChance)
            {
                alpha = _minAlpha;
            }
            else
            {
                float t = (Mathf.Sin(_phase) + 1f) * 0.5f;
                alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);
            }

            Color c = _text.color;
            c.a = alpha;
            _text.color = c;
        }
    }
}
