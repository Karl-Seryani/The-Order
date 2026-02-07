using System.Collections;
using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// Interactive door that toggles between open and closed states.
    /// Implements IInteractable for player use (E key).
    /// Provides OpenDoor() for Hunter AI to force-open.
    /// </summary>
    public class DoorController : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Door Settings")]
        [SerializeField] private float _rotationAngle = 90f;
        [SerializeField] private float _rotationDuration = 0.5f;
        [SerializeField] private Transform _hingePoint;

        #endregion

        #region Private Fields

        private bool _isOpen;
        private bool _isAnimating;
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private Transform _doorTransform;

        #endregion

        #region Public API

        /// <summary>True if the door is currently open.</summary>
        public bool IsOpen => _isOpen;

        /// <summary>True if the door is currently animating.</summary>
        public bool IsAnimating => _isAnimating;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _doorTransform = _hingePoint != null ? _hingePoint : transform;
            _closedRotation = _doorTransform.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, _rotationAngle, 0f);
        }

        #endregion

        #region IInteractable

        /// <summary>Toggle the door open/closed when the player interacts.</summary>
        public void Interact(GameObject interactor)
        {
            if (_isAnimating) return;

            if (_isOpen)
                CloseDoor();
            else
                OpenDoor();
        }

        /// <summary>Returns context-appropriate prompt text.</summary>
        public string GetPromptText()
        {
            if (_isAnimating) return string.Empty;
            return _isOpen ? "Close Door" : "Open Door";
        }

        #endregion

        #region Door Control

        /// <summary>
        /// Open the door. Can be called by player interaction or Hunter AI.
        /// </summary>
        public void OpenDoor()
        {
            if (_isOpen || _isAnimating) return;
            StartCoroutine(AnimateDoor(true));
        }

        /// <summary>
        /// Close the door.
        /// </summary>
        public void CloseDoor()
        {
            if (!_isOpen || _isAnimating) return;
            StartCoroutine(AnimateDoor(false));
        }

        private IEnumerator AnimateDoor(bool opening)
        {
            _isAnimating = true;

            Quaternion startRotation = _doorTransform.localRotation;
            Quaternion targetRotation = opening ? _openRotation : _closedRotation;
            float elapsed = 0f;

            while (elapsed < _rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _rotationDuration);
                _doorTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }

            _doorTransform.localRotation = targetRotation;
            _isOpen = opening;
            _isAnimating = false;

            // Fire appropriate event for audio and Hunter detection
            if (opening)
                GameEvents.DoorOpened(transform.position);
            else
                GameEvents.DoorClosed(transform.position);
        }

        #endregion
    }
}
