using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostStory
{
    public class IdleState : MovementBaseState
    {
        public override void EnterState(PlayerMovement player)
        {
            player.UpdateAnimState(PlayerState.Idle);
        }
        public override void UpdateState(PlayerMovement player)
        {
            if (player.InputDir.magnitude > 0.01f)
            {
                if (Keyboard.current.leftShiftKey.isPressed) player.SwitchState(player.runState);
                else player.SwitchState(player.walkState);
            }
        }
        public override void ExitState(PlayerMovement player)
        {

        }
    }
}
