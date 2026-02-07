using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder
{
    /// <summary>
    /// Drives the prologue cinematic sequence. No player input — fully scripted.
    /// Sequence: walk outside → 3s pause → nuke blast + shake →
    /// fade to black → "THE ORDER" title → fade out → transition to bunker.
    /// </summary>
    public class PrologueManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _characterAnimator;
        [SerializeField] private Transform _characterTransform;
        [SerializeField] private Transform[] _walkWaypoints;
        [SerializeField] private Camera _cinematicCamera;

        [Header("Camera POV")]
        [SerializeField] private float _eyeHeight = 1.68f;

        private bool _cameraFollowing = true;

        [Header("Nuke VFX")]
        [SerializeField] private GameObject _nukePrefab;
        [SerializeField] private Transform _nukeSpawnPoint;
        [SerializeField] private float _nukeDistance = 100f;
        [SerializeField] private float _nukeScale = 100f;

        [Header("UI")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private CanvasGroup _titleCanvasGroup;
        [SerializeField] private Text _titleText;

        [Header("Lighting")]
        [SerializeField] private Light _directionalLight;
        [SerializeField] private float _blastLightIntensity = 8f;

        [Header("Audio")]
        [SerializeField] private AudioSource _sfxAudio;
        [SerializeField] private AudioClip _explosionClip;
        [SerializeField] private AudioClip _screamClip;
        [SerializeField] private AudioClip _windBlastClip;

        [Header("Timing")]
        [SerializeField] private float _walkSpeed = 1.5f;
        [SerializeField] private float _walkDuration = 5f;
        [SerializeField] private float _pauseBeforeBlast = 3f;
        [SerializeField] private float _blastFlashDuration = 0.4f;
        [SerializeField] private float _shakeDuration = 2.5f;
        [SerializeField] private float _shakeMagnitude = 0.4f;
        [SerializeField] private float _fadeToBlackDuration = 1.5f;
        [SerializeField] private float _titleFadeInDuration = 1.5f;
        [SerializeField] private float _titleHoldDuration = 3f;
        [SerializeField] private float _titleFadeOutDuration = 1.5f;
        [SerializeField] private float _preTransitionDelay = 1f;

        [Header("Scene Transition")]
        [SerializeField] private string _bunkerSceneName = "Bunker";

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Prologue);

            _fadeCanvasGroup.alpha = 0f;
            _titleCanvasGroup.alpha = 0f;

            // Hide character model — first-person POV
            if (_characterTransform != null)
                foreach (var r in _characterTransform.GetComponentsInChildren<Renderer>())
                    r.enabled = false;

            StartCoroutine(PlayPrologue());
        }

        private void LateUpdate()
        {
            if (!_cameraFollowing || _cinematicCamera == null || _characterTransform == null) return;

            _cinematicCamera.transform.position = _characterTransform.position + Vector3.up * _eyeHeight;
            _cinematicCamera.transform.rotation = _characterTransform.rotation;
        }

        private IEnumerator PlayPrologue()
        {
            // 1) Walk outside
            yield return StartCoroutine(WalkSequence());

            // 2) Pause — stand still, looking at the horizon
            yield return new WaitForSeconds(_pauseBeforeBlast);

            // 3) Nuke blast
            yield return StartCoroutine(BlastSequence());

            // 4) Fade to black
            yield return StartCoroutine(FadeToBlack(_fadeToBlackDuration));

            // 5) Title card "THE ORDER"
            yield return StartCoroutine(TitleSequence());

            // 6) Transition to bunker
            yield return new WaitForSeconds(_preTransitionDelay);
            TransitionToBunker();
        }

        #region Sequence Phases

        private IEnumerator WalkSequence()
        {
            if (_characterAnimator != null)
                _characterAnimator.SetBool("IsWalking", true);

            float elapsed = 0f;
            int waypointIndex = 0;

            while (elapsed < _walkDuration && _walkWaypoints != null && waypointIndex < _walkWaypoints.Length)
            {
                Transform target = _walkWaypoints[waypointIndex];
                Vector3 direction = (target.position - _characterTransform.position).normalized;

                if (direction.sqrMagnitude > 0.01f)
                    _characterTransform.rotation = Quaternion.Slerp(
                        _characterTransform.rotation,
                        Quaternion.LookRotation(direction),
                        Time.deltaTime * 5f
                    );

                _characterTransform.position = Vector3.MoveTowards(
                    _characterTransform.position,
                    target.position,
                    _walkSpeed * Time.deltaTime
                );

                if (Vector3.Distance(_characterTransform.position, target.position) < 0.2f)
                    waypointIndex++;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_characterAnimator != null)
                _characterAnimator.SetBool("IsWalking", false);
        }

        private IEnumerator BlastSequence()
        {
            // Spawn nuke VFX in front of the camera
            if (_nukePrefab != null)
            {
                Vector3 spawnPos;
                if (_nukeSpawnPoint != null)
                {
                    spawnPos = _nukeSpawnPoint.position;
                }
                else
                {
                    spawnPos = _cinematicCamera.transform.position
                             + _cinematicCamera.transform.forward * _nukeDistance;
                    spawnPos.y = 0f;
                }
                GameObject nuke = Instantiate(_nukePrefab, spawnPos, Quaternion.identity);
                nuke.transform.localScale = Vector3.one * _nukeScale;
            }

            // Explosion sound
            PlayClip(_sfxAudio, _explosionClip);

            // Flash screen white
            _fadeCanvasGroup.alpha = 1f;

            // Blast light surge
            float originalIntensity = 1f;
            if (_directionalLight != null)
            {
                originalIntensity = _directionalLight.intensity;
                _directionalLight.intensity = _blastLightIntensity;
            }

            yield return new WaitForSeconds(_blastFlashDuration);

            // Fade flash out while shaking camera
            _cameraFollowing = false;

            // Scream
            PlayClip(_sfxAudio, _screamClip);

            float elapsed = 0f;
            Vector3 originalCamPos = _cinematicCamera.transform.position;
            Quaternion originalCamRot = _cinematicCamera.transform.rotation;

            while (elapsed < _shakeDuration)
            {
                float t = elapsed / _shakeDuration;

                // Fade white flash out over first half of shake
                if (t < 0.5f)
                {
                    _fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t * 2f);
                    if (_directionalLight != null)
                        _directionalLight.intensity = Mathf.Lerp(_blastLightIntensity, originalIntensity, t * 2f);
                }
                else
                {
                    _fadeCanvasGroup.alpha = 0f;
                    if (_directionalLight != null)
                        _directionalLight.intensity = originalIntensity;
                }

                // Camera shake — intensifies over time
                float shakeAmount = _shakeMagnitude * (1f + t);
                float x = Random.Range(-1f, 1f) * shakeAmount;
                float y = Random.Range(-1f, 1f) * shakeAmount;
                _cinematicCamera.transform.position = originalCamPos + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Wind blast sound
            PlayClip(_sfxAudio, _windBlastClip);

            // Reset camera before fall
            _cinematicCamera.transform.position = originalCamPos;
            _cinematicCamera.transform.rotation = originalCamRot;

            _fadeCanvasGroup.alpha = 0f;
            if (_directionalLight != null)
                _directionalLight.intensity = originalIntensity;

            // Brief delay then collapse
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(FallSequence(originalCamPos, originalCamRot));
        }

        private IEnumerator TitleSequence()
        {
            // Fade in "THE ORDER"
            yield return StartCoroutine(FadeCanvasGroup(_titleCanvasGroup, 0f, 1f, _titleFadeInDuration));

            // Hold
            yield return new WaitForSeconds(_titleHoldDuration);

            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(_titleCanvasGroup, 1f, 0f, _titleFadeOutDuration));
        }

        #endregion

        #region Utilities

        private IEnumerator FallSequence(Vector3 startPos, Quaternion startRot)
        {
            float fallDuration = 0.8f;
            float groundY = 0.3f;
            float elapsed = 0f;

            // Camera tilts forward and drops — like collapsing face-first
            while (elapsed < fallDuration)
            {
                float t = elapsed / fallDuration;
                // Accelerate downward (ease-in curve)
                float fallT = t * t;

                // Drop height
                float y = Mathf.Lerp(startPos.y, groundY, fallT);
                _cinematicCamera.transform.position = new Vector3(startPos.x, y, startPos.z);

                // Tilt camera forward (face hitting the ground)
                float tiltAngle = Mathf.Lerp(0f, 70f, fallT);
                _cinematicCamera.transform.rotation = startRot * Quaternion.Euler(tiltAngle, 0f, Random.Range(-5f, 5f) * t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Final position — on the ground, looking sideways
            _cinematicCamera.transform.position = new Vector3(startPos.x, groundY, startPos.z);
            _cinematicCamera.transform.rotation = startRot * Quaternion.Euler(70f, 0f, 15f);

            // Brief hold on the ground before fade
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator FadeToBlack(float duration)
        {
            yield return StartCoroutine(FadeCanvasGroup(_fadeCanvasGroup, 0f, 1f, duration));
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                group.alpha = Mathf.Lerp(from, to, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            group.alpha = to;
        }

        private void PlayClip(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip);
        }

        private void TransitionToBunker()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoadScene(_bunkerSceneName);
        }

        #endregion
    }
}
