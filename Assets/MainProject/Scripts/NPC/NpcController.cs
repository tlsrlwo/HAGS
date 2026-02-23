using UnityEngine;
using UnityEngine.AI;

namespace GhostStory
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcController : MonoBehaviour
    {
        private NpcBaseState _currentState;
        public Npc currentNpc;

        // FSM
        public readonly NpcIdleState idleState = new NpcIdleState();
        public readonly NpcPatrolState patrolState = new NpcPatrolState();


        [Header("순찰 설정")]
        public float patrolSpeed = 1f;
        public float patorlRadius = 5f;
        public float minWaitTime = 2f;
        public float maxWaitTime = 6f;

        public bool isInteracting { get; private set; }


        [HideInInspector] public NavMeshAgent navAgent;
        [HideInInspector] public Vector3 originPoint;
        public Animator anim;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
            originPoint = transform.position;

            currentNpc = GetComponent<Npc>();
        }

        private void Start()
        {
            SwitchState(idleState);
        }

        private void Update()
        {
            _currentState?.UpdateState(this);
        }

        public void SwitchState(NpcBaseState newState)
        {
            _currentState?.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }

        public void OnDialogueStart()
        {
            isInteracting = true;
            SwitchState(idleState);
        }

        public void OnDialogueEnd()
        {
            isInteracting = false;
        }
    }
}
