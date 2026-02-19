using System;
using System.Collections;
using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// A screw that can be unscrewed with the correct tool.
    /// Plays a programmatic unscrew animation (rotate + back out) then deactivates.
    /// </summary>
    public class ScrewInteractable : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Required Item")]
        [SerializeField] private ItemData _requiredItem;

        [Header("Unscrew Animation")]
        [SerializeField] private float _unscrewDuration = 0.8f;
        [SerializeField] private float _unscrewRotations = 3f;
        [SerializeField] private float _backOutDistance = 0.15f;

        #endregion

        #region Private Fields

        private bool _isUnscrewing;
        private bool _isUnscrewed;

        #endregion

        #region Events

        /// <summary>Fired when this screw finishes unscrewing.</summary>
        public event Action<ScrewInteractable> Unscrewed;

        #endregion

        #region IInteractable

        /// <summary>Unscrew if holding the correct tool.</summary>
        public void Interact(GameObject interactor)
        {
            if (_isUnscrewed || _isUnscrewing) return;

            var heldItem = interactor.GetComponent<HeldItemController>();
            if (heldItem == null || !heldItem.HasItem || heldItem.CurrentItem != _requiredItem)
                return;

            StartCoroutine(UnscrewAnimation());
        }

        /// <summary>Context-aware prompt.</summary>
        public string GetPromptText()
        {
            if (_isUnscrewed) return string.Empty;

            var heldItem = HeldItemController.Instance;
            if (heldItem != null && heldItem.HasItem && heldItem.CurrentItem == _requiredItem)
                return "Unscrew";

            if (_requiredItem != null)
                return $"Requires {_requiredItem.DisplayName}";

            return "Requires tool";
        }

        #endregion

        #region Animation

        private IEnumerator UnscrewAnimation()
        {
            _isUnscrewing = true;

            Vector3 startPos = transform.localPosition;
            Quaternion startRot = transform.localRotation;

            // Back out along the screw's local forward axis
            Vector3 backOutWorld = transform.forward * _backOutDistance;
            Vector3 endPos = transform.localPosition
                + (transform.parent != null
                    ? transform.parent.InverseTransformVector(backOutWorld)
                    : backOutWorld);

            float elapsed = 0f;
            while (elapsed < _unscrewDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _unscrewDuration);

                // Rotate around local forward
                float angle = t * _unscrewRotations * 360f;
                transform.localRotation = startRot * Quaternion.AngleAxis(angle, Vector3.forward);

                // Translate outward
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);

                yield return null;
            }

            _isUnscrewed = true;
            _isUnscrewing = false;
            Unscrewed?.Invoke(this);
            gameObject.SetActive(false);
        }

        #endregion
    }
}
