using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Raycasts from the camera center to detect IInteractable objects.
    /// Shows prompt text and triggers interaction on E press.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Raycast")]
        [SerializeField] private float _interactionRange = 2.5f;
        [SerializeField] private LayerMask _interactionMask = ~0;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _playerCamera;

        #endregion

        #region Private Fields

        private PlayerInputHandler _input;
        private IInteractable _currentTarget;

        #endregion

        #region Public API

        /// <summary>Currently targeted interactable, or null if none.</summary>
        public IInteractable CurrentInteractable => _currentTarget;

        /// <summary>Prompt text for the current target, or empty string.</summary>
        public string PromptText => _currentTarget?.GetPromptText() ?? string.Empty;

        /// <summary>True if currently looking at an interactable.</summary>
        public bool HasTarget => _currentTarget != null;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();

            if (_playerCamera == null)
            {
                _playerCamera = GetComponentInChildren<UnityEngine.Camera>();
            }

            if (_playerCamera == null)
            {
                Debug.LogWarning("[PlayerInteraction] No camera found — interaction disabled.", this);
            }
        }

        private void Update()
        {
            DetectInteractable();

            if (_input.InteractPressed && _currentTarget != null)
            {
                _currentTarget.Interact(gameObject);
                GameEvents.Interaction(_currentTarget);
            }
        }

        #endregion

        #region Detection

        private void DetectInteractable()
        {
            if (_playerCamera == null) { _currentTarget = null; return; }

            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionMask))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _currentTarget = interactable;
                    return;
                }
            }

            _currentTarget = null;
        }

        #endregion
    }
}
