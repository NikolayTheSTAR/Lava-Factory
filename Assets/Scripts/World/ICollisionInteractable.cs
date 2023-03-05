namespace World
{
    public interface ICollisionInteractable
    {
        bool CanInteract { get; }

        void Interact(Player p);

        void StopInteract(Player p);
    }
}