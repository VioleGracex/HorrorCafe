using UnityEngine;
using Ouiki.FPS;
using Ouiki.Interfaces;

namespace Ouiki.Items
{
    public class CupDispenser : MonoBehaviour, IInteractable, ILabel, ICommand
    {
        [Header("Cup Dispenser Settings")]
        [SerializeField] private string dispenserLabel = "Cup Dispenser";
        [SerializeField] private string actionName = "[E] Grab a cup";
        [SerializeField] private GameObject cupPrefab;
        [SerializeField] private BaseSlot outputSlot; // Accepts PlaceableSlot or PickUpOnlySlot

        [Header("Cooldown")]
        [SerializeField] private float cooldownTime = 1.0f;
        private float cooldownTimer = 0f;

        [Header("Max Cups Settings")]
        [SerializeField] private int maxCups = 50;
        private int cupsDispensed = 0;

        public string Label => dispenserLabel;
        public string ActionName => actionName;
        public bool IsInteractable =>
            cupPrefab != null &&
            outputSlot != null &&
            !outputSlot.IsOccupied &&
            cooldownTimer <= 0f &&
            cupsDispensed < maxCups;

        void Update()
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;
        }

        public void OnInteract(PlayerInteractionController controller)
        {
            if (!IsInteractable) return;

            Vector3 spawnPos = outputSlot.snapPoint ? outputSlot.snapPoint.position : outputSlot.transform.position;
            Quaternion spawnRot = outputSlot.snapPoint ? outputSlot.snapPoint.rotation : outputSlot.transform.rotation;

            GameObject cupGO = Instantiate(cupPrefab, spawnPos, spawnRot);
            CupItem cup = cupGO.GetComponent<CupItem>();
            if (cup != null)
            {
                controller.TryPickUp(cup, Vector3.Distance(controller.playerCamera.transform.position, cup.transform.position));
                cooldownTimer = cooldownTime;
                cupsDispensed++;
            }
            else
            {
                Debug.LogWarning("Cup prefab does not have a CupItem component!");
            }
        }

        public void SetInteractable(bool interactable)
        {
          
        }
    }
}