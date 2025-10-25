namespace Ouiki.FPS
{
    public interface IInteractable
    {
        bool IsInteractable { get; }
        void SetInteractable(bool interactable);
        void OnInteract(PlayerInteractionController controller);
    }
}