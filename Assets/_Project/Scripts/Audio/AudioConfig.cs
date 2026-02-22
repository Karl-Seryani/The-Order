using TheOrder.Items;
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

        #region Interaction

        [Header("Interaction")]
        [SerializeField] private AudioClip _doorCreakClip;
        [SerializeField] private AudioClip _furnitureSlideClip;
        [SerializeField] [Range(0f, 1f)] private float _interactionMaxVolume = 0.6f;

        /// <summary>Looping creak clip for doors being held open/closed.</summary>
        public AudioClip DoorCreakClip => _doorCreakClip;

        /// <summary>Looping slide clip for furniture being held open/closed.</summary>
        public AudioClip FurnitureSlideClip => _furnitureSlideClip;

        /// <summary>Max volume for interaction sounds (scales with loudness).</summary>
        public float InteractionMaxVolume => _interactionMaxVolume;

        #endregion

        #region Stingers

        [Header("Wake-Up Stinger")]
        [SerializeField] private AudioClip _wakeUpStingerClip;
        [SerializeField] [Range(0f, 1f)] private float _wakeUpStingerVolume = 0.5f;
        [SerializeField] private float _wakeUpStingerDuration = 10f;

        [Header("Chase Music")]
        [SerializeField] private AudioClip _chaseMusicClip;
        [SerializeField] [Range(0f, 1f)] private float _chaseMusicVolume = 0.4f;

        [Header("Second Key Stinger")]
        [SerializeField] private AudioClip _secondKeyStingerClip;
        [SerializeField] [Range(0f, 1f)] private float _secondKeyStingerVolume = 0.5f;
        [SerializeField] private float _secondKeyStingerDuration = 10f;

        [Header("Machine Room Key Stinger")]
        [SerializeField] private AudioClip _machineRoomKeyStingerClip;
        [SerializeField] [Range(0f, 1f)] private float _machineRoomKeyStingerVolume = 0.5f;
        [SerializeField] private float _machineRoomKeyStingerDuration = 15f;
        [SerializeField] private ItemData _machineRoomKeyItemData;

        [Header("Random Stinger")]
        [SerializeField] private AudioClip _randomStingerClip;
        [SerializeField] [Range(0f, 1f)] private float _randomStingerVolume = 0.4f;
        [SerializeField] private float _randomStingerDuration = 5f;
        [SerializeField] private float _randomStingerMinInterval = 60f;
        [SerializeField] private float _randomStingerMaxInterval = 60f;

        /// <summary>Clip played during wake-up sequence.</summary>
        public AudioClip WakeUpStingerClip => _wakeUpStingerClip;
        public float WakeUpStingerVolume => _wakeUpStingerVolume;
        public float WakeUpStingerDuration => _wakeUpStingerDuration;

        /// <summary>Music that loops during Hunter chase.</summary>
        public AudioClip ChaseMusicClip => _chaseMusicClip;
        public float ChaseMusicVolume => _chaseMusicVolume;

        /// <summary>Clip played when the second key is collected.</summary>
        public AudioClip SecondKeyStingerClip => _secondKeyStingerClip;
        public float SecondKeyStingerVolume => _secondKeyStingerVolume;
        public float SecondKeyStingerDuration => _secondKeyStingerDuration;

        /// <summary>Clip played when the Machine Room Key is picked up.</summary>
        public AudioClip MachineRoomKeyStingerClip => _machineRoomKeyStingerClip;
        public float MachineRoomKeyStingerVolume => _machineRoomKeyStingerVolume;
        public float MachineRoomKeyStingerDuration => _machineRoomKeyStingerDuration;
        public ItemData MachineRoomKeyItemData => _machineRoomKeyItemData;

        /// <summary>Clip played at random intervals.</summary>
        public AudioClip RandomStingerClip => _randomStingerClip;
        public float RandomStingerVolume => _randomStingerVolume;
        public float RandomStingerDuration => _randomStingerDuration;
        public float RandomStingerMinInterval => _randomStingerMinInterval;
        public float RandomStingerMaxInterval => _randomStingerMaxInterval;

        #endregion

        #region Outdoor / Indoor Ambient

        [Header("Outdoor Ambient")]
        [SerializeField] private AudioClip[] _outdoorAmbientClips;
        [SerializeField] [Range(0f, 1f)] private float _outdoorAmbientVolume = 0.3f;

        [Header("Indoor Re-entry One-Shot")]
        [SerializeField] private AudioClip _indoorAmbientOneShot;
        [SerializeField] [Range(0f, 1f)] private float _indoorOneShotVolume = 0.3f;

        /// <summary>Random forest ambient clips for the outdoor area.</summary>
        public AudioClip[] OutdoorAmbientClips => _outdoorAmbientClips;
        public float OutdoorAmbientVolume => _outdoorAmbientVolume;

        /// <summary>One-shot ambient clip played on re-entering the bunker.</summary>
        public AudioClip IndoorAmbientOneShot => _indoorAmbientOneShot;
        public float IndoorOneShotVolume => _indoorOneShotVolume;

        #endregion

        #region Footsteps

        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _walkFootstepClips;
        [SerializeField] private AudioClip[] _sprintFootstepClips;
        [SerializeField] [Range(0f, 1f)] private float _footstepVolume = 0.5f;
        [SerializeField] private float _walkFootstepInterval = 0.5f;
        [SerializeField] private float _sprintFootstepInterval = 0.35f;
        [SerializeField] private float _sprintSpeedThreshold = 4.5f;

        /// <summary>Walk footstep clips for random selection.</summary>
        public AudioClip[] WalkFootstepClips => _walkFootstepClips;

        /// <summary>Sprint footstep clips for random selection.</summary>
        public AudioClip[] SprintFootstepClips => _sprintFootstepClips;

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
