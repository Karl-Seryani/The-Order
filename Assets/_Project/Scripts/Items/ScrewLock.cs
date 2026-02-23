using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// Monitors a set of screws and unlocks a SlidableFurniture when all are removed.
    /// Place on the parent object (e.g., LockedDrawer_OfficeA).
    /// </summary>
    public class ScrewLock : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Screws")]
        [SerializeField] private ScrewInteractable[] _screws;

        [Header("Targets")]
        [SerializeField] private Doors.SlidableFurniture[] _targetFurniture;

        [Header("Audio")]
        [SerializeField] private AudioClip _unlockSound;

        #endregion

        #region Private Fields

        private int _screwsRemaining;
        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            _screwsRemaining = _screws.Length;

            foreach (var screw in _screws)
            {
                if (screw != null)
                {
                    screw.Unscrewed += HandleScrewRemoved;

                    // Account for screws already unscrewed in a previous day
                    if (screw.IsRestoredUnscrewed)
                        _screwsRemaining--;
                }
            }

            // If all screws were already removed, unlock immediately
            if (_screwsRemaining <= 0)
            {
                foreach (var furniture in _targetFurniture)
                {
                    if (furniture != null)
                        furniture.IsLocked = false;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var screw in _screws)
            {
                if (screw != null)
                    screw.Unscrewed -= HandleScrewRemoved;
            }
        }

        #endregion

        #region Private Methods

        private void HandleScrewRemoved(ScrewInteractable screw)
        {
            _screwsRemaining--;

            if (_screwsRemaining <= 0)
            {
                foreach (var furniture in _targetFurniture)
                {
                    if (furniture != null)
                        furniture.IsLocked = false;
                }

                if (_unlockSound != null && _audioSource != null)
                    _audioSource.PlayOneShot(_unlockSound);
            }
        }

        #endregion
    }
}
