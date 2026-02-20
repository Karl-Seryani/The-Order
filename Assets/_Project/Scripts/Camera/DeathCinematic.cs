using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.PlayerCamera
{
    /// <summary>
    /// Death cinematic sequence. Placed on the player camera object.
    /// On PlayerCaught: disables input, turns camera to face the Hunter,
    /// plays hit reactions with red slash marks, then falls and cuts to black.
    /// </summary>
    public class DeathCinematic : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Timing — slash impacts (seconds from sequence start)")]
        [SerializeField] private float _lookAtDuration = 0.3f;
        [SerializeField] private float _firstHitTime = 0.7f;
        [SerializeField] private float _secondHitTime = 1.5f;
        [SerializeField] private float _thirdHitTime = 2.5f;

        [Header("Timing — reactions")]
        [SerializeField] private float _flinchDuration = 0.15f;
        [SerializeField] private float _returnDuration = 0.2f;
        [SerializeField] private float _fallDuration = 0.4f;

        [Header("Camera")]
        [SerializeField] private float _flinchAngle = 12f;
        [SerializeField] private float _fallTargetPitch = 60f;
        [SerializeField] private float _fallTargetRoll = 35f;
        [SerializeField] private float _stepBackDistance = 1.5f;
        [SerializeField] private float _lookAtHeightOffset = 0.2f;

        [Header("Slash Marks")]
        [SerializeField] private Color _slashColor = new Color(0.6f, 0f, 0f, 0.8f);
        [SerializeField] private float _slashFadeDuration = 0.1f;

        [Header("References")]
        [SerializeField] private FirstPersonCamera _firstPersonCamera;
        [SerializeField] private Transform _playerBody;

        #endregion

        #region Private Fields

        private bool _isPlaying;
        private float _currentPitch;
        private float _currentRoll;

        private Canvas _overlayCanvas;
        private Image _slashLeft;
        private Image _slashRight;
        private Image _slashDiagonal;
        private Image _blackout;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CreateOverlayUI();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerCaught += HandlePlayerCaught;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerCaught -= HandlePlayerCaught;
        }

        #endregion

        #region Event Handlers

        private void HandlePlayerCaught(Transform hunterTransform)
        {
            if (_isPlaying || hunterTransform == null) return;
            StartCoroutine(CinematicSequence(hunterTransform));
        }

        #endregion

        #region Cinematic Sequence

        private IEnumerator CinematicSequence(Transform hunterTransform)
        {
            _isPlaying = true;
            float sequenceStart = Time.time;

            // Freeze mouse look
            if (_firstPersonCamera != null)
            {
                _firstPersonCamera.IsEnabled = false;
            }

            // Disable player input and movement
            GameEvents.DeathCinematicStarted();
            var playerController = _playerBody.GetComponent<Player.PlayerController>();
            if (playerController != null) playerController.enabled = false;
            var characterController = _playerBody.GetComponent<CharacterController>();
            if (characterController != null) characterController.enabled = false;

            // Step player back from the Hunter for better framing
            Vector3 awayFromHunter = _playerBody.position - hunterTransform.position;
            awayFromHunter.y = 0f;
            if (awayFromHunter.sqrMagnitude > 0.001f)
            {
                _playerBody.position += awayFromHunter.normalized * _stepBackDistance;
            }

            // === Phase 1: Smooth look at the Hunter ===
            Vector3 targetPos = hunterTransform.position + Vector3.up * _lookAtHeightOffset;
            Vector3 toTarget = targetPos - transform.position;

            Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
            Quaternion endBodyRot = flatDir.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(flatDir)
                : _playerBody.rotation;

            Quaternion worldLook = Quaternion.LookRotation(toTarget.normalized);
            Quaternion endCamRot = Quaternion.Inverse(endBodyRot) * worldLook;

            Quaternion startBodyRot = _playerBody.rotation;
            Quaternion startCamRot = transform.localRotation;

            float elapsed = 0f;
            while (elapsed < _lookAtDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _lookAtDuration);
                _playerBody.rotation = Quaternion.Slerp(startBodyRot, endBodyRot, t);
                transform.localRotation = Quaternion.Slerp(startCamRot, endCamRot, t);
                yield return null;
            }

            _playerBody.rotation = endBodyRot;
            transform.localRotation = endCamRot;

            _currentPitch = transform.localEulerAngles.x;
            if (_currentPitch > 180f) _currentPitch -= 360f;
            _currentRoll = 0f;

            // Enable overlay canvas
            if (_overlayCanvas != null) _overlayCanvas.enabled = true;

            // === Phase 2: First slash — flinch right + left slash mark ===
            yield return WaitUntilSequenceTime(sequenceStart, _firstHitTime);
            StartCoroutine(FadeInImage(_slashLeft));
            yield return StartCoroutine(Flinch(_flinchAngle, _flinchDuration));

            // === Phase 3: Second slash — flinch left + right slash mark ===
            yield return WaitUntilSequenceTime(sequenceStart, _secondHitTime);
            StartCoroutine(FadeInImage(_slashRight));
            yield return StartCoroutine(Flinch(-_flinchAngle, _flinchDuration));

            // Return to center
            yield return StartCoroutine(Flinch(0f, _returnDuration));

            // === Phase 4: 360 slash — diagonal slash + fall to ground ===
            yield return WaitUntilSequenceTime(sequenceStart, _thirdHitTime);
            StartCoroutine(FadeInImage(_slashDiagonal));
            yield return StartCoroutine(FallToGround());

            // === Phase 5: Cut to black instantly, then hand off for YOU DIED ===
            SetImageAlpha(_blackout, 1f);
            GameEvents.DeathCinematicComplete();
        }

        #endregion

        #region Camera Helpers

        private IEnumerator WaitUntilSequenceTime(float sequenceStart, float targetTime)
        {
            float remaining = targetTime - (Time.time - sequenceStart);
            if (remaining > 0f)
            {
                yield return new WaitForSeconds(remaining);
            }
        }

        private IEnumerator Flinch(float targetRoll, float duration)
        {
            float startRoll = _currentRoll;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _currentRoll = Mathf.Lerp(startRoll, targetRoll, t);
                transform.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentRoll);
                yield return null;
            }

            _currentRoll = targetRoll;
            transform.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentRoll);
        }

        private IEnumerator FallToGround()
        {
            float startPitch = _currentPitch;
            float startRoll = _currentRoll;
            float endPitch = startPitch + _fallTargetPitch;
            float endRoll = startRoll + _fallTargetRoll;
            float elapsed = 0f;

            while (elapsed < _fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _fallDuration);
                float easedT = t * t;
                _currentPitch = Mathf.Lerp(startPitch, endPitch, easedT);
                _currentRoll = Mathf.Lerp(startRoll, endRoll, easedT);
                transform.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentRoll);
                yield return null;
            }

            _currentPitch = endPitch;
            _currentRoll = endRoll;
            transform.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentRoll);
        }

        #endregion

        #region Overlay UI

        /// <summary>
        /// Creates the screen overlay: 3 red slash marks + blackout panel.
        /// </summary>
        private void CreateOverlayUI()
        {
            var canvasGO = new GameObject("DeathOverlayCanvas");
            canvasGO.transform.SetParent(transform, false);

            _overlayCanvas = canvasGO.AddComponent<Canvas>();
            _overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _overlayCanvas.sortingOrder = 90;
            _overlayCanvas.enabled = false;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Slash 1: left side, angled slightly
            _slashLeft = CreateSlashMark(canvasGO.transform, "SlashLeft",
                new Vector2(0.1f, 0.5f),  // anchored left-center
                new Vector2(80f, 600f),    // thin and tall
                -15f);                     // slight angle

            // Slash 2: right side, opposite angle
            _slashRight = CreateSlashMark(canvasGO.transform, "SlashRight",
                new Vector2(0.9f, 0.5f),   // anchored right-center
                new Vector2(80f, 600f),
                15f);

            // Slash 3: diagonal across the screen
            _slashDiagonal = CreateSlashMark(canvasGO.transform, "SlashDiagonal",
                new Vector2(0.5f, 0.5f),   // centered
                new Vector2(100f, 1400f),   // long diagonal
                -35f);

            // Blackout panel: fullscreen black, behind slash marks initially but covers all
            _blackout = CreateFullscreenImage(canvasGO.transform, "Blackout", Color.black);
            _blackout.transform.SetAsLastSibling(); // On top of everything
        }

        private Image CreateSlashMark(Transform parent, string name, Vector2 anchorPos, Vector2 size, float angle)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(_slashColor.r, _slashColor.g, _slashColor.b, 0f);

            var rect = image.rectTransform;
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            return image;
        }

        private Image CreateFullscreenImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(color.r, color.g, color.b, 0f);

            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return image;
        }

        private IEnumerator FadeInImage(Image image)
        {
            if (image == null) yield break;

            float elapsed = 0f;
            float targetAlpha = image == _blackout ? 1f : _slashColor.a;

            while (elapsed < _slashFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _slashFadeDuration);
                SetImageAlpha(image, targetAlpha * t);
                yield return null;
            }

            SetImageAlpha(image, targetAlpha);
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image == null) return;
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        #endregion
    }
}
