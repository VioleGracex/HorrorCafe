using UnityEngine;
using System.Collections;

namespace Ouiki.FPS
{
    public class CooldownManager : MonoBehaviour
    {
        [Header("Stamina (Sprint)")]
        public float staminaDuration = 3f;
        public float staminaRegenRate = 0.5f;
        public float staminaRemaining { get; private set; }
        public float sprintCostPerSecond = 1f;
        public bool isSprinting { get; set; }

        [Header("Sprint Cooldown")]
        public float sprintCooldown = 1.5f;
        public float sprintCooldownTimer { get; private set; }
        public bool sprintOnCooldown { get; private set; }
        public float sprintCooldownReset => sprintCooldown;

        [Header("Jump Cooldown")]
        public float jumpCooldown = 1f;
        public float jumpCooldownTimer { get; private set; }
        public bool jumpOnCooldown { get; private set; }

        private Coroutine sprintCooldownRoutine;
        private Coroutine jumpCooldownRoutine;

        public void Init()
        {
            staminaRemaining = staminaDuration;
            sprintCooldownTimer = 0f;
            sprintOnCooldown = false;
            isSprinting = false;
            jumpCooldownTimer = 0f;
            jumpOnCooldown = false;
        }

        public void TickStamina(float deltaTime)
        {
            if (isSprinting && !sprintOnCooldown && staminaRemaining > 0f)
            {
                staminaRemaining = Mathf.Max(0f, staminaRemaining - sprintCostPerSecond * deltaTime);
                if (staminaRemaining == 0f)
                {
                    StartSprintCooldown();
                }
            }

            else if (sprintOnCooldown && staminaRemaining < staminaDuration)
            {
                staminaRemaining = Mathf.Min(staminaDuration, staminaRemaining + staminaRegenRate * deltaTime);

                if (staminaRemaining == staminaDuration)
                {
                    sprintOnCooldown = false;
                    sprintCooldownTimer = 0f;
                }
            }

            else if (!isSprinting && !sprintOnCooldown && staminaRemaining < staminaDuration)
            {
                staminaRemaining = Mathf.Min(staminaDuration, staminaRemaining + staminaRegenRate * deltaTime);
            }
        }

        public void UseSprint(float amount)
        {
            staminaRemaining = Mathf.Max(0f, staminaRemaining - amount);
            if (staminaRemaining == 0f && !sprintOnCooldown)
            {
                StartSprintCooldown();
            }
        }

        public void RechargeSprint(float amount)
        {
            staminaRemaining = Mathf.Min(staminaDuration, staminaRemaining + amount);
            if (sprintOnCooldown && staminaRemaining == staminaDuration)
            {
                sprintOnCooldown = false;
                sprintCooldownTimer = 0f;
            }
        }

        public bool CanSprint => staminaRemaining > 0f && !sprintOnCooldown;
        public bool CanJump => !jumpOnCooldown;

        public void StartSprintCooldown()
        {
            if (!sprintOnCooldown)
            {
                if (sprintCooldownRoutine != null) StopCoroutine(sprintCooldownRoutine);
                sprintCooldownRoutine = StartCoroutine(SprintCooldownCoroutine());
            }
        }

        private IEnumerator SprintCooldownCoroutine()
        {
            sprintOnCooldown = true;
            sprintCooldownTimer = sprintCooldown;
            while (sprintCooldownTimer > 0f)
            {
                yield return new WaitForFixedUpdate();
                sprintCooldownTimer -= Time.fixedDeltaTime;
            }
            sprintCooldownTimer = 0f;
        }

        public void StartJumpCooldown()
        {
            if (!jumpOnCooldown)
            {
                if (jumpCooldownRoutine != null) StopCoroutine(jumpCooldownRoutine);
                jumpCooldownRoutine = StartCoroutine(JumpCooldownCoroutine());
            }
        }

        private IEnumerator JumpCooldownCoroutine()
        {
            jumpOnCooldown = true;
            jumpCooldownTimer = jumpCooldown;
            while (jumpCooldownTimer > 0f)
            {
                yield return new WaitForFixedUpdate();
                jumpCooldownTimer -= Time.fixedDeltaTime;
            }
            jumpOnCooldown = false;
            jumpCooldownTimer = 0f;
        }
    }
}