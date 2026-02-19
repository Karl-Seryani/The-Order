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
        [SerializeField] [Range(0f, 1f)] private float _maxVolume = 0.7f;

        [Header("Settings")]
        [SerializeField] private float _minImpactSpeed = 1f;
        [SerializeField] private float _maxImpactSpeed = 8f;
        [SerializeField] private float _cooldown = 0.15f;

        #endregion

        #region Private Fields

        private float _lastImpactTime = -1f;

        #endregion

        #region Public API

        /// <summary>Set the impact clip at runtime (used by ItemSpawner).</summary>
        public void SetImpactClip(AudioClip clip)
        {
            _impactClip = clip;
        }

        #endregion

        #region Unity Lifecycle

        private void OnCollisionEnter(Collision collision)
        {
            if (_impactClip == null) return;
            if (Time.time - _lastImpactTime < _cooldown) return;

            float speed = collision.relativeVelocity.magnitude;
            if (speed < _minImpactSpeed) return;

            _lastImpactTime = Time.time;

            // Scale volume and loudness with impact speed
            float t = Mathf.InverseLerp(_minImpactSpeed, _maxImpactSpeed, speed);
            float volume = Mathf.Lerp(0.1f, _maxVolume, t);
            float loudness = Mathf.Lerp(0.2f, 1f, t);

            AudioSource.PlayClipAtPoint(_impactClip, collision.contacts[0].point, volume);
            GameEvents.InteractableNoise(transform.position, loudness);
        }

        #endregion
    }
}
