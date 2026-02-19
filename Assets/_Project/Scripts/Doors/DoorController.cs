using System.Collections;
using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// Interactive door toggled by pressing E. Smoothly animates open/close.
    /// Retains OpenDoor()/CloseDoor() for Hunter AI instant control.
    /// </summary>
    public class DoorController : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Door Settings")]
        [SerializeField] private float _rotationAngle = 90f;
        [SerializeField] private float _openSpeed = 120f;
        [SerializeField] private Transform _hingePoint;

        [Tooltip("Local-space offset from mesh center to hinge edge. Use for cabinets without a hinge child.")]
        [SerializeField] private Vector3 _pivotOffset;

        [Header("Barricade")]
        [SerializeField] private bool _startsBarricaded;

        #endregion

        #region Private Fields

        private float _currentAngle;
        private bool _isOpen;
        private bool _isAnimating;
        private Transform _doorTransform;
        private Quaternion _closedRotation;
        private Vector3 _closedPosition;
        private Coroutine _animationCoroutine;

        #endregion

        #region Public API

        /// <summary>True if the door is more than half open.</summary>
        public bool IsOpen => _isOpen;

        /// <summary>True if currently animating.</summary>
        public bool IsAnimating => _isAnimating;

        /// <summary>Current open fraction from 0 (closed) to 1 (fully open).</summary>
        public float OpenFraction => Mathf.Abs(_rotationAngle) > 0.01f ? Mathf.Clamp01(Mathf.Abs(_currentAngle) / Mathf.Abs(_rotationAngle)) : 0f;

        /// <summary>Current angle of the door.</summary>
        public float CurrentAngle => _currentAngle;

        /// <summary>When true, door cannot be opened (blocked by barricade).</summary>
        public bool IsBarricaded { get; set; }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _doorTransform = _hingePoint != null ? _hingePoint : transform;
            _closedRotation = _doorTransform.localRotation;
            _closedPosition = _doorTransform.localPosition;
            IsBarricaded = _startsBarricaded;
        }

        #endregion

        #region IInteractable

        /// <summary>Toggle the door open/close on E press.</summary>
        public void Interact(GameObject interactor)
        {
            if (IsBarricaded) return;

            float targetAngle = _isOpen ? 0f : _rotationAngle;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateDoor(targetAngle));
        }

        /// <summary>Returns context-appropriate prompt text.</summary>
        public string GetPromptText()
        {
            if (IsBarricaded) return "Barricaded";
            return _isOpen ? "Close" : "Open";
        }

        #endregion

        #region Hunter AI — Instant Control

        /// <summary>Instantly open the door. Used by Hunter AI.</summary>
        public void OpenDoor()
        {
            if (_isOpen && Mathf.Abs(_currentAngle - _rotationAngle) < 0.5f) return;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _currentAngle = _rotationAngle;
            _isOpen = true;
            _isAnimating = false;
            ApplyRotation();
            GameEvents.DoorOpened(transform.position);
        }

        /// <summary>Instantly close the door. Used by Hunter AI.</summary>
        public void CloseDoor()
        {
            if (!_isOpen && Mathf.Abs(_currentAngle) < 0.5f) return;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _currentAngle = 0f;
            _isOpen = false;
            _isAnimating = false;
            ApplyRotation();
            GameEvents.DoorClosed(transform.position);
        }

        #endregion

        #region Animation

        private IEnumerator AnimateDoor(float targetAngle)
        {
            _isAnimating = true;
            float direction = targetAngle > _currentAngle ? 1f : -1f;

            while (Mathf.Abs(_currentAngle - targetAngle) > 0.5f)
            {
                _currentAngle += _openSpeed * direction * Time.deltaTime;
                _currentAngle = direction > 0f
                    ? Mathf.Min(_currentAngle, targetAngle)
                    : Mathf.Max(_currentAngle, targetAngle);
                ApplyRotation();
                yield return null;
            }

            _currentAngle = targetAngle;
            ApplyRotation();
            _isAnimating = false;

            bool wasOpen = _isOpen;
            _isOpen = Mathf.Abs(_currentAngle) > Mathf.Abs(_rotationAngle) * 0.5f;

            if (_isOpen && !wasOpen)
                GameEvents.DoorOpened(transform.position);
            else if (!_isOpen && wasOpen)
                GameEvents.DoorClosed(transform.position);
        }

        #endregion

        #region Rotation

        private void ApplyRotation()
        {
            if (_doorTransform == null) return;

            Quaternion targetRot = _closedRotation * Quaternion.Euler(0f, _currentAngle, 0f);
            _doorTransform.localRotation = targetRot;

            // Offset position so the door rotates around the hinge edge
            if (_pivotOffset != Vector3.zero)
            {
                Vector3 hingeInParent = _closedPosition + _closedRotation * _pivotOffset;
                Vector3 offsetFromHinge = targetRot * (-_pivotOffset);
                _doorTransform.localPosition = hingeInParent + offsetFromHinge;
            }
        }

        #endregion
    }
}