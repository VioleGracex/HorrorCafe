using Ouiki.FPS;

namespace Ouiki.Interfaces
{
    public interface IInteractable
    {
        bool IsInteractable { get; }
        void SetInteractable(bool interactable);
        void OnInteract(PlayerInteractionController controller);
    }
}