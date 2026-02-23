

namespace GhostStory
{
    public class WalkState : MovementBaseState
    {
        public override void EnterState(PlayerMovement player)
        {
            player.UpdateAnimState(PlayerState.Walk);
        }
        public override void UpdateState(PlayerMovement player)
        {
            /*  if (Keyboard.current.leftShiftKey.isPressed)
             {
                 player.SwitchState(player.runState);
                 return;
             } */
            if (player.isRunPressed)
            {
                player.SwitchState(player.runState);
                return;
            }
            else if (player.InputDir.magnitude < 0.01f)
            {
                player.SwitchState(player.idleState);
                return;
            }

            player.Move(player.walkSpeed);
        }
        public override void ExitState(PlayerMovement player)
        {

        }
    }
}
