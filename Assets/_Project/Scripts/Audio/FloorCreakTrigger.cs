using UnityEngine;

namespace TheOrder.Audio
{
    /// <summary>
    /// Trigger zone that plays a floor creak SFX when the player steps on it
    /// and alerts the Hunter via GameEvents.InteractableNoise.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FloorCreakTrigger : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Audio")]
        [SerializeField] private AudioClip _creakClip;
        [SerializeField] [Range(0f, 1f)] private float _volume = 0.8f;

        [Header("Cooldown")]
        [SerializeField] private float _cooldown = 1.5f;

        [Header("Hunter Alert")]
        [SerializeField] private float _noiseLoudness = 1f;

        #endregion

        #region Private Fields

        private float _lastCreakTime = -999f;

        #endregion

        private const int PLAYER_LAYER = 8;

        #region Unity Lifecycle

        private void OnTriggerEnter(Collider other)
        {
            // Only react to Player
            if (other.gameObject.layer != PLAYER_LAYER) return;

            // Cooldown check
            if (Time.time - _lastCreakTime < _cooldown) return;

            _lastCreakTime = Time.time;

            // Play creak SFX
            if (_creakClip != null)
            {
                AudioSource.PlayClipAtPoint(_creakClip, transform.position, _volume);
            }

            // Alert Hunter
            GameEvents.InteractableNoise(transform.position, _noiseLoudness);
        }

        #endregion
    }
}
