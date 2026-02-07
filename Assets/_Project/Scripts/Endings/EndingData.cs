using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// ScriptableObject defining a single ending's data.
    /// 9 instances total: 3 knowledge levels x 3 choices.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnding", menuName = "The Order/Ending Data")]
    public class EndingData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private EndingType _endingType;
        [SerializeField] private string _endingName;

        [Header("Determination")]
        [SerializeField] private KnowledgeLevel _knowledgeLevel;
        [SerializeField] private EndingChoice _choice;

        [Header("Content")]
        [SerializeField] [TextArea(5, 15)] private string _narrativeText;

        /// <summary>Enum identifier for this ending.</summary>
        public EndingType EndingType => _endingType;

        /// <summary>Display name (e.g. "Blind Violence", "Absolution").</summary>
        public string EndingName => _endingName;

        /// <summary>Required knowledge level to trigger this ending.</summary>
        public KnowledgeLevel KnowledgeLevel => _knowledgeLevel;

        /// <summary>Which final choice leads to this ending.</summary>
        public EndingChoice Choice => _choice;

        /// <summary>Narrative text displayed during the ending sequence.</summary>
        public string NarrativeText => _narrativeText;
    }
}
