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

        private void Awake()
        {
            inputHandler.Init();
            stateManager.SetState(PlayerState.Standing);
            cooldownManager.Init();
            movementController.Init(this);
            cameraController.Init(this);
            uiController.Init(this);
        }
    }
}