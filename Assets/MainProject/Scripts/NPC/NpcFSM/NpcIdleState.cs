using UnityEngine;

namespace GhostStory
{
    public class NpcIdleState : NpcBaseState
    {
        private float _waitTimer;
        private float _currentWaitTime;

        public override void EnterState(NpcController npc)
        {
            // Debug.Log($"[NpcIdleState] {npc.currentNpc.npcName} Idle상태 진입");
            _waitTimer = 0f;
            _currentWaitTime = Random.Range(npc.minWaitTime, npc.maxWaitTime);
            npc.navAgent.isStopped = true;
        }


        public override void UpdateState(NpcController npc)
        {
            if (npc.isInteracting) return;

            _waitTimer += Time.deltaTime;

            if (_waitTimer >= _currentWaitTime)
            {
                npc.SwitchState(npc.patrolState);
            }
        }
        
        public override void ExitState(NpcController npc)
        {
            
        }
    }
}
