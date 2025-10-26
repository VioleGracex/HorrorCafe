using UnityEngine;

namespace Ouiki.Items
{
    public abstract class BaseSlot : MonoBehaviour
    {
        public Transform snapPoint;
        public bool IsOccupied { get; protected set; }
        public abstract bool CanPlace(PickableItem item);
        public abstract void Place(PickableItem item);
        public abstract void Remove();
    }
}