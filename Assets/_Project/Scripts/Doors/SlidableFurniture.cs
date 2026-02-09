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
        [SerializeField] private float _slideSpeed = 1.5f;

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
            if (_isAnimating) return;

            float targetOffset = _isOpen ? 0f : _slideDistance;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateSlide(targetOffset));
        }

        /// <summary>Returns prompt text.</summary>
        public string GetPromptText()
        {
            return _promptText;
        }

        #endregion

        #region Animation

        private IEnumerator AnimateSlide(float targetOffset)
        {
            _isAnimating = true;
            float direction = targetOffset > _currentOffset ? 1f : -1f;

            while (Mathf.Abs(_currentOffset - targetOffset) > 0.005f)
            {
                _currentOffset += _slideSpeed * direction * Time.deltaTime;
                _currentOffset = direction > 0f
                    ? Mathf.Min(_currentOffset, targetOffset)
                    : Mathf.Max(_currentOffset, targetOffset);
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