using System.Collections;
using UnityEngine;

namespace TheOrder.Ending
{
    /// <summary>
    /// IInteractable on the "seat" GameObject. Lets the player enter/exit the car
    /// once all parts are drilled. Handles camera transition to driving view.
    /// </summary>
    public class CarSeat : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Transform _seatCameraPosition;
        [SerializeField] private CarRepairStation _station;

        [Header("Settings")]
        [SerializeField] private float _transitionDuration = 0.4f;

        #endregion

        #region Private Fields

        private bool _isSeated;
        private bool _isTransitioning;
        private Transform _playerTransform;
        private Transform _cameraTransform;
        private CharacterController _characterController;
        private PlayerCamera.FirstPersonCamera _firstPersonCamera;
        private Vector3 _cachedPlayerPosition;
        private Quaternion _cachedPlayerRotation;
        private Vector3 _cachedCameraLocalPos;
        private Quaternion _cachedCameraLocalRot;

        #endregion

        #region Public API

        /// <summary>Whether the player is currently seated in the car.</summary>
        public bool IsSeated => _isSeated;

        #endregion

        #region IInteractable

        public void Interact(GameObject interactor)
        {
            if (_isTransitioning) return;

            CachePlayerReferences(interactor);
            if (_characterController == null || _cameraTransform == null) return;

            if (_isSeated)
            {
                StartCoroutine(ExitCarRoutine());
            }
            else
            {
                StartCoroutine(EnterCarRoutine());
            }
        }

        public string GetPromptText()
        {
            if (_isTransitioning) return "";

            if (_isSeated)
            {
                return "Exit car";
            }

            if (!AllPartsDrilled())
            {
                return "Car seat";
            }

            return "Sit in car";
        }

        public bool CanInteract(GameObject interactor)
        {
            if (_isTransitioning) return false;
            if (_isSeated) return true;
            return AllPartsDrilled();
        }

        public string GetBlockedMessage()
        {
            if (_isTransitioning) return "";
            if (!_isSeated && !AllPartsDrilled())
            {
                return "Parts not secured";
            }
            return "";
        }

        #endregion

        #region Private Methods

        private bool AllPartsDrilled()
        {
            return _station != null && _station.AllPartsDrilled;
        }

        private void CachePlayerReferences(GameObject interactor)
        {
            if (_playerTransform != null) return;

            _playerTransform = interactor.transform;
            _characterController = interactor.GetComponent<CharacterController>();
            _firstPersonCamera = interactor.GetComponentInChildren<PlayerCamera.FirstPersonCamera>();
            if (_firstPersonCamera != null)
            {
                _cameraTransform = _firstPersonCamera.transform;
            }
        }

        private IEnumerator EnterCarRoutine()
        {
            _isTransitioning = true;

            // Cache position for exit
            _cachedPlayerPosition = _playerTransform.position;
            _cachedPlayerRotation = _playerTransform.rotation;
            _cachedCameraLocalPos = _cameraTransform.localPosition;
            _cachedCameraLocalRot = _cameraTransform.localRotation;

            // Disable movement and camera look
            _characterController.enabled = false;
            _firstPersonCamera.IsEnabled = false;

            // Move player to seat area (hidden, just for position)
            _playerTransform.position = _seatCameraPosition.position;

            // Lerp camera to seat view
            Vector3 startPos = _cameraTransform.position;
            Quaternion startRot = _cameraTransform.rotation;
            Vector3 endPos = _seatCameraPosition.position;
            Quaternion endRot = _seatCameraPosition.rotation;

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _transitionDuration);
                _cameraTransform.position = Vector3.Lerp(startPos, endPos, t);
                _cameraTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            _cameraTransform.position = endPos;
            _cameraTransform.rotation = endRot;

            // Re-enable camera look so player can look around inside
            _firstPersonCamera.IsEnabled = true;
            _isSeated = true;
            _isTransitioning = false;
        }

        private IEnumerator ExitCarRoutine()
        {
            _isTransitioning = true;
            _firstPersonCamera.IsEnabled = false;

            // Restore player to exact cached position first
            _playerTransform.position = _cachedPlayerPosition;
            _playerTransform.rotation = _cachedPlayerRotation;

            // Restore camera local transform to exactly what it was before entering
            _cameraTransform.localPosition = _cachedCameraLocalPos;
            _cameraTransform.localRotation = _cachedCameraLocalRot;

            // Re-enable controller and camera
            _characterController.enabled = true;
            _firstPersonCamera.IsEnabled = true;

            _isSeated = false;
            _isTransitioning = false;
            yield break;
        }

        #endregion
    }
}
