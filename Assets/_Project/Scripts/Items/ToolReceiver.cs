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

        [Header("Audio")]
        [SerializeField] private AudioClip _useSound;

        #endregion

        #region Private Fields

        private bool _isUsed;
        private AudioSource _audioSource;

        #endregion

        #region Public API

        /// <summary>True if this receiver has already been used.</summary>
        public bool IsUsed => _isUsed;

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

            // Activate / deactivate linked objects
            if (_objectToDeactivate != null)
                _objectToDeactivate.SetActive(false);
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

            GameObject rewardGo;

            if (_rewardItem.MeshPrefab != null)
            {
                rewardGo = Instantiate(_rewardItem.MeshPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                rewardGo = new GameObject($"Reward_{_rewardItem.DisplayName}");
                rewardGo.transform.position = spawnPos;
            }

            // Ensure collider for raycasting
            if (rewardGo.GetComponentInChildren<Collider>() == null)
            {
                rewardGo.AddComponent<BoxCollider>();
            }

            var pickup = rewardGo.GetComponent<ItemPickup>();
            if (pickup == null)
            {
                pickup = rewardGo.AddComponent<ItemPickup>();
            }
            pickup.Initialize(_rewardItem);

            rewardGo.name = $"Reward_{_rewardItem.DisplayName}";
        }

        #endregion
    }
}
