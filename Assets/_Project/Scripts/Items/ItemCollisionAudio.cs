using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// Plays impact sound and alerts the Hunter when a dropped item hits a surface.
    /// Attached automatically by ItemSpawner when items are spawned with physics.
    /// Loudness scales with impact velocity — the core Granny distraction mechanic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ItemCollisionAudio : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Audio")]
        [SerializeField] private AudioClip _impactClip;
        [SerializeField] [Range(0f, 5f)] private float _maxVolume = 0.7f;

        [Header("Settings")]
        [SerializeField] private float _minImpactSpeed = 1f;
        [SerializeField] private float _maxImpactSpeed = 8f;
        [SerializeField] private float _cooldown = 0.15f;

        #endregion

        #region Private Fields

        private float _lastImpactTime = -1f;
        private AudioSource _audioSource;

        #endregion

        #region Public API

        /// <summary>Set the impact clip and volume at runtime (used by ItemSpawner).</summary>
        public void SetImpactClip(AudioClip clip, float volumeMultiplier = 1f)
        {
            _impactClip = clip;
            _maxVolume = 0.7f * volumeMultiplier;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f;
            _audioSource.minDistance = 1f;
            _audioSource.maxDistance = 25f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_impactClip == null || _audioSource == null) return;
            if (Time.time - _lastImpactTime < _cooldown) return;

            float speed = collision.relativeVelocity.magnitude;
            if (speed < _minImpactSpeed) return;

            _lastImpactTime = Time.time;

            // Scale volume and loudness with impact speed
            float t = Mathf.InverseLerp(_minImpactSpeed, _maxImpactSpeed, speed);
            float volume = Mathf.Lerp(0.1f, _maxVolume, t);
            float loudness = Mathf.Lerp(0.2f, 1f, t);

            _audioSource.PlayOneShot(_impactClip, volume);
            GameEvents.InteractableNoise(transform.position, loudness);
        }

        #endregion
    }
}
