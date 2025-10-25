namespace Ouiki.FPS
{
    public interface IPickable : IInteractable
    {
        void OnPickUp();
        void OnDrop();
    }
}