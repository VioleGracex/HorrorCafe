using Ouiki.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Ouiki.FPS
{
    public enum PlayerAction
    {
        Move,
        Jump,
        Crouch,
        Sprint,
        Zoom,
        Hide,
        Push
    }

    public enum PlayerState
    {
        Standing,
        Crouching,
        Sprinting,
        Jumping,
        KnockedOut,
        Hiding,
        Pushing,
        Dead,
        Grabbed
    }

    public class PlayerCapabilities
    {
        public bool CanMove;
        public bool CanJump;
        public bool CanCrouch;
        public bool CanSprint;
        public bool CanZoom;
        public bool CanHide;
        public bool CanPush;

        public bool CanDo(PlayerAction action)
        {
            return action switch
            {
                PlayerAction.Move => CanMove,
                PlayerAction.Jump => CanJump,
                PlayerAction.Crouch => CanCrouch,
                PlayerAction.Sprint => CanSprint,
                PlayerAction.Zoom => CanZoom,
                PlayerAction.Hide => CanHide,
                PlayerAction.Push => CanPush,
                _ => false
            };
        }
    }

    public class PlayerStateManager : MonoBehaviour
    {
        public PlayerState CurrentState { get; private set; } = PlayerState.Standing;
        public PlayerAnimationController animationController;
        private bool _lastIsWalking = false;

        private readonly Dictionary<PlayerState, PlayerCapabilities> stateCapabilities = new()
        {
            [PlayerState.Standing] = new PlayerCapabilities
            {
                CanMove = true,
                CanJump = true,
                CanCrouch = true,
                CanSprint = true,
                CanZoom = true,
                CanHide = true,
                CanPush = true
            },
            [PlayerState.Crouching] = new PlayerCapabilities
            {
                CanMove = true,
                CanJump = false,
                CanCrouch = true,
                CanSprint = false,
                CanZoom = true,
                CanHide = true,
                CanPush = true
            },
            [PlayerState.Sprinting] = new PlayerCapabilities
            {
                CanMove = true,
                CanJump = true,
                CanCrouch = true,
                CanSprint = true,
                CanZoom = false,
                CanHide = false,
                CanPush = false
            },
            [PlayerState.Jumping] = new PlayerCapabilities
            {
                CanMove = true,
                CanJump = false,
                CanCrouch = false,
                CanSprint = false,
                CanZoom = false,
                CanHide = false,
                CanPush = false
            },
            [PlayerState.KnockedOut] = new PlayerCapabilities
            {
                CanMove = false,
                CanJump = false,
                CanCrouch = false,
                CanSprint = false,
                CanZoom = false,
                CanHide = false,
                CanPush = false
            },
            [PlayerState.Hiding] = new PlayerCapabilities
            {
                CanMove = false,
                CanJump = false,
                CanCrouch = false,
                CanSprint = false,
                CanZoom = false,
                CanHide = true,
                CanPush = false
            },
            [PlayerState.Pushing] = new PlayerCapabilities
            {
                CanMove = true,
                CanJump = false,
                CanCrouch = false,
                CanSprint = false,
                CanZoom = false,
                CanHide = false,
                CanPush = true
            },
            [PlayerState.Dead] = new PlayerCapabilities
            {
                CanMove = false,
                CanJump = false,
                CanCrouch = false,
                CanSprint = false,
                CanZoom = false,
                CanHide = false,
                CanPush = false
            },
            [PlayerState.Grabbed] = new PlayerCapabilities
            {
                CanMove = false,
                CanJump = false,
                CanCrouch = false,
                CanSprint = false,
                CanZoom = true,
                CanHide = false,
                CanPush = false
            }
        };

        public bool IsKnockedOut => CurrentState == PlayerState.KnockedOut;
        public bool IsDead => CurrentState == PlayerState.Dead;
        public bool IsGrabbed => CurrentState == PlayerState.Grabbed;

        public bool CanDo(PlayerAction action)
        {
            if (stateCapabilities.TryGetValue(CurrentState, out var capabilities))
                return capabilities.CanDo(action);
            return false;
        }

        public void UpdateWalkAnim(bool isWalking)
        {
            if (animationController != null && (CurrentState == PlayerState.Standing))
            {
                if (_lastIsWalking != isWalking)
                {
                    animationController.PlayStateAnimation(CurrentState, isWalking);
                    _lastIsWalking = isWalking;
                }
            }
        }

        public void KnockOut() => SetState(PlayerState.KnockedOut);
        public void Recover() => SetState(PlayerState.Standing);

        public void Die()
        {
            SetState(PlayerState.Dead);
            MusicManager.Instance?.StopChaseMusic();
            // Show Game Over UI
            if (GameOverCanvasManager.Instance != null)
                GameOverCanvasManager.Instance.ShowGameOver("You Died!");
        }

        public void GetGrabbed() => SetState(PlayerState.Grabbed);
        public void StartHide() => SetState(PlayerState.Hiding);
        public void StopHide() => SetState(PlayerState.Standing);
        public void StartPush() => SetState(PlayerState.Pushing);
        public void StopPush() => SetState(PlayerState.Standing);

        public void ToggleCrouch()
        {
            if (CurrentState == PlayerState.Crouching)
                SetState(PlayerState.Standing);
            else if (CanDo(PlayerAction.Crouch))
                SetState(PlayerState.Crouching);
        }

        public void StartSprint()
        {
            if (CanDo(PlayerAction.Sprint))
                SetState(PlayerState.Sprinting);
        }

        public void StopSprint()
        {
            if (CurrentState == PlayerState.Sprinting)
                SetState(PlayerState.Standing);
        }

        public void StartJump()
        {
            if (CanDo(PlayerAction.Jump))
                SetState(PlayerState.Jumping);
        }

        public void Land()
        {
            if (!IsKnockedOut && !IsDead && !IsGrabbed)
                SetState(PlayerState.Standing);
        }

        public void SetState(PlayerState newState)
        {
            if (CurrentState == PlayerState.Dead) return;
            CurrentState = newState;
            if (animationController != null)
            {
                bool isWalking = false;
                if (CurrentState == PlayerState.Standing)
                    isWalking = _lastIsWalking;
                animationController.PlayStateAnimation(newState, isWalking);
            }
        }
    }
}