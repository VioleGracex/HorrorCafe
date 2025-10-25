using UnityEngine;

namespace Ouiki.FPS
{
    public class CupItem : PickableItem
    {
        [SerializeField] private string cupLabel = "Cup";
        public override string Label => cupLabel;
        [SerializeField] private bool isFilled = false;
        public bool IsFilled => isFilled;
        public GameObject coffeeVisual;

        private PlaceableSlot currentSlot;

        public void SetFilled(bool filled)
        {
            isFilled = filled;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (coffeeVisual != null)
                coffeeVisual.SetActive(isFilled);
        }

        public override void OnPlacedInSlot(PlaceableSlot slot)
        {
            base.OnPlacedInSlot(slot);
            currentSlot = slot;
            CoffeeMachine machine = slot.GetComponentInParent<CoffeeMachine>();
            if (machine != null)
            {
                machine.StartFilling(this);
            }
        }

        public override void OnRemovedFromSlot(PlaceableSlot slot)
        {
            base.OnRemovedFromSlot(slot);
            currentSlot = null;
        }

        private void OnValidate()
        {
            UpdateVisuals();
        }
    }
}