using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Hidden stamina system — no UI representation.
    /// Drains while sprinting, regenerates after a delay when not sprinting.
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        #region Constants

        private const float MAX_STAMINA = 100f;
        private const float SPRINT_DRAIN_RATE = 20f;
        private const float REGEN_RATE = 10f;
        private const float REGEN_DELAY = 1.5f;
        private const float MIN_SPRINT_THRESHOLD = 10f;

        #endregion

        #region State

        private float _currentStamina = MAX_STAMINA;
        private float _regenDelayTimer;
        private bool _isSprinting;

        #endregion

        #region Public API

        /// <summary>Current stamina value (0 to MaxStamina).</summary>
        public float CurrentStamina => _currentStamina;

        /// <summary>Maximum stamina value.</summary>
        public float MaxStamina => MAX_STAMINA;

        /// <summary>True if the player has enough stamina to start or continue sprinting.</summary>
        public bool CanSprint => _currentStamina >= MIN_SPRINT_THRESHOLD;

        /// <summary>True while actively sprinting.</summary>
        public bool IsSprinting => _isSprinting;

        /// <summary>
        /// Set sprint state. When sprinting, stamina drains; when not, regen delay begins.
        /// </summary>
        public void SetSprinting(bool sprinting)
        {
            if (sprinting && !CanSprint) return;

            if (sprinting && !_isSprinting)
            {
                _isSprinting = true;
                _regenDelayTimer = REGEN_DELAY;
            }
            else if (!sprinting && _isSprinting)
            {
                _isSprinting = false;
                _regenDelayTimer = REGEN_DELAY;
            }
        }

        /// <summary>Drain stamina by a specific amount (for testability).</summary>
        public void DrainStamina(float amount)
        {
            _currentStamina = Mathf.Max(0f, _currentStamina - amount);
        }

        /// <summary>Calculate drain for a given delta time (pure math, for testing).</summary>
        public static float CalculateDrain(float deltaTime)
        {
            return SPRINT_DRAIN_RATE * deltaTime;
        }

        /// <summary>Calculate regen for a given delta time (pure math, for testing).</summary>
        public static float CalculateRegen(float deltaTime)
        {
            return REGEN_RATE * deltaTime;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (_isSprinting)
            {
                DrainStamina(CalculateDrain(Time.deltaTime));

                if (_currentStamina <= 0f)
                {
                    _isSprinting = false;
                    _regenDelayTimer = REGEN_DELAY;
                }
            }
            else
            {
                _regenDelayTimer -= Time.deltaTime;

                if (_regenDelayTimer <= 0f)
                {
                    _currentStamina = Mathf.Min(MAX_STAMINA, _currentStamina + CalculateRegen(Time.deltaTime));
                }
            }
        }

        #endregion
    }
}
