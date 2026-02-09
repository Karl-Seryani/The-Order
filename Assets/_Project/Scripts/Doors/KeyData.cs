using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// ScriptableObject defining a key that unlocks a specific LockedDoor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewKey", menuName = "The Order/Key Data")]
    public class KeyData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] [TextArea(2, 4)] private string _description;

        /// <summary>Unique identifier for this key.</summary>
        public string Id => _id;

        /// <summary>Name shown in UI notifications.</summary>
        public string DisplayName => _displayName;

        /// <summary>Optional flavor text.</summary>
        public string Description => _description;
    }
}
