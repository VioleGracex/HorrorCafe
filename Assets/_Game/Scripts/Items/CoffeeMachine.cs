using UnityEngine;
using System.Collections;
using DG.Tweening;
using Ouiki.FPS;
using Ouiki.Interfaces;
using Ouiki.Items;

namespace Ouiki.Items
{
    public class CoffeeMachine : MonoBehaviour, ILabel, IInteractable, ICommand
    {
        public string Label => "Coffee Machine";
        public PickUpOnlySlot cupSlot;
        public float fillTime = 3f;
        public AudioSource fillSound;

        private Coroutine fillRoutine;

        public bool IsInteractable => true;
        public string ActionName
        {
            get
            {
                var player = FindFirstObjectByType<PlayerInteractionController>();
                var held = player != null ? player.GetHeldItem() : null;
                if (held is CupItem cup && cupSlot.CanPlace(cup) && !cup.IsFilled)
                {
                    return "[E] Fill Cup";
                }
                else
                {
                    return "[E] Needs Cup";
                }
            }
        }

        public void OnInteract(PlayerInteractionController controller)
        {
            var held = controller.GetHeldItem();
            if (held is CupItem cup && cupSlot.CanPlace(cup) && !cup.IsFilled)
            {
                controller.ForceDrop(held); 
                cupSlot.Place(cup);
            }
        }

        public void SetInteractable(bool interactable) { }

        public void StartFilling(CupItem cup)
        {
            if (fillRoutine != null) StopCoroutine(fillRoutine);
            fillRoutine = StartCoroutine(FillCupRoutine(cup));
        }

        IEnumerator FillCupRoutine(CupItem cup)
        {
            if (fillSound) fillSound.Play();

            cup.AnimateFill(fillTime);

            yield return new WaitForSeconds(fillTime);

            if (fillSound) fillSound.Stop();

            cup.SetFilled(true);
            cup.SetInteractable(true);
            cupSlot.Remove();
            fillRoutine = null;
        }
    }
}