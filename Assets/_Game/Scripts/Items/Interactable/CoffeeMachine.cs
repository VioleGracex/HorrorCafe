using UnityEngine;
using System.Collections;

namespace Ouiki.FPS
{
    public class CoffeeMachine : MonoBehaviour, ILabel
    {
        public string Label => "Coffee Machine";
        public PlaceableSlot cupSlot;
        public float fillTime = 3f;
        public AudioSource fillSound;

        private Coroutine fillRoutine;

        public void StartFilling(CupItem cup)
        {
            if (fillRoutine != null) StopCoroutine(fillRoutine);
            fillRoutine = StartCoroutine(FillCupRoutine(cup));
        }

        IEnumerator FillCupRoutine(CupItem cup)
        {
            if (fillSound) fillSound.Play();
            yield return new WaitForSeconds(fillTime);
            if (fillSound) fillSound.Stop();

            cup.SetFilled(true);
            cup.SetInteractable(true); 
            cupSlot.Remove();
            fillRoutine = null;
        }
    }
}