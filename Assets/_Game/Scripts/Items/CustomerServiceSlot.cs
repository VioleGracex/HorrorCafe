using UnityEngine;
using Ouiki.Items;
using Ouiki.FPS;
using Ouiki.Interfaces;
using DG.Tweening;

namespace Ouiki.Restaurant
{
    public class CustomerServiceSlot : BaseSlot, IInteractable, ILabel
    {
        public string Label => "Serve Coffee";
        public bool IsInteractable => !IsOccupied;

        public string ActionName => "[E] Place Coffee";
        public Transform indicator; // assign a floating icon in inspector

        private void Start()
        {
            if (indicator != null)
            {
                AnimateIndicator();
                indicator.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (indicator != null)
                indicator.gameObject.SetActive(!IsOccupied);
        }

        void AnimateIndicator()
        {
            if (indicator == null) return;
            indicator.DOLocalMoveY(0.25f, 0.7f)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);
            indicator.DOLocalRotate(new Vector3(0, 360, 0), 1.5f, RotateMode.FastBeyond360)
                    .SetLoops(-1)
                    .SetEase(Ease.Linear);
        }

        public override bool CanPlace(PickableItem item)
        {
            var cup = item as CupItem;
            return !IsOccupied && cup != null && cup.IsFilled;
        }

        public override void Place(PickableItem item)
        {
            if (!CanPlace(item)) return;
            item.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
            item.transform.SetParent(snapPoint);
            IsOccupied = true;
            item.OnPlacedInSlot(this);
        }

        public override void Remove()
        {
            IsOccupied = false;
        }

        public void OnInteract(PlayerInteractionController controller)
        {
            if (!IsOccupied)
            {
                var held = controller.GetHeldItem();
                var cup = held as CupItem;
                if (cup != null && cup.IsFilled)
                {
                    controller.ForceDrop(held);
                    Place(cup);
                }
            }
        }

        public void SetInteractable(bool interactable) { }
    }
}