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

        [Header("Mode")]
        [Tooltip("When true, uses outdoor/indoor ambient swap instead of config switching.")]
        [SerializeField] private bool _isOutdoorZone;

        [Header("Audio Configs (config-switch mode)")]
        [Tooltip("Config to use when the player enters this trigger (e.g., outdoor config).")]
        [SerializeField] private AudioConfig _enterConfig;

        [Tooltip("Config to restore when the player exits this trigger (e.g., indoor config).")]
        [SerializeField] private AudioConfig _exitConfig;

        [Header("References")]
        [SerializeField] private AmbientAudioManager _audioManager;

        #endregion

        private const int PLAYER_LAYER = 8;

        #region Unity Lifecycle

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != PLAYER_LAYER) return;
            if (_audioManager == null) return;

            if (_isOutdoorZone)
            {
                _audioManager.PlayOutdoorAmbient();
            }
            else
            {
                if (_enterConfig != null)
                    _audioManager.SetConfig(_enterConfig);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != PLAYER_LAYER) return;
            if (_audioManager == null) return;

            if (_isOutdoorZone)
            {
                _audioManager.StopOutdoorAmbient();
            }
            else
            {
                if (_exitConfig != null)
                    _audioManager.SetConfig(_exitConfig);
            }
        }

        #endregion
    }
}
