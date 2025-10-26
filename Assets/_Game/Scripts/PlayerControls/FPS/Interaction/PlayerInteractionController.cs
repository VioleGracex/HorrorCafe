using Ouiki.Interfaces;
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

        [Header("Hold Offset (ignored, item is centered on ray)")]
        public Vector3 holdOffset = Vector3.zero;

        [Header("Scroll Settings")]
        [Tooltip("Minimum distance you can hold an item from the camera.")]
        public float MinHoldDistance = 0.5f;
        [Tooltip("Maximum distance you can hold an item from the camera.")]
        public float MaxHoldDistance = 4f;
        [Tooltip("How much scrolling moves the held item.")]
        public float ScrollSensitivity = 0.5f;

        private float heldItemDistance = 0f;

        private PickableItem heldItem;
        public bool HasItem => heldItem != null;
        public bool IsLookingAtInteractable { get; private set; }
        public IInteractable LookedAtInteractable { get; private set; } // for UI

        void Update()
        {
            // Always allow dropping with interact when holding, regardless of raycast
            if (HasItem && inputHandler.InteractPressed.Value)
            {
                inputHandler.ResetInteract();
                TryDrop();
                return;
            }

            IsLookingAtInteractable = false;
            LookedAtInteractable = null;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            // Update held item position (always centered on ray)
            if (heldItem != null)
            {
                float scroll = inputHandler.Scroll.Value;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    heldItemDistance += scroll * ScrollSensitivity * Time.deltaTime * 100f;
                    heldItemDistance = Mathf.Clamp(heldItemDistance, MinHoldDistance, MaxHoldDistance);
                }
                inputHandler.Scroll.Value = 0f;

                Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * heldItemDistance;
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
                    LookedAtInteractable = interactable;
                    if (!HasItem && inputHandler.InteractPressed.Value)
                    {
                        inputHandler.ResetInteract();
                        TryPickUp(pickable, hit.distance);
                        return;
                    }
                }
                else if (interactable != null && interactable.IsInteractable)
                {
                    IsLookingAtInteractable = true;
                    LookedAtInteractable = interactable;
                    if (inputHandler.InteractPressed.Value)
                    {
                        inputHandler.ResetInteract();
                        interactable.OnInteract(this);
                        return;
                    }
                }
            }
        }

        public void TryPickUp(PickableItem item, float? pickupDistance = null)
        {
            if (HasItem || !item.IsInteractable) return;
            heldItem = item;

            // If a specific pickup distance was provided (from raycast), use it, else calculate as before
            float distance = pickupDistance ?? Vector3.Distance(playerCamera.transform.position, item.transform.position);
            heldItemDistance = Mathf.Clamp(distance, MinHoldDistance, MaxHoldDistance);

            item.OnPickUp();
        }

        public void TryDrop()
        {
            if (!HasItem) return;
            heldItem.OnDrop();
            heldItem = null;
        }

        public IInteractable GetLookedAtInteractable()
        {
            return LookedAtInteractable;
        }

        void OnDrawGizmos()
        {
            Vector3 holdPoint = (playerCamera != null)
                ? playerCamera.transform.position + playerCamera.transform.forward * heldItemDistance
                : (transform.position + transform.forward * heldItemDistance);
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