using UnityEngine;

namespace Ouiki.FPS
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform cameraAnchor;  
        public Transform neckBone;     
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
        public Vector3 bobAmount = new Vector3(.05f, .025f, 0f); // Reduced for comfort

        private PlayerManager manager;
        private PlayerInputHandler input;
        private PlayerStateManager state;
        private float pitch, yaw, timer;
        private Vector3 jointOriginalPos;
        private bool isZooming;
        private bool _initialized = false;
        private bool _cameraOnNeck = false; // Tracks if camera is parented to neck

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

            if (cameraAnchor != null && playerCamera != null)
                playerCamera.transform.SetParent(cameraAnchor, false);

            _initialized = true;
        }

        void Update()
        {
            if (!_initialized) return;

            if (state != null)
            {
                if (state.CurrentState == PlayerState.KnockedOut && !_cameraOnNeck)
                {
                    ParentCameraToNeck();
                }
                else if (state.CurrentState != PlayerState.KnockedOut && _cameraOnNeck)
                {
                    ParentCameraToAnchor();
                }
            }

            HandleLook();
            HandleZoom();

            if (enableHeadBob && !_cameraOnNeck)
                HeadBob(); // Only apply headbob when camera is on stable anchor
        }

        void HandleLook()
        {
            if (state != null && !state.CanDo(PlayerAction.Move)) return;
            if (Cursor.visible) return;
            if (_cameraOnNeck) return; // Don't apply look when camera is parented to neck

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
            isZooming = enableZoom && input.ZoomHeld && (state == null || state.CanDo(PlayerAction.Zoom));
            float targetFov = isZooming ? zoomFOV : fov;
            if (playerCamera)
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, zoomSpeed * Time.deltaTime);
        }

        void HeadBob()
        {
            bool moving = input.MoveInput.Value.magnitude > 0.1f && (state == null || state.CanDo(PlayerAction.Move));
            Vector3 targetPos = jointOriginalPos;
            if (moving)
            {
                timer += Time.deltaTime * bobSpeed;
                targetPos += new Vector3(Mathf.Sin(timer) * bobAmount.x, Mathf.Sin(timer * 2) * bobAmount.y, 0f);
            }
            else
            {
                timer = 0;
            }
            headBobJoint.localPosition = Vector3.Lerp(headBobJoint.localPosition, targetPos, Time.deltaTime * (moving ? bobSpeed : bobSpeed * 2f));
        }

        private void ParentCameraToNeck()
        {
            if (neckBone != null && playerCamera != null)
            {
                playerCamera.transform.SetParent(neckBone, false);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
                _cameraOnNeck = true;
            }
        }

        private void ParentCameraToAnchor()
        {
            if (cameraAnchor != null && playerCamera != null)
            {
                playerCamera.transform.SetParent(cameraAnchor, false);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
                _cameraOnNeck = false;
            }
        }
    }
}