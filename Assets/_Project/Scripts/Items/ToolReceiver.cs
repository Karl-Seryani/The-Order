using System.Collections;
using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// An object that requires a specific item to interact with (e.g., locked drawer needs Hammer).
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

        #endregion

        #region Private Fields

        private bool _isUsed;
        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            if (_openVisual != null)
                _openVisual.SetActive(false);
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
            GameEvents.ItemUsed(heldItem.CurrentItem);
            heldItem.ClearHeldItem();

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

            // Activate / deactivate linked objects
            if (_objectToDeactivate != null)
            {
                if (_useBreakAnimation)
                    StartCoroutine(BreakAndFall(_objectToDeactivate));
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
                return $"Use {heldItem.CurrentItem.DisplayName}";

            if (_requiredItem != null)
                return $"{_lockedPrompt} — requires {_requiredItem.DisplayName}";

            return _lockedPrompt;
        }

        #endregion

        #region Private Methods

        private void SpawnReward()
        {
            var spawnPos = _rewardSpawnPoint != null
                ? _rewardSpawnPoint.position
                : transform.position + Vector3.up * 0.5f;

            ItemSpawner.SpawnPickup(_rewardItem, spawnPos);
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
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

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
