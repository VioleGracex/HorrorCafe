using UnityEngine;

namespace Ouiki.FPS
{
    public class PlayerManager : MonoBehaviour
    {
        public PlayerInputHandler inputHandler;
        public MovementController movementController;
        public CameraController cameraController;
        public PlayerStateManager stateManager;
        public PlayerUIController uiController;
        public CooldownManager cooldownManager;
        public PlayerInteractionController interactionController;
        public PlayerAnimationController animationController;

        private void Awake()
        {
            inputHandler?.Init();
            stateManager?.SetState(PlayerState.Standing);
            cooldownManager?.Init();
            movementController?.Init(this);
            cameraController?.Init(this);
            interactionController.inputHandler = inputHandler;
            uiController?.Init(this);
        }
    }
}