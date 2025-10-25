using DG.Tweening;
using UnityEngine;

namespace Ouiki.FPS
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PickableItem : InteractableItem, IPickable, ILabel
    {
        [SerializeField] private string label;
        public virtual string Label => label;
        [HideInInspector] public bool IsHeldByPlayer = false;
        private Rigidbody rb;
        private Collider col;

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
            SetInteractable(false);
            rb.useGravity = false;
            rb.linearDamping = 15f;
            rb.angularDamping = 15f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.None;
            IsHeldByPlayer = true;
            if (col != null)
                col.enabled = false;
        }

        public virtual void OnDrop()
        {
            SetInteractable(true);
            rb.useGravity = true;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.interpolation = RigidbodyInterpolation.None;
            IsHeldByPlayer = false;
            if (col != null)
                col.enabled = true;
        }

        public void HoldAt(Vector3 pos, Quaternion rot)
        {
            Vector3 toTarget = pos - rb.position;
            rb.linearVelocity = toTarget * 30f;
            rb.MoveRotation(rot);
        }

        public virtual void OnPlacedInSlot(PlaceableSlot slot)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.DOMove(slot.snapPoint.position, 0.3f).SetEase(Ease.OutSine);
            transform.DORotateQuaternion(slot.snapPoint.rotation, 0.3f).SetEase(Ease.OutSine);
            SetInteractable(false);
            if (col != null)
                col.enabled = true; 
        }

        public virtual void OnRemovedFromSlot(PlaceableSlot slot)
        {
            rb.isKinematic = false;
            SetInteractable(true);
            if (col != null)
                col.enabled = true;
        }
    }
}