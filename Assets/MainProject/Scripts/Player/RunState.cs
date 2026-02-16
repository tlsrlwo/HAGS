using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostStory
{
    public class RunState : MovementBaseState
    {
        public override void EnterState(PlayerMovement player)
        {
            player.UpdateAnimState(PlayerState.Run);
        }
        public override void UpdateState(PlayerMovement player)
        {
            if (!Keyboard.current.leftShiftKey.isPressed)
            {
                player.SwitchState(player.walkState);
                return;
            }
            else if (player.InputDir.magnitude < 0.01f)
            {
                player.SwitchState(player.idleState);
                return;
            }

            player.Move(player.runSpeed);
        }
        public override void ExitState(PlayerMovement player)
        {
            
        }
    }
}
