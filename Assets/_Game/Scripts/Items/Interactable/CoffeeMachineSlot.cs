using Ouiki.Items;
using UnityEngine;

namespace Ouiki.FPS
{
    public class CoffeeMachineSlot : PlaceableSlot
    {
        public override bool CanPlace(PickableItem item)
        {
            var cup = item as CupItem;
            if (cup != null && cup.IsFilled)
                return false;
            return base.CanPlace(item);
        }
    }
}