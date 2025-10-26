using UnityEngine;
using Ouiki.FPS;

namespace Ouiki.Items
{
    [RequireComponent(typeof(BoxCollider))]
    public class PlaceableSlot : BaseSlot
    {
        private PickableItem currentItem;
        private BoxCollider triggerArea;

        void Awake()
        {
            triggerArea = GetComponent<BoxCollider>();
            triggerArea.isTrigger = true;
        }

        public override bool CanPlace(PickableItem item)
        {
            return !IsOccupied && item != null;
        }

        public override void Place(PickableItem item)
        {
            if (CanPlace(item))
            {
                currentItem = item;
                IsOccupied = true;
                item.OnPlacedInSlot(this);
            }
        }

        public override void Remove()
        {
            currentItem = null;
            IsOccupied = false;
        }

        void OnTriggerStay(Collider other)
        {
            var pickable = other.GetComponent<PickableItem>();
            if (IsOccupied) return;

            if (pickable != null && !pickable.IsHeldByPlayer && CanPlace(pickable))
            {
                Place(pickable);
            }
        }

        void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box)
            {
                Gizmos.color = IsOccupied ? new Color(1f, 0f, 0f, 0.1f) : new Color(0f, 1f, 0f, 0.1f);
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }

            if (snapPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(snapPoint.position, 0.08f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, snapPoint.position);
            }
        }
    }
}