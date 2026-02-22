using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Orchestrates the death camera sequence when the Hunter catches the player.
    /// Locks camera onto Hunter, plays attack animation, shows blood splash,
    /// player falls to the right, then fires PlayerCaught for the existing death screen.
    /// </summary>
    public class DeathCinematic : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Camera Lock")]
        [SerializeField] private float _lookLerpSpeed = 8f;

        [Header("Fall")]
        [SerializeField] private float _fallDelay = 0.6f;
        [SerializeField] private float _fallDuration = 0.5f;
        [SerializeField] private float _cameraDropHeight = 1.2f;

        [Header("Hold")]
        [SerializeField] private float _groundHoldTime = 0.6f;

        [Header("Blood Overlay")]
        [SerializeField] private Sprite _bloodSprite;
        [SerializeField] [Range(0f, 1f)] private float _bloodAlpha = 0.7f;

        #endregion

        #region Private Fields

        private bool _isPlaying;
        private Hunter.HunterAI _hunterAI;
        private PlayerCamera.FirstPersonCamera _fpCamera;
        private Player.PlayerController _playerController;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            GameEvents.OnDeathCinematicStart += HandleDeathCinematicStart;
        }

        private void OnDisable()
        {
            GameEvents.OnDeathCinematicStart -= HandleDeathCinematicStart;
        }

        #endregion

        #region Event Handlers

        private void HandleDeathCinematicStart()
        {
            if (_isPlaying) return;
            _hunterAI = FindFirstObjectByType<Hunter.HunterAI>();
            _fpCamera = FindFirstObjectByType<PlayerCamera.FirstPersonCamera>();
            _playerController = FindFirstObjectByType<Player.PlayerController>();
            StartCoroutine(DeathCinematicSequence());
        }

        #endregion

        #region Blood Overlay

        /// <summary>
        /// Creates a fullscreen blood splash overlay on the player's camera canvas.
        /// Returns the created Image so it can be faded.
        /// </summary>
        private Image CreateBloodOverlay()
        {
            if (_bloodSprite == null) return null;

            // Create a temporary canvas on the camera
            var overlayGO = new GameObject("BloodOverlay");
            var canvas = overlayGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            var imageGO = new GameObject("BloodImage");
            imageGO.transform.SetParent(overlayGO.transform, false);

            // Add CanvasScaler so it scales properly
            var scaler = overlayGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var image = imageGO.AddComponent<Image>();
            image.sprite = _bloodSprite;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
            image.color = new Color(1f, 1f, 1f, 0f);

            // Fill the entire screen — stretch well beyond edges so the blood covers everything
            var rect = imageGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(4800f, 2800f);
            rect.anchoredPosition = Vector2.zero;

            return image;
        }

        #endregion

        #region Cinematic Sequence

        private IEnumerator DeathCinematicSequence()
        {
            _isPlaying = true;

            // --- Setup: use cached references ---
            var hunterAI = _hunterAI;
            var fpCamera = _fpCamera;
            var playerController = _playerController;

            if (hunterAI == null || fpCamera == null)
            {
                Debug.LogWarning("[DeathCinematic] Missing references — skipping cinematic.");
                GameEvents.PlayerCaught();
                _isPlaying = false;
                yield break;
            }

            Transform cameraTransform = fpCamera.transform;
            Transform playerBody = cameraTransform.parent != null ? cameraTransform.parent : playerController.transform;

            // Disable camera mouse look
            fpCamera.IsEnabled = false;

            // Disable CharacterController (required before transform changes)
            CharacterController cc = playerBody.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Position player close to the Hunter, offset to Hunter's right
            // so the player is in front of the Hunter's swinging hand
            Transform hunterTransform = hunterAI.transform;
            const float DESIRED_DISTANCE = 1.2f;
            Vector3 inFront = hunterTransform.position + hunterTransform.forward * DESIRED_DISTANCE;
            // Shift to the Hunter's right (player's left) to line up with attack swing
            Vector3 rightOffset = hunterTransform.right * 0.6f;
            playerBody.position = new Vector3(inFront.x + rightOffset.x, playerBody.position.y, inFront.z + rightOffset.z);

            // Stop the Hunter's NavMeshAgent completely
            var hunterAgent = hunterAI.Agent;
            if (hunterAgent != null)
            {
                hunterAgent.isStopped = true;
                hunterAgent.velocity = Vector3.zero;
            }

            // Make the Hunter face the player, rotated slightly to his left
            Vector3 hunterLookDir = playerBody.position - hunterTransform.position;
            hunterLookDir.y = 0f;
            if (hunterLookDir.sqrMagnitude > 0.01f)
            {
                Quaternion baseLook = Quaternion.LookRotation(hunterLookDir);
                hunterTransform.rotation = baseLook * Quaternion.Euler(0f, -25f, 0f);
            }

            // Play Hunter attack animation
            hunterAI.PlayAttack();

            // Prepare blood overlay (hidden initially)
            Image bloodImage = CreateBloodOverlay();

            // --- Phase 1: Smooth camera turn toward Hunter (0 to _fallDelay) ---
            // Offset the look target slightly to the player's left (Hunter's right hand swings left)
            float elapsed = 0f;
            while (elapsed < _fallDelay)
            {
                elapsed += Time.deltaTime;

                // Look at Hunter's chest, offset to the left to match attack swing
                Vector3 lookTarget = hunterAI.GetLookTarget();
                Vector3 toTarget = lookTarget - cameraTransform.position;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toTarget);
                    Vector3 euler = targetRot.eulerAngles;

                    // Smooth yaw
                    Quaternion targetYaw = Quaternion.Euler(0f, euler.y, 0f);
                    playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetYaw, _lookLerpSpeed * Time.deltaTime);

                    // Smooth pitch
                    float targetPitch = euler.x;
                    if (targetPitch > 180f) targetPitch -= 360f;
                    targetPitch = Mathf.Clamp(targetPitch, -85f, 85f);
                    Quaternion goalPitch = Quaternion.Euler(targetPitch, 0f, 0f);
                    cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, goalPitch, _lookLerpSpeed * Time.deltaTime);
                }

                yield return null;
            }

            // --- Blood splash on hit ---
            if (bloodImage != null)
            {
                Color c = bloodImage.color;
                c.a = _bloodAlpha;
                bloodImage.color = c;
            }

            // Small impact shake
            cameraTransform.localRotation *= Quaternion.Euler(
                Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0f);

            // --- Phase 2: Fall to the RIGHT (camera drops + rolls right) ---
            elapsed = 0f;
            Vector3 fallStartPos = cameraTransform.localPosition;
            Quaternion fallStartRot = cameraTransform.localRotation;

            // End position: dropped down + shifted right
            Vector3 fallEndPos = fallStartPos + new Vector3(0.3f, -_cameraDropHeight, 0f);

            // End rotation: rolled right (Z tilt) + looking slightly up
            float fallStartPitch = fallStartRot.eulerAngles.x;
            if (fallStartPitch > 180f) fallStartPitch -= 360f;
            Quaternion fallEndRot = Quaternion.Euler(-40f, 0f, 70f);

            while (elapsed < _fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _fallDuration);

                // Ease-in for impact feel (accelerating fall)
                float easedT = t * t;

                // Drop + shift right
                cameraTransform.localPosition = Vector3.Lerp(fallStartPos, fallEndPos, easedT);

                // Roll right + tilt up
                cameraTransform.localRotation = Quaternion.Slerp(fallStartRot, fallEndRot, easedT);

                yield return null;
            }

            // Snap to final fall position
            cameraTransform.localPosition = fallEndPos;
            cameraTransform.localRotation = fallEndRot;

            // --- Phase 3: Hold on ground, looking up at Hunter from the side ---
            yield return new WaitForSeconds(_groundHoldTime);

            // --- Cleanup and fire PlayerCaught ---
            if (bloodImage != null)
            {
                Destroy(bloodImage.transform.parent.gameObject);
            }

            _isPlaying = false;
            GameEvents.PlayerCaught();
        }

        #endregion
    }
}
