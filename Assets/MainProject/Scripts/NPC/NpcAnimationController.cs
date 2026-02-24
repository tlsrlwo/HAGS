using UnityEngine;
using UnityEngine.AI;

namespace GhostStory
{
    public class NpcAnimationController : MonoBehaviour
    {
        public enum AnimState
        {
            Idle = 0,
            Walk = 1
        }

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Animator _anim;

        [Header("이동값")]
        private float lastX = 0f;
        private float lastZ = -1f;

        // 파라미터 해시값 (메모리 및 성능 최적화)
        private readonly int hashXInput = Animator.StringToHash("xInput");
        private readonly int hashZInput = Animator.StringToHash("zInput");
        private readonly int hashState = Animator.StringToHash("State");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            UpdateAnimState();
        }

        private void UpdateAnimState()
        {
            Vector3 velocity = _agent.velocity;

            bool isMoving = velocity.sqrMagnitude > 0.1f;

            if (isMoving)
            {
                Vector3 dir = velocity.normalized;

                // 방향값 업데이트
                _anim.SetFloat(hashXInput, dir.x);
                _anim.SetFloat(hashZInput, dir.z);

                // 움직임이 있으면 상태를 walk 로 변경
                _anim.SetInteger(hashState, (int)AnimState.Walk);

                lastX = dir.x;
                lastZ = dir.z;
            }
            else
            {
                // 이동을 멈췄을 때 State 변경, 마지막 이동방향으로 시선 방향 설정
                _anim.SetInteger(hashState, (int)AnimState.Idle);
                _anim.SetFloat(hashXInput, lastX);
                _anim.SetFloat(hashZInput, lastZ);
            }
        }

        public void LookAtPlayer(Vector3 playerPosition)
        {
            // 플레이어가 어느 방향에 있는지 계산
            Vector3 dir = (playerPosition - transform.position).normalized;

            // 애니메이션 적용
            _anim.SetFloat(hashXInput, dir.x);
            _anim.SetFloat(hashZInput, dir.z);

            // Idle 로 전환
            _anim.SetInteger(hashState, (int)AnimState.Idle);

            lastX = dir.x;
            lastZ = dir.z;
        }
    }
}
