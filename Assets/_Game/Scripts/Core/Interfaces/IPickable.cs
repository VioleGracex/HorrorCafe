namespace Ouiki.Interfaces
{
    public interface IPickable : IInteractable
    {
        void OnPickUp();
        void OnDrop();
    }
}