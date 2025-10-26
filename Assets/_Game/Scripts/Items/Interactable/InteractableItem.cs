using Ouiki.Interfaces;
using UnityEngine;

namespace Ouiki.FPS
{
    [RequireComponent(typeof(Collider))]
    public class InteractableItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool isInteractable = true;
        public bool IsInteractable => isInteractable;

        public virtual void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        public virtual void OnInteract(PlayerInteractionController controller)
        {
        }
    }
}