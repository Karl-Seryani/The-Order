using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Applies horror fonts and color palette to all Text components in children.
    /// Attach to every Canvas root. Runs once in Awake before any UI is shown.
    /// Title font (Nosifer) applied to text marked with "Title" tag or fontSize >= 28.
    /// Body font (Creepster) applied to everything else.
    /// </summary>
    public class HorrorFontApplier : MonoBehaviour
    {
        [Header("Fonts")]
        [SerializeField] private Font _titleFont;
        [SerializeField] private Font _bodyFont;

        [Header("Font Size Multiplier")]
        [SerializeField] private float _fontSizeMultiplier = 1.0f;

        [Header("Color Override")]
        [SerializeField] private bool _overrideColors;
        [SerializeField] private Color _titleColor = new Color(0.85f, 0.12f, 0.1f, 1f);
        [SerializeField] private Color _bodyColor = new Color(0.88f, 0.84f, 0.78f, 1f);

        [Header("Title Detection")]
        [SerializeField] private int _titleFontSizeThreshold = 28;

        private void Awake()
        {
            ApplyFonts();
        }

        private void ApplyFonts()
        {
            var texts = GetComponentsInChildren<Text>(true);

            foreach (var text in texts)
            {
                bool isTitle = text.fontSize >= _titleFontSizeThreshold;

                if (isTitle && _titleFont != null)
                {
                    text.font = _titleFont;
                    if (_overrideColors) text.color = _titleColor;
                }
                else if (_bodyFont != null)
                {
                    text.font = _bodyFont;
                    if (_overrideColors) text.color = _bodyColor;
                }

                if (_fontSizeMultiplier > 0f && Mathf.Abs(_fontSizeMultiplier - 1f) > 0.01f)
                {
                    text.fontSize = Mathf.RoundToInt(text.fontSize * _fontSizeMultiplier);
                }
            }
        }
    }
}
