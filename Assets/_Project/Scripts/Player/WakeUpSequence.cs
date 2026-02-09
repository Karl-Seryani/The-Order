using System.Collections;
using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// First-person wake-up cinematic. Camera starts tilted sideways (lying on bed),
    /// blinks open three times while rising to upright, then grants player control.
    /// Uses coroutine-based camera animation sequence.
    /// </summary>
    public class WakeUpSequence : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Transform _playerCamera;
        [SerializeField] private CanvasGroup _blinkOverlay;

        [Header("Timing")]
        [SerializeField] private float _initialBlackDuration = 1.5f;
        [SerializeField] private float _blink1OpenDuration = 0.5f;
        [SerializeField] private float _blink1CloseDuration = 0.3f;
        [SerializeField] private float _blink2OpenDuration = 1.0f;
        [SerializeField] private float _blink2CloseDuration = 0.3f;
        [SerializeField] private float _blink3FadeInDuration = 0.3f;
        [SerializeField] private float _riseUpDuration = 3.0f;

        [Header("Camera")]
        [SerializeField] private float _startingZRoll = 90f;

        #endregion

        #region Private Fields

        private PlayerCamera.FirstPersonCamera _firstPersonCamera;
        private bool _sequenceComplete;

        #endregion

        #region Public API

        /// <summary>True after the wake-up sequence has finished.</summary>
        public bool IsComplete => _sequenceComplete;

        /// <summary>Start the wake-up sequence. Called by BunkerSceneBootstrap.</summary>
        public void Begin()
        {
            // Find the first-person camera script to disable during sequence
            if (_playerCamera != null)
                _firstPersonCamera = _playerCamera.GetComponent<PlayerCamera.FirstPersonCamera>();

            StartCoroutine(PlayWakeUpSequence());
        }

        /// <summary>Immediately finish the wake-up sequence (used on respawn).</summary>
        public void Skip()
        {
            if (_playerCamera != null)
                _firstPersonCamera = _playerCamera.GetComponent<PlayerCamera.FirstPersonCamera>();

            SetCameraRoll(0f);
            if (_blinkOverlay != null)
                _blinkOverlay.alpha = 0f;

            if (_firstPersonCamera != null)
                _firstPersonCamera.IsEnabled = true;

            _sequenceComplete = true;
        }

        #endregion

        #region Sequence

        private IEnumerator PlayWakeUpSequence()
        {
            // Disable camera look so we can control rotation
            if (_firstPersonCamera != null)
                _firstPersonCamera.IsEnabled = false;

            // Start fully black, camera tilted sideways
            if (_blinkOverlay != null)
                _blinkOverlay.alpha = 1f;
            SetCameraRoll(_startingZRoll);

            // Hold black
            yield return new WaitForSeconds(_initialBlackDuration);

            // Blink 1: brief peek — see blurry sideways room
            yield return StartCoroutine(Blink(
                _blink1OpenDuration, _blink1CloseDuration,
                _startingZRoll, 60f));

            // Blink 2: longer open — camera continues rising
            yield return StartCoroutine(Blink(
                _blink2OpenDuration, _blink2CloseDuration,
                60f, 30f));

            // Blink 3: eyes stay open, camera rises to upright
            yield return StartCoroutine(FadeOverlay(1f, 0f, _blink3FadeInDuration));
            yield return StartCoroutine(RiseToUpright(30f, 0f, _riseUpDuration));

            // Ensure clean final state
            SetCameraRoll(0f);
            if (_blinkOverlay != null)
                _blinkOverlay.alpha = 0f;

            // Re-enable camera look
            if (_firstPersonCamera != null)
                _firstPersonCamera.IsEnabled = true;

            // Transition to gameplay
            _sequenceComplete = true;
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        #endregion

        #region Blink Helpers

        /// <summary>
        /// Simulate a single blink: open eyes, hold while rotating, close eyes.
        /// </summary>
        private IEnumerator Blink(float openDuration, float closeDuration,
            float startRoll, float endRoll)
        {
            // Open eyes
            yield return StartCoroutine(FadeOverlay(1f, 0f, 0.15f));

            // Hold open while camera rotates
            float elapsed = 0f;
            while (elapsed < openDuration)
            {
                float t = elapsed / openDuration;
                float roll = Mathf.Lerp(startRoll, endRoll, t);
                SetCameraRoll(roll);
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetCameraRoll(endRoll);

            // Close eyes
            yield return StartCoroutine(FadeOverlay(0f, 1f, closeDuration));
        }

        /// <summary>
        /// Smoothly rise from startRoll to endRoll with smoothstep easing.
        /// </summary>
        private IEnumerator RiseToUpright(float startRoll, float endRoll, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Smoothstep for natural motion
                float smoothT = t * t * (3f - 2f * t);
                SetCameraRoll(Mathf.Lerp(startRoll, endRoll, smoothT));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetCameraRoll(endRoll);
        }

        #endregion

        #region Utilities

        private void SetCameraRoll(float zAngle)
        {
            if (_playerCamera == null) return;
            Vector3 euler = _playerCamera.localEulerAngles;
            euler.z = zAngle;
            _playerCamera.localEulerAngles = euler;
        }

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            if (_blinkOverlay == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _blinkOverlay.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _blinkOverlay.alpha = to;
        }

        #endregion
    }
}
