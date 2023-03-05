namespace World
{
    public interface ICollisionInteractable
    {
        bool CanInteract { get; }
        ICICondition Condition { get; }

        void Interact(Player p);

        void StopInteract(Player p);
    }

    public enum ICICondition
    {
        None,
        PlayerIsStopped
    }
}