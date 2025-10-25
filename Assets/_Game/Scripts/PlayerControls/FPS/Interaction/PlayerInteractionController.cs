using UnityEngine;

namespace Ouiki.FPS
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("References")]
        public PlayerInputHandler inputHandler;
        public Camera playerCamera;

        [Header("Interaction Settings")]
        public float interactDistance = 2f;
        public LayerMask interactableLayerMask = ~0;

        [Header("Hold Offset")]
        public Vector3 holdOffset = new Vector3(0.5f, -0.5f, 1.5f);

        [Header("Scroll Settings")]
        [Tooltip("Minimum distance you can hold an item from the camera.")]
        public float MinHoldDistance = 0.5f;
        [Tooltip("Maximum distance you can hold an item from the camera.")]
        public float MaxHoldDistance = 4f;
        [Tooltip("How much scrolling moves the held item.")]
        public float ScrollSensitivity = 0.5f;

        private float heldItemDistance = 0f;
        private Vector3 heldItemWorldOffset = Vector3.zero;

        private PickableItem heldItem;
        public bool HasItem => heldItem != null;
        public bool IsLookingAtInteractable { get; private set; }

        void Update()
        {
            IsLookingAtInteractable = false;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            // Handle held item position and scroll
            if (heldItem != null)
            {
                float scroll = inputHandler.Scroll.Value;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    heldItemDistance += scroll * ScrollSensitivity * Time.deltaTime * 100f;
                    heldItemDistance = Mathf.Clamp(heldItemDistance, MinHoldDistance, MaxHoldDistance);
                }
                inputHandler.Scroll.Value = 0f;

                Vector3 basePos = playerCamera.transform.position + playerCamera.transform.forward * heldItemDistance;
                Vector3 targetPos = basePos + heldItemWorldOffset;
                Quaternion targetRot = Quaternion.LookRotation(playerCamera.transform.forward, Vector3.up);
                heldItem.HoldAt(targetPos, targetRot);
            }

            // Raycast for interactable objects
            if (Physics.Raycast(ray, out hit, interactDistance, interactableLayerMask))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable is PickableItem pickable && pickable.IsInteractable)
                {
                    IsLookingAtInteractable = true;
                    if (inputHandler.InteractPressed.Value)
                    {
                        inputHandler.ResetInteract();
                        if (HasItem)
                        {
                            TryDropOrPlace();
                        }
                        else
                        {
                            TryPickUp(pickable);
                        }
                    }
                }
                else if (interactable != null && interactable.IsInteractable)
                {
                    IsLookingAtInteractable = true;
                    if (inputHandler.InteractPressed.Value)
                    {
                        inputHandler.ResetInteract();
                        interactable.OnInteract(this);
                    }
                }
            }
            else
            {
                if (inputHandler.InteractPressed.Value)
                {
                    inputHandler.ResetInteract();
                    if (HasItem)
                        TryDropOrPlace();
                }
            }
        }

        public void TryPickUp(PickableItem item)
        {
            if (HasItem || !item.IsInteractable) return;
            heldItem = item;
            Vector3 camPos = playerCamera.transform.position;
            Vector3 itemPos = item.transform.position;
            Vector3 camForward = playerCamera.transform.forward;

            // Project item's position onto camera's forward ray to get distance
            heldItemDistance = Vector3.Dot(itemPos - camPos, camForward);
            heldItemDistance = Mathf.Clamp(heldItemDistance, MinHoldDistance, MaxHoldDistance);
            // Calculate offset from "ideal" hold position
            heldItemWorldOffset = itemPos - (camPos + camForward * heldItemDistance);

            item.OnPickUp();
        }

        public void TryDrop()
        {
            if (!HasItem) return;
            heldItem.OnDrop();
            heldItem = null;
            heldItemWorldOffset = Vector3.zero;
        }

        void TryDropOrPlace()
        {
            if (!HasItem) return;
            var heldTransform = heldItem.transform;
            var slots = Object.FindObjectsByType<PlaceableSlot>(FindObjectsSortMode.None);
            PlaceableSlot nearest = null;
            float minDist = 0.5f; // Snap range

            foreach (var slot in slots)
            {
                float dist = Vector3.Distance(slot.snapPoint.position, heldTransform.position);
                if (dist < minDist && slot.CanPlace(heldItem))
                {
                    minDist = dist;
                    nearest = slot;
                }
            }

            if (nearest != null)
            {
                nearest.Place(heldItem);
                heldItem = null;
                heldItemWorldOffset = Vector3.zero;
            }
            else
            {
                TryDrop();
            }
        }

        public void TryPlaceAt(Transform placeTarget)
        {
            if (!HasItem) return;
            heldItem.OnDrop();
            var heldTransform = heldItem.transform;
            heldTransform.position = placeTarget.position;
            heldItem = null;
            heldItemWorldOffset = Vector3.zero;
        }

        void OnDrawGizmos()
        {
            Vector3 holdPoint = (playerCamera != null)
                ? playerCamera.transform.position + playerCamera.transform.TransformDirection(holdOffset)
                : (transform.position + transform.TransformDirection(holdOffset));
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(holdPoint, 0.12f);
            Gizmos.DrawLine(transform.position, holdPoint);

            if (playerCamera != null)
            {
                Vector3 camPos = playerCamera.transform.position;
                Vector3 camDir = playerCamera.transform.forward;
                Vector3 rayEnd = camPos + camDir * interactDistance;
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(camPos, rayEnd);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(rayEnd, 0.08f);
            }
        }
    }
}