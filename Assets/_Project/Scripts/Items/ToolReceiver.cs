using System.Collections;
using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// An object that requires a specific item to interact with (e.g., barricaded door needs Hammer).
    /// When the player uses the correct held item, it opens/breaks and optionally spawns a reward item.
    /// </summary>
    public class ToolReceiver : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Required Item")]
        [SerializeField] private ItemData _requiredItem;
        [SerializeField] private string _lockedPrompt = "Locked";

        [Header("Reward")]
        [SerializeField] private ItemData _rewardItem;
        [SerializeField] private Transform _rewardSpawnPoint;

        [Header("Visuals")]
        [SerializeField] private GameObject _closedVisual;
        [SerializeField] private GameObject _openVisual;

        [Header("Activation")]
        [SerializeField] private GameObject _objectToDeactivate;
        [SerializeField] private GameObject _objectToEnable;
        [SerializeField] private Doors.DoorController _doorToUnblock;

        [Header("Break Animation")]
        [SerializeField] private bool _useBreakAnimation;
        [SerializeField] private float _breakFallDelay = 1.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip _useSound;
        [SerializeField] private AudioClip _breakSound;

        #endregion

        #region Private Fields

        private bool _isUsed;
        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1f;
            }

            if (_openVisual != null)
                _openVisual.SetActive(false);
        }

        private void Start()
        {
            RestoreRunState();
        }

        #endregion

        #region IInteractable

        /// <summary>Check held item and activate if correct.</summary>
        public void Interact(GameObject interactor)
        {
            if (_isUsed) return;

            var heldItem = interactor.GetComponent<HeldItemController>();
            if (heldItem == null) return;

            if (!heldItem.HasItem || heldItem.CurrentItem != _requiredItem)
            {
                // Wrong item or empty hands
                return;
            }

            // Correct item — activate
            _isUsed = true;
            SaveRunState();
            GameEvents.ItemUsed(heldItem.CurrentItem);
            
            // Don't destroy the tool - player keeps it for reuse

            // Disable this component so it stops being detected as interactable
            enabled = false;

            // Swap visuals
            if (_closedVisual != null)
                _closedVisual.SetActive(false);
            if (_openVisual != null)
                _openVisual.SetActive(true);

            // Play sound
            if (_useSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_useSound);
            }

            // Unblock barricaded door
            if (_doorToUnblock != null)
                _doorToUnblock.IsBarricaded = false;

            // Open associated door if present (use Interact for animated opening)
            var doorController = GetComponent<Doors.DoorController>();
            if (doorController != null && !doorController.IsOpen)
            {
                doorController.Interact(interactor);
            }
            
            var slidableFurniture = GetComponent<Doors.SlidableFurniture>();
            if (slidableFurniture != null && !slidableFurniture.IsOpen)
            {
                slidableFurniture.Interact(interactor);
            }

            // Activate / deactivate linked objects
            if (_objectToDeactivate != null)
            {
                if (_useBreakAnimation)
                {
                    if (_breakSound != null && _audioSource != null)
                        _audioSource.PlayOneShot(_breakSound);
                    StartCoroutine(BreakAndFall(_objectToDeactivate));
                }
                else
                    _objectToDeactivate.SetActive(false);
            }
            if (_objectToEnable != null)
                _objectToEnable.SetActive(true);

            // Spawn reward item
            if (_rewardItem != null)
            {
                SpawnReward();
            }
        }

        /// <summary>Returns context-aware prompt text.</summary>
        public string GetPromptText()
        {
            if (_isUsed)
                return string.Empty;

            var heldItem = HeldItemController.Instance;

            if (heldItem != null && heldItem.HasItem && heldItem.CurrentItem == _requiredItem)
                return $"Use {_requiredItem.DisplayName}";

            if (_requiredItem != null)
                return $"Locked  -  need {_requiredItem.DisplayName}";

            return _lockedPrompt;
        }

        /// <summary>Can interact only when holding the correct tool.</summary>
        public bool CanInteract(GameObject interactor)
        {
            if (_isUsed) return true;
            var heldItem = HeldItemController.Instance;
            return heldItem != null && heldItem.HasItem && heldItem.CurrentItem == _requiredItem;
        }

        /// <summary>Returns blocked reason.</summary>
        public string GetBlockedMessage()
        {
            if (_isUsed) return "";
            if (_requiredItem != null) return $"Need {_requiredItem.DisplayName}";
            return _lockedPrompt;
        }

        #endregion

        #region Private Methods

        private void SpawnReward()
        {
            var spawnPos = _rewardSpawnPoint != null
                ? _rewardSpawnPoint.position
                : transform.position + Vector3.up * 0.8f;

            var rewardObj = ItemSpawner.SpawnPickup(_rewardItem, spawnPos);

            var rb = rewardObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Gentle pop upward — no horizontal velocity to avoid clipping into walls/floor
                rb.linearVelocity = Vector3.up * 2f;
                rb.angularVelocity = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                );

                // Prevent clipping through thin surfaces
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        private void SaveRunState()
        {
            if (RunStateManager.Instance == null) return;
            RunStateManager.Instance.MarkToolReceiverUsed(transform.GetPersistenceId());
        }

        private void RestoreRunState()
        {
            if (RunStateManager.Instance == null) return;
            string id = transform.GetPersistenceId();
            if (!RunStateManager.Instance.IsToolReceiverUsed(id)) return;

            // Silently apply the same effects as Interact without sounds/physics
            _isUsed = true;
            enabled = false;

            if (_closedVisual != null)
                _closedVisual.SetActive(false);
            if (_openVisual != null)
                _openVisual.SetActive(true);

            if (_doorToUnblock != null)
                _doorToUnblock.IsBarricaded = false;

            var doorController = GetComponent<Doors.DoorController>();
            if (doorController != null && !doorController.IsOpen)
                doorController.ForceOpen();

            var slidableFurniture = GetComponent<Doors.SlidableFurniture>();
            if (slidableFurniture != null && !slidableFurniture.IsOpen)
                slidableFurniture.ForceOpen();

            if (_objectToDeactivate != null)
                _objectToDeactivate.SetActive(false);
            if (_objectToEnable != null)
                _objectToEnable.SetActive(true);

            // Re-spawn reward item if it hasn't been consumed
            if (_rewardItem != null && !string.IsNullOrEmpty(_rewardItem.Id))
            {
                if (!RunStateManager.Instance.IsKeyConsumed(_rewardItem.Id))
                {
                    // Check if it was dropped somewhere, otherwise use original spawn point
                    Vector3 spawnPos;
                    if (RunStateManager.Instance.TryGetItemDropPosition(_rewardItem.Id, out Vector3 dropPos))
                        spawnPos = dropPos;
                    else if (_rewardSpawnPoint != null)
                        spawnPos = _rewardSpawnPoint.position;
                    else
                        spawnPos = transform.position + Vector3.up * 0.8f;

                    var rewardObj = ItemSpawner.SpawnPickup(_rewardItem, spawnPos);

                    // No velocity — just place it
                    var rb = rewardObj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }
            }
        }

        private IEnumerator BreakAndFall(GameObject target)
        {
            // Unparent so it falls freely instead of staying glued to door
            target.transform.SetParent(null);

            // MeshColliders must be convex for non-kinematic Rigidbody
            var mc = target.GetComponent<MeshCollider>();
            if (mc != null)
                mc.convex = true;

            // Add Rigidbody to let physics drop it
            var rb = target.GetComponent<Rigidbody>();
            if (rb == null)
                rb = target.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.mass = 2f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.8f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            // Give a slight outward push so it topples away from the door
            rb.AddForce(target.transform.forward * 1f + Vector3.down * 0.5f, ForceMode.Impulse);
            rb.AddTorque(target.transform.right * 3f, ForceMode.Impulse);

            yield return new WaitForSeconds(_breakFallDelay);

            if (target != null)
                target.SetActive(false);
        }

        #endregion
    }
}
