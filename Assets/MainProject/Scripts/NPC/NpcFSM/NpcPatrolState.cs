using UnityEngine;
using UnityEngine.AI;

namespace GhostStory
{
    public class NpcPatrolState : NpcBaseState
    {        
        public override void EnterState(NpcController npc)
        {
            Debug.Log($"[NpcIdleState] {npc.currentNpc.npcName} Patrol상태 진입");

            npc.navAgent.isStopped = false;
            npc.navAgent.speed = npc.patrolSpeed;

            SetRandomDestination(npc);
        }
        public override void UpdateState(NpcController npc)
        {
            // 대화 시작 시 즉시 정지 및 idle 상태로 전환 유도
            if (npc.isInteracting)
            {
                npc.SwitchState(npc.idleState);
                return;
            }

            // 목적지 도착 확인
            if (!npc.navAgent.pathPending && npc.navAgent.remainingDistance <= npc.navAgent.stoppingDistance + 0.1f)
            {
                npc.SwitchState(npc.idleState);
            }
        }

        private void SetRandomDestination(NpcController npc)
        {
            Vector3 randomDir = Random.insideUnitSphere * npc.patorlRadius;
            randomDir += npc.originPoint;

            NavMeshHit hit;

            if(NavMesh.SamplePosition(randomDir,out hit, 2.0f, NavMesh.AllAreas))
            {
                npc.navAgent.SetDestination(hit.position);
            }
        }
        
         public override void ExitState(NpcController npc)
        {
        }
    }
}
