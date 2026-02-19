using System.Collections;
using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// Press E to toggle-slide furniture (drawers, cabinets, shelves).
    /// Smoothly slides open/closed along a configurable local axis.
    /// Attach to any furniture mesh with a collider.
    /// </summary>
    public class SlidableFurniture : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Slide Settings")]
        [SerializeField] private Vector3 _slideDirection = Vector3.back;
        [SerializeField] private float _slideDistance = 0.35f;
        [SerializeField] private float _slideDuration = 0.5f;

        [Header("Lock")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private string _lockedPrompt = "Locked";

        [Header("Audio")]
        [SerializeField] private AudioClip _slideSound;
        [SerializeField] [Range(0f, 1f)] private float _soundVolume = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float _noiseLoudness = 0.6f;

        [Header("Prompt")]
        [SerializeField] private string _promptText = "Slide";

        #endregion

        #region Private Fields

        private float _currentOffset;
        private bool _isOpen;
        private bool _isAnimating;
        private Vector3 _closedPosition;
        private Coroutine _animationCoroutine;
        private Rigidbody _rigidbody;

        #endregion

        #region Public API

        /// <summary>Current open fraction from 0 (closed) to 1 (fully open).</summary>
        public float OpenFraction => _slideDistance > 0f ? Mathf.Clamp01(_currentOffset / _slideDistance) : 0f;

        /// <summary>True if more than half open.</summary>
        public bool IsOpen => _isOpen;

        /// <summary>Lock or unlock this furniture from external scripts.</summary>
        public bool IsLocked { get => _isLocked; set => _isLocked = value; }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _closedPosition = transform.localPosition;
            _rigidbody = GetComponent<Rigidbody>();

            // Ensure any Rigidbody doesn't fight with transform-driven animation
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }
        }

        #endregion

        #region IInteractable

        /// <summary>Toggle slide open/close on E press.</summary>
        public void Interact(GameObject interactor)
        {
            if (_isLocked || _isAnimating) return;

            float targetOffset = _isOpen ? 0f : _slideDistance;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateSlide(targetOffset));
            if (_slideSound != null)
                AudioSource.PlayClipAtPoint(_slideSound, transform.position, _soundVolume);
            GameEvents.InteractableNoise(transform.position, _noiseLoudness);
        }

        /// <summary>Returns prompt text.</summary>
        public string GetPromptText()
        {
            if (_isLocked) return _lockedPrompt;
            return _promptText;
        }

        #endregion

        #region Animation

        private IEnumerator AnimateSlide(float targetOffset)
        {
            _isAnimating = true;
            float startOffset = _currentOffset;
            float elapsed = 0f;
            float duration = Mathf.Max(_slideDuration, 0.01f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth ease-out curve
                t = 1f - (1f - t) * (1f - t);
                _currentOffset = Mathf.Lerp(startOffset, targetOffset, t);
                ApplyPosition();
                yield return null;
            }

            _currentOffset = targetOffset;
            ApplyPosition();
            _isAnimating = false;
            _isOpen = _currentOffset > _slideDistance * 0.5f;
        }

        #endregion

        #region Position

        private void ApplyPosition()
        {
            transform.localPosition = _closedPosition + _slideDirection.normalized * _currentOffset;
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 worldDir = transform.TransformDirection(_slideDirection.normalized);
            Gizmos.DrawRay(transform.position, worldDir * _slideDistance);
            Gizmos.DrawWireSphere(transform.position + worldDir * _slideDistance, 0.05f);
        }
#endif
    }
}