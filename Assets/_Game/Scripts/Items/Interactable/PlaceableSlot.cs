using UnityEngine;

namespace Ouiki.FPS
{
    public class PlaceableSlot : MonoBehaviour
    {
        public Transform snapPoint;
        public bool IsOccupied { get; private set; }
        private PickableItem currentItem;

        public virtual bool CanPlace(PickableItem item)
        {
            return !IsOccupied && item != null;
        }

        public virtual void Place(PickableItem item)
        {
            if (CanPlace(item))
            {
                currentItem = item;
                IsOccupied = true;
                item.OnPlacedInSlot(this);
            }
        }

        public virtual void Remove()
        {
            currentItem = null;
            IsOccupied = false;
        }

        void OnDrawGizmos()
        {
            Color boxColor = IsOccupied ? new Color(1f, 0f, 0f, 0.25f) : new Color(0f, 1f, 0f, 0.25f);
            Gizmos.color = boxColor;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);

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