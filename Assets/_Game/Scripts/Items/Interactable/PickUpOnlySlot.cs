using UnityEngine;

namespace Ouiki.Items
{
    public class PickUpOnlySlot : BaseSlot
    {
        private PickableItem currentItem;

        [Tooltip("If true, will eject any existing item before placing a new one.")]
        public bool ejectIfBlocked = true;

        public override bool CanPlace(PickableItem item)
        {
            return (!IsOccupied || ejectIfBlocked) && item != null;
        }

        public override void Place(PickableItem item)
        {
            if (item == null) return;

            if (IsOccupied)
            {
                if (ejectIfBlocked)
                {
                    EjectCurrentItem();
                }
                else
                {
                    return;
                }
            }

            currentItem = item;
            IsOccupied = true;
            item.OnPlacedInSlot(this);
        }

        public override void Remove()
        {
            currentItem = null;
            IsOccupied = false;
        }

        protected virtual void EjectCurrentItem()
        {
            if (currentItem != null)
            {
                currentItem.OnRemovedFromSlot(this);
                var rb = currentItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
                }
                currentItem.transform.position += Vector3.forward * 0.25f;
            }
            Remove();
        }

        void OnDrawGizmos()
        {
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