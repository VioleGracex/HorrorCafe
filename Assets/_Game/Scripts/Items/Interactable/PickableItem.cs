using DG.Tweening;
using Ouiki.FPS;
using Ouiki.Interfaces;
using UnityEngine;

namespace Ouiki.Items
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PickableItem : InteractableItem, IPickable, ILabel
    {
        [SerializeField] private string label;
        public virtual string Label => label;

        public virtual string ActionName => "[E] Grab";
        [HideInInspector] public bool IsHeldByPlayer = false;
        private Rigidbody rb;
        private Collider col;

        private Tween moveTween;
        private Tween rotateTween;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public override void OnInteract(PlayerInteractionController controller)
        {
            controller.TryPickUp(this);
        }

        public virtual void OnPickUp()
        {
            // Kill possible active tweens to ensure smooth pickup
            if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
            if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();

            SetInteractable(false);
            rb.useGravity = false;
            rb.linearDamping = 15f;
            rb.angularDamping = 15f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false; // Ensure it's dynamic for HoldAt movement
            IsHeldByPlayer = true;
        }

        public virtual void OnDrop()
        {
            SetInteractable(true);
            rb.useGravity = true;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.isKinematic = false;
            IsHeldByPlayer = false;
        }

        public void HoldAt(Vector3 pos, Quaternion rot)
        {
            Vector3 toTarget = pos - rb.position;
            rb.linearVelocity = toTarget * 30f;
            rb.MoveRotation(rot);
        }

        public virtual void OnPlacedInSlot(BaseSlot slot)
        {
            if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
            if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            moveTween = transform.DOMove(slot.snapPoint.position, 0.3f).SetEase(Ease.OutSine);
            rotateTween = transform.DORotateQuaternion(slot.snapPoint.rotation, 0.3f).SetEase(Ease.OutSine);
            SetInteractable(false);
            if (col != null)
                col.enabled = true; 
        }

        public virtual void OnRemovedFromSlot(BaseSlot slot)
        {
            if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
            if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();

            rb.isKinematic = false;
            SetInteractable(true);
            if (col != null)
                col.enabled = true;
        }
    }
}