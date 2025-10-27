using Ouiki.Interfaces;
using Ouiki.Items;
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Ouiki.FPS
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("References")]
        public PlayerInputHandler inputHandler;
        public Camera playerCamera;
        public PlayerStateManager stateManager;

        [Header("Interaction Settings")]
        public float interactDistance = 2f;
        public LayerMask interactableLayerMask = ~0;

        [Header("Hold Offset (ignored, item is centered on ray)")]
        public Vector3 holdOffset = Vector3.zero;

        [Header("Scroll Settings")]
        public float MinHoldDistance = 0.5f;
        public float MaxHoldDistance = 4f;
        public float ScrollSensitivity = 0.5f;

        [Header("Layer Settings")]
        public LayerMask heldItemLayerMask;
        public LayerMask interactableLayerMaskForDrop;

        [Header("Knockout Settings")]
        [Tooltip("How long the player stays knocked out (seconds)")]
        public float knockedOutTime = 2f;
        [Tooltip("How long the standup animation/tween should take (seconds)")]
        public float standUpDuration = 0.25f;
        [Tooltip("World Y coordinate to tween to when standing up (player standing height)")]
        public float standUpY = 1.7f;

        private float heldItemDistance = 0f;

        private PickableItem heldItem;
        private int originalHeldItemLayer = -1;

        public bool HasItem => heldItem != null;
        public bool IsLookingAtInteractable { get; private set; }
        public IInteractable LookedAtInteractable { get; private set; }

        private Coroutine knockoutRoutine;
        private Tween knockdownTween;
        private Tween standupTween;

        void Update()
        {
            if (stateManager != null && stateManager.IsKnockedOut)
                return;

            IsLookingAtInteractable = false;
            LookedAtInteractable = null;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

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

            bool didInteract = false;

            if (Physics.Raycast(ray, out hit, interactDistance, interactableLayerMask))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                IsLookingAtInteractable = interactable != null && interactable.IsInteractable;
                LookedAtInteractable = interactable;

                if (IsLookingAtInteractable && inputHandler.InteractPressed.Value)
                {
                    inputHandler.ResetInteract();

                    if (interactable is PickableItem pickable && !HasItem)
                    {
                        TryPickUp(pickable, hit.distance);
                        didInteract = true;
                    }
                    else
                    {
                        interactable.OnInteract(this);
                        didInteract = true;
                    }
                }
            }

            if (!didInteract && HasItem && inputHandler.InteractPressed.Value)
            {
                inputHandler.ResetInteract();
                TryDrop();
            }
        }

        public void TryPickUp(PickableItem item, float? pickupDistance = null)
        {
            if (HasItem || !item.IsInteractable) return;
            heldItem = item;
            float distance = pickupDistance ?? Vector3.Distance(playerCamera.transform.position, item.transform.position);
            heldItemDistance = Mathf.Clamp(distance, MinHoldDistance, MaxHoldDistance);
            item.OnPickUp();

            originalHeldItemLayer = item.gameObject.layer;
            SetLayerRecursively(item.gameObject, LayerMaskToLayer(heldItemLayerMask));
        }

        public void TryDrop()
        {
            if (!HasItem) return;

            if (originalHeldItemLayer != -1)
                SetLayerRecursively(heldItem.gameObject, originalHeldItemLayer);
            else
                SetLayerRecursively(heldItem.gameObject, LayerMaskToLayer(interactableLayerMaskForDrop));

            heldItem.OnDrop();
            heldItem = null;
            originalHeldItemLayer = -1;
        }

        public IInteractable GetLookedAtInteractable()
        {
            return LookedAtInteractable;
        }

        public PickableItem GetHeldItem()
        {
            return heldItem;
        }

        public void ForceDrop(PickableItem item)
        {
            if (heldItem == item)
            {
                if (originalHeldItemLayer != -1)
                    SetLayerRecursively(heldItem.gameObject, originalHeldItemLayer);
                else
                    SetLayerRecursively(heldItem.gameObject, LayerMaskToLayer(interactableLayerMaskForDrop));

                heldItem = null;
                originalHeldItemLayer = -1;
            }
        }

        /// <summary>
        /// Knock out the player. Will smoothly move the player to the ground, play knockdown, then stand up after a delay.
        /// </summary>
        public void KnockOutPlayer()
        {
            if (knockoutRoutine != null)
                StopCoroutine(knockoutRoutine);

            knockdownTween?.Kill();
            standupTween?.Kill();

            knockoutRoutine = StartCoroutine(KnockoutRoutine());
        }

        private IEnumerator KnockoutRoutine()
        {
            yield return StartCoroutine(DOTweenSnapToGround());

            if (stateManager != null)
                stateManager.KnockOut();

            yield return new WaitForSeconds(knockedOutTime);

            if (stateManager != null)
                stateManager.Recover();

            yield return StartCoroutine(DOTweenStandUp());
        }

        private IEnumerator DOTweenSnapToGround()
        {
            Vector3 start = transform.position + Vector3.up * 0.1f;
            float maxDistance = 10f;
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 groundPos = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                float duration = 0.15f;
                knockdownTween = transform.DOMoveY(groundPos.y, duration).SetEase(Ease.InSine);
                yield return knockdownTween.WaitForCompletion();
            }
            else
            {
                yield return null;
            }
        }

        private IEnumerator DOTweenStandUp()
        {
            float currentY = transform.position.y;
            float targetY = standUpY;

            if (currentY < targetY - 0.05f)
            {
                standupTween = transform.DOMoveY(targetY, standUpDuration).SetEase(Ease.OutSine);
                yield return standupTween.WaitForCompletion();
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child != null)
                    SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private int LayerMaskToLayer(LayerMask mask)
        {
            int bits = mask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((bits & (1 << i)) != 0)
                    return i;
            }
            return 0;
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