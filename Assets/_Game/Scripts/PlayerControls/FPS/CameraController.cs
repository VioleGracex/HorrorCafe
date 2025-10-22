using UnityEngine;

namespace Ouiki.FPS
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform headBobJoint;
        public Transform playerBody;

        [Header("Look Settings")]
        [Range(0.1f, 5f)]
        public float mouseSensitivity = 2f;
        public float maxLookAngle = 65f;
        public bool invertY = false;
        public bool lockCursor = true;

        [Header("Zoom Settings")]
        public bool enableZoom = true;
        public float zoomFOV = 30f;
        public float fov = 60f;
        public float zoomSpeed = 7f;

        [Header("Head Bob")]
        public bool enableHeadBob = true;
        public float bobSpeed = 10f;
        public Vector3 bobAmount = new Vector3(.1f, .05f, 0f);

        private PlayerManager manager;
        private PlayerInputHandler input;
        private PlayerStateManager state;
        private float pitch, yaw, timer;
        private Vector3 jointOriginalPos;
        private bool isZooming;
        private bool _initialized = false;

        public void Init(PlayerManager mgr)
        {
            manager = mgr;
            input = manager.inputHandler;
            state = manager.stateManager;
            jointOriginalPos = headBobJoint.localPosition;
            if (playerCamera)
            {
                playerCamera.fieldOfView = fov;
            }
            else
            {
                playerCamera = Camera.main;
                if (playerCamera)
                    playerCamera.fieldOfView = fov;
            }

            Cursor.visible = false;
            _initialized = true;
        }

        void Update()
        {
            if (!_initialized) return;
            HandleLook();
            HandleZoom();
            if (enableHeadBob) HeadBob();
        }

        void HandleLook()
        {
            if (!state.CanDo(PlayerAction.Move)) return;
            if (Cursor.visible) return;

            yaw += input.Look.Value.x * mouseSensitivity;
            pitch += (invertY ? input.Look.Value.y : -input.Look.Value.y) * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            playerBody.localEulerAngles = new Vector3(0f, yaw, 0f);
            if (playerCamera)
                playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);

            if (lockCursor)
                Cursor.lockState = CursorLockMode.Locked;
        }

        void HandleZoom()
        {
            isZooming = enableZoom && input.ZoomHeld && state.CanDo(PlayerAction.Zoom);
            float targetFov = isZooming ? zoomFOV : fov;
            if (playerCamera)
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, zoomSpeed * Time.deltaTime);
        }

        void HeadBob()
        {
            bool moving = input.MoveInput.Value.magnitude > 0.1f && state.CanDo(PlayerAction.Move);
            if (moving)
            {
                timer += Time.deltaTime * bobSpeed;
                headBobJoint.localPosition = jointOriginalPos +
                    new Vector3(Mathf.Sin(timer) * bobAmount.x, Mathf.Sin(timer * 2) * bobAmount.y, 0f);
            }
            else
            {
                timer = 0;
                headBobJoint.localPosition = Vector3.Lerp(headBobJoint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed);
            }
        }
    }
}