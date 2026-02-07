using UnityEngine;

namespace TheOrder.Audio
{
    /// <summary>
    /// ScriptableObject holding all audio configuration for the bunker.
    /// Clips, volumes, and timing are tunable from the inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "The Order/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        #region Ambient

        [Header("Ambient")]
        [SerializeField] private AudioClip _ambientLoopClip;
        [SerializeField] [Range(0f, 1f)] private float _ambientVolume = 0.3f;

        /// <summary>Looping ambient clip for the bunker atmosphere.</summary>
        public AudioClip AmbientLoopClip => _ambientLoopClip;

        /// <summary>Volume for the ambient loop.</summary>
        public float AmbientVolume => _ambientVolume;

        #endregion

        #region Doors

        [Header("Doors")]
        [SerializeField] private AudioClip _doorOpenClip;
        [SerializeField] private AudioClip _doorCloseClip;
        [SerializeField] [Range(0f, 1f)] private float _doorVolume = 0.7f;

        /// <summary>SFX played when a door opens.</summary>
        public AudioClip DoorOpenClip => _doorOpenClip;

        /// <summary>SFX played when a door closes.</summary>
        public AudioClip DoorCloseClip => _doorCloseClip;

        /// <summary>Volume for door SFX.</summary>
        public float DoorVolume => _doorVolume;

        #endregion

        #region Footsteps

        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _footstepClips;
        [SerializeField] [Range(0f, 1f)] private float _footstepVolume = 0.5f;
        [SerializeField] private float _walkFootstepInterval = 0.5f;
        [SerializeField] private float _sprintFootstepInterval = 0.35f;
        [SerializeField] private float _sprintSpeedThreshold = 4.5f;

        /// <summary>Array of footstep clips for random selection.</summary>
        public AudioClip[] FootstepClips => _footstepClips;

        /// <summary>Volume for footstep SFX.</summary>
        public float FootstepVolume => _footstepVolume;

        /// <summary>Time between footstep sounds while walking.</summary>
        public float WalkFootstepInterval => _walkFootstepInterval;

        /// <summary>Time between footstep sounds while sprinting.</summary>
        public float SprintFootstepInterval => _sprintFootstepInterval;

        /// <summary>Speed above which the player is considered sprinting for footstep timing.</summary>
        public float SprintSpeedThreshold => _sprintSpeedThreshold;

        #endregion
    }
}
