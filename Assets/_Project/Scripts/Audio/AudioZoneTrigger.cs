using UnityEngine;

namespace TheOrder.Audio
{
    /// <summary>
    /// Trigger collider that switches the AmbientAudioManager config when the
    /// player crosses a zone boundary (e.g., bunker exit doorway).
    /// Place on a GameObject with a trigger collider at the transition point.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AudioZoneTrigger : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Audio Configs")]
        [Tooltip("Config to use when the player enters this trigger (e.g., outdoor config).")]
        [SerializeField] private AudioConfig _enterConfig;

        [Tooltip("Config to restore when the player exits this trigger (e.g., indoor config).")]
        [SerializeField] private AudioConfig _exitConfig;

        [Header("References")]
        [SerializeField] private AmbientAudioManager _audioManager;

        #endregion

        #region Unity Lifecycle

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 8) return;
            if (_audioManager == null || _enterConfig == null) return;

            _audioManager.SetConfig(_enterConfig);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != 8) return;
            if (_audioManager == null || _exitConfig == null) return;

            _audioManager.SetConfig(_exitConfig);
        }

        #endregion
    }
}
