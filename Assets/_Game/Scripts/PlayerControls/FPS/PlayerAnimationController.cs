using UnityEngine;

namespace Ouiki.FPS
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        public Animator animator; 

        public void PlayStateAnimation(PlayerState state, bool isWalking)
        {
            switch (state)
            {
                case PlayerState.Standing:
                    animator.Play(isWalking ? "Walk" : "Idle");
                    break;
                case PlayerState.Crouching:
                    animator.Play("Crouch");
                    break;
                case PlayerState.Sprinting:
                    animator.Play("Sprint");
                    break;
                case PlayerState.Jumping:
                    animator.Play("Jump");
                    break;
                case PlayerState.KnockedOut:
                    animator.Play("KnockedOut");
                    break;
                case PlayerState.Hiding:
                    animator.Play("Hide");
                    break;
                case PlayerState.Pushing:
                    animator.Play("Push");
                    break;
            }
        }
    }
}