using UnityEngine;

namespace Ouiki.FPS
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class MovementController : MonoBehaviour
    {
        private Rigidbody rb;
        private CapsuleCollider standingCollider;
        private CapsuleCollider crouchCollider;
        private PlayerManager manager;
        private PlayerInputHandler input;
        private PlayerStateManager state;
        private CooldownManager cooldown;

        public float walkSpeed = 5f;
        public float sprintSpeed = 8f;
        public float crouchSpeed = 2.5f;
        public float jumpPower = 5f;
        public CapsuleCollider standingCapsule;
        public CapsuleCollider crouchCapsule;
        public Transform playerVisual;

        private bool isGrounded;
        private float lastGroundCheckDistance = 0.3f;
        private Vector3 lastGroundCheckOrigin;
        private Vector3 lastGroundCheckDirection;
        private bool _initialized = false;

        public void Init(PlayerManager mgr)
        {
            manager = mgr;
            input = manager.inputHandler;
            state = manager.stateManager;
            cooldown = manager.cooldownManager;
            rb = GetComponent<Rigidbody>();

            standingCollider = standingCapsule;
            crouchCollider = crouchCapsule;

            EnableCollider(true);
            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;
            if (!state.CanDo(PlayerAction.Move)) return;

            float speed = walkSpeed;
            if (state.CurrentState == PlayerState.Sprinting && cooldown.CanSprint) speed = sprintSpeed;
            if (state.CurrentState == PlayerState.Crouching) speed = crouchSpeed;

            Vector3 moveInput = new Vector3(input.MoveInput.Value.x, 0f, input.MoveInput.Value.y);
            Vector3 moveDir = transform.TransformDirection(moveInput);

            Vector3 velocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed);
            rb.linearVelocity = velocity;

            if (state.CanDo(PlayerAction.Jump) && input.JumpPressed.Value && isGrounded && cooldown.CanJump)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpPower, rb.linearVelocity.z);
                state.StartJump();
                cooldown.StartJumpCooldown();
            }

            cooldown.isSprinting = state.CurrentState == PlayerState.Sprinting && input.SprintHeld.Value && cooldown.CanSprint;
            cooldown.TickStamina(Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (!_initialized) return;
            CheckGround();

            if (state.IsKnockedOut)
                return;

            if (input.CrouchPressed.Value)
            {
                input.ResetCrouch();
                if (state.CurrentState != PlayerState.Crouching)
                {
                    state.ToggleCrouch();
                    EnableCollider(false);
                }
                else
                {
                    if (CanStandUp())
                    {
                        state.ToggleCrouch();
                        EnableCollider(true);
                    }
                }
            }

            if (input.SprintHeld.Value && state.CanDo(PlayerAction.Sprint) && cooldown.CanSprint)
                state.StartSprint();
            else if ((!input.SprintHeld.Value || !cooldown.CanSprint) && state.CurrentState == PlayerState.Sprinting)
                state.StopSprint();

            if (isGrounded && state.CurrentState == PlayerState.Jumping)
                state.Land();
        }

        private void EnableCollider(bool standing)
        {
            if (standingCollider && crouchCollider)
            {
                standingCollider.enabled = standing;
                crouchCollider.enabled = !standing;
            }
        }

        private void CheckGround()
        {
            lastGroundCheckOrigin = transform.position + Vector3.up * 0.1f;
            lastGroundCheckDirection = Vector3.down;
            CapsuleCollider activeCol = standingCollider.enabled ? standingCollider : crouchCollider;
            float checkDistance = activeCol.height / 2f + lastGroundCheckDistance;
            isGrounded = Physics.Raycast(lastGroundCheckOrigin, lastGroundCheckDirection, checkDistance);
        }

        private bool CanStandUp()
        {
            if (!standingCollider || !crouchCollider) return true;
            float checkDistance = standingCollider.height - crouchCollider.height;
            Vector3 headPos = transform.position + Vector3.up * crouchCollider.height;
            return !Physics.Raycast(headPos, Vector3.up, checkDistance);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(lastGroundCheckOrigin, lastGroundCheckOrigin + lastGroundCheckDirection * lastGroundCheckDistance);
            if (isGrounded)
                Gizmos.DrawSphere(lastGroundCheckOrigin + lastGroundCheckDirection * lastGroundCheckDistance, 0.05f);
        }
    }
}