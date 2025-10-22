using UnityEngine;
using UnityEngine.InputSystem;

namespace Ouiki.FPS
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public ObservableValue<Vector2> MoveInput = new ObservableValue<Vector2>();
        public ObservableValue<Vector2> Look = new ObservableValue<Vector2>();
        public ObservableValue<bool> JumpPressed = new ObservableValue<bool>();
        public ObservableValue<bool> SprintHeld = new ObservableValue<bool>();
        public ObservableValue<bool> CrouchPressed = new ObservableValue<bool>();
        public ObservableValue<bool> ZoomHeld = new ObservableValue<bool>();

        private PlayerControls controls;

        public void Init()
        {
            controls = new PlayerControls();

            controls.Player.Move.performed += ctx => MoveInput.Value = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled += ctx => MoveInput.Value = Vector2.zero;

            controls.Player.Look.performed += ctx => Look.Value = ctx.ReadValue<Vector2>();
            controls.Player.Look.canceled += ctx => Look.Value = Vector2.zero;

            controls.Player.Jump.performed += ctx => JumpPressed.Value = true;
            controls.Player.Jump.canceled += ctx => JumpPressed.Value = false;

            controls.Player.Sprint.performed += ctx => SprintHeld.Value = true;
            controls.Player.Sprint.canceled += ctx => SprintHeld.Value = false;

            controls.Player.Crouch.performed += ctx => CrouchPressed.Value = true;
            controls.Player.Crouch.canceled += ctx => CrouchPressed.Value = false;

            controls.Player.Zoom.performed += ctx => ZoomHeld.Value = true;
            controls.Player.Zoom.canceled += ctx => ZoomHeld.Value = false;
        }

        private void OnEnable() => controls?.Enable();
        private void OnDisable() => controls?.Disable();

        public void ResetCrouch() => CrouchPressed.Value = false;
    }
}