namespace GhostStory
{
    public abstract class MovementBaseState
    {
        public abstract void EnterState(PlayerMovement player);
        public abstract void UpdateState(PlayerMovement player);
        public abstract void ExitState(PlayerMovement player);
    }
}
