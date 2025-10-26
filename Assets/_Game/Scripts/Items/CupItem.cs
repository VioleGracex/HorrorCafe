using UnityEngine;
using DG.Tweening;
using Ouiki.FPS;

namespace Ouiki.Items
{
    public class CupItem : PickableItem
    {
        [SerializeField] private string cupLabel = "Coffee Cup";
        public override string Label => cupLabel;
        [SerializeField] private bool isFilled = false;
        public bool IsFilled => isFilled;
        public GameObject coffeeVisual;

        private BaseSlot currentSlot;

        public void AnimateFill(float duration)
        {
            if (coffeeVisual) return;

            coffeeVisual.SetActive(true);
            Renderer rend = coffeeVisual.GetComponent<Renderer>();
            if (rend == null) return;

            Material mat = rend.material;
            mat.SetFloat("_FillValue", 0f);
            DOTween.To(
                () => mat.GetFloat("_FillValue"),
                x => mat.SetFloat("_FillValue", x),
                1f, duration
            );
        }

        public void SetFilled(bool filled)
        {
            isFilled = filled;
            UpdateVisuals();
            if (coffeeVisual != null)
            {
                Renderer rend = coffeeVisual.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.SetFloat("_FillValue", filled ? 1f : 0f);
            }
        }

        private void UpdateVisuals()
        {
            if (coffeeVisual != null)
                coffeeVisual.SetActive(isFilled);
        }

        public override void OnPlacedInSlot(BaseSlot slot)
        {
            base.OnPlacedInSlot(slot);
            currentSlot = slot;
            CoffeeMachine machine = slot.GetComponentInParent<CoffeeMachine>();
            if (machine != null)
            {
                machine.StartFilling(this);
            }
        }

        public override void OnRemovedFromSlot(BaseSlot slot)
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