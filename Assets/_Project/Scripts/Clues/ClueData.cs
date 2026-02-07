using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// ScriptableObject defining a single clue's data.
    /// 17 instances total across 3 categories: Truth, Mike, Weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClue", menuName = "The Order/Clue Data")]
    public class ClueData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private ClueCategory _category;
        [SerializeField] private string _title;

        [Header("Content")]
        [SerializeField] [TextArea(3, 10)] private string _contentText;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private AudioClip _audioClip;

        [Header("Gameplay")]
        [SerializeField] private float _sanityImpact;

        /// <summary>Unique identifier for this clue.</summary>
        public string Id => _id;

        /// <summary>Which category this clue belongs to (Truth, Mike, Weapon).</summary>
        public ClueCategory Category => _category;

        /// <summary>Display title shown in journal and pickup notification.</summary>
        public string Title => _title;

        /// <summary>Full text content displayed when viewing the clue.</summary>
        public string ContentText => _contentText;

        /// <summary>Visual representation of the clue (document, photo, etc).</summary>
        public Sprite Sprite => _sprite;

        /// <summary>Optional audio clip for audio log clues. Null if not an audio clue.</summary>
        public AudioClip AudioClip => _audioClip;

        /// <summary>
        /// Sanity impact on collection. Negative = drain (disturbing clues), positive = recovery (Mike clues).
        /// </summary>
        public float SanityImpact => _sanityImpact;
    }
}
