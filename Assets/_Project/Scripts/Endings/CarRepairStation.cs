using UnityEngine;

namespace TheOrder.Ending
{
    /// <summary>
    /// IInteractable on the Body_Goblin (car frame). Each install zone (child collider)
    /// maps to a specific car part. Handles:
    /// 1. Holding a car part + aiming at matching zone → place it
    /// 2. Holding the drill + aiming at a wheel zone → drill that wheel
    /// 3. All parts installed + has car key → start the car
    /// </summary>
    public class CarRepairStation : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Required Parts")]
        [SerializeField] private Items.CarPartPickup[] _requiredParts;

        [Header("Drill")]
        [SerializeField] private Items.ItemData _drillItemData;

        [Header("Car Key")]
        [SerializeField] private Items.ItemData _carKeyItemData;

        [Header("Audio")]
        [SerializeField] private AudioClip _installClip;
        [SerializeField] private AudioClip _drillClip;
        [SerializeField] private AudioClip _carStartClip;
        [SerializeField] [Range(0f, 1f)] private float _installVolume = 0.8f;

        #endregion

        #region Private Fields

        private int _installedCount;
        private bool _carStarted;
        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1f;
            }
        }

        #endregion

        #region Zone API (called by CarInstallZone children)

        /// <summary>Called by a CarInstallZone when the player interacts with it.</summary>
        public void InteractWithZone(GameObject interactor, Items.CarPartPickup zonePart)
        {
            if (_carStarted) return;

            // --- All parts installed → start car with key ---
            if (_installedCount >= _requiredParts.Length)
            {
                TryStartCar();
                return;
            }

            var heldItem = Items.HeldItemController.Instance;
            if (heldItem == null || !heldItem.HasItem) return;

            // --- Holding drill → drill this specific wheel if it's placed ---
            if (heldItem.CurrentItem == _drillItemData)
            {
                if (zonePart != null && zonePart.RequiresDrill && zonePart.IsPlaced && !zonePart.IsInstalled)
                {
                    zonePart.Drill();
                    _installedCount++;

                    if (_drillClip != null)
                    {
                        _audioSource.PlayOneShot(_drillClip, _installVolume);
                    }

                    GameEvents.CarPartInstalled(zonePart.ItemData, _installedCount, _requiredParts.Length);
                }
                return;
            }

            // --- Holding a car part → place it if it matches this zone ---
            if (zonePart == null || zonePart.IsPlaced || !zonePart.IsCollected) return;
            if (zonePart.ItemData != heldItem.CurrentItem) return;

            zonePart.Place();
            heldItem.ClearHeldItem();

            if (_installClip != null)
            {
                _audioSource.PlayOneShot(_installClip, _installVolume);
            }

            if (!zonePart.RequiresDrill)
            {
                _installedCount++;
                GameEvents.CarPartInstalled(zonePart.ItemData, _installedCount, _requiredParts.Length);
            }
        }

        /// <summary>Whether the player can interact with a specific zone right now.</summary>
        public bool CanInteractWithZone(GameObject interactor, Items.CarPartPickup zonePart)
        {
            if (_carStarted) return false;

            if (_installedCount >= _requiredParts.Length)
            {
                var inventory = Player.PlayerInventory.Instance;
                return inventory != null && inventory.HasKey(_carKeyItemData);
            }

            var heldItem = Items.HeldItemController.Instance;
            if (heldItem == null || !heldItem.HasItem) return false;

            // Holding drill — check if this zone has a drillable wheel
            if (heldItem.CurrentItem == _drillItemData)
            {
                return zonePart != null && zonePart.RequiresDrill && zonePart.IsPlaced && !zonePart.IsInstalled;
            }

            // Holding a car part — check if it matches this zone
            if (zonePart == null || zonePart.IsPlaced || !zonePart.IsCollected) return false;
            return zonePart.ItemData == heldItem.CurrentItem;
        }

        /// <summary>Returns blocked message for a specific zone.</summary>
        public string GetZoneBlockedMessage(Items.CarPartPickup zonePart)
        {
            if (_carStarted) return "";

            if (_installedCount >= _requiredParts.Length)
            {
                return "Need Car Key";
            }

            var heldItem = Items.HeldItemController.Instance;
            if (heldItem == null || !heldItem.HasItem) return "";

            // Holding a car part at wrong zone
            if (heldItem.CurrentItem != _drillItemData && IsCarPart(heldItem.CurrentItem))
            {
                if (zonePart != null && !zonePart.IsPlaced)
                {
                    return "Wrong spot";
                }
            }

            return "";
        }

        /// <summary>Returns prompt text for a specific zone.</summary>
        public string GetZonePromptText(Items.CarPartPickup zonePart)
        {
            if (_carStarted) return "";

            // All parts installed — need car key
            if (_installedCount >= _requiredParts.Length)
            {
                var inventory = Player.PlayerInventory.Instance;
                if (inventory != null && inventory.HasKey(_carKeyItemData))
                {
                    return "Start car";
                }
                return "Need Car Key";
            }

            var heldItem = Items.HeldItemController.Instance;

            if (heldItem == null || !heldItem.HasItem)
            {
                return $"Car  [{_installedCount}/{_requiredParts.Length}]";
            }

            // Holding drill — check this specific zone's wheel
            if (heldItem.CurrentItem == _drillItemData)
            {
                if (zonePart != null && zonePart.RequiresDrill && zonePart.IsPlaced && !zonePart.IsInstalled)
                {
                    return "Drill wheel";
                }
                return $"Car  [{_installedCount}/{_requiredParts.Length}]";
            }

            // Holding a car part — check if it matches this zone
            if (zonePart != null && !zonePart.IsPlaced && zonePart.IsCollected && zonePart.ItemData == heldItem.CurrentItem)
            {
                return $"Place {zonePart.ItemData.DisplayName}";
            }

            // Holding a car part but wrong zone
            if (zonePart != null && !zonePart.IsPlaced && IsCarPart(heldItem.CurrentItem))
            {
                return $"Wrong spot  -  holding {heldItem.CurrentItem.DisplayName}";
            }

            return $"Car  [{_installedCount}/{_requiredParts.Length}]";
        }

        #endregion

        #region IInteractable (fallback for hitting Body_Goblin directly)

        public void Interact(GameObject interactor)
        {
            if (_carStarted) return;

            if (_installedCount >= _requiredParts.Length)
            {
                TryStartCar();
            }
        }

        public string GetPromptText()
        {
            if (_carStarted) return "";

            if (_installedCount >= _requiredParts.Length)
            {
                var inventory = Player.PlayerInventory.Instance;
                if (inventory != null && inventory.HasKey(_carKeyItemData))
                {
                    return "Start car";
                }
                return "Need Car Key";
            }

            return $"Car  [{_installedCount}/{_requiredParts.Length}]";
        }

        public bool CanInteract(GameObject interactor)
        {
            if (_carStarted) return false;
            if (_installedCount >= _requiredParts.Length)
            {
                var inventory = Player.PlayerInventory.Instance;
                return inventory != null && inventory.HasKey(_carKeyItemData);
            }
            return true;
        }

        public string GetBlockedMessage()
        {
            if (_carStarted) return "";
            if (_installedCount >= _requiredParts.Length)
            {
                return "Need Car Key";
            }
            return "";
        }

        #endregion

        #region Private Methods

        private void TryStartCar()
        {
            var inventory = Player.PlayerInventory.Instance;
            if (inventory == null || !inventory.HasKey(_carKeyItemData)) return;

            _carStarted = true;

            if (_carStartClip != null)
            {
                _audioSource.PlayOneShot(_carStartClip, _installVolume);
            }

            GameEvents.CarRepairComplete();
        }

        private bool IsCarPart(Items.ItemData item)
        {
            if (item == null) return false;
            for (int i = 0; i < _requiredParts.Length; i++)
            {
                if (_requiredParts[i] != null && _requiredParts[i].ItemData == item)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
