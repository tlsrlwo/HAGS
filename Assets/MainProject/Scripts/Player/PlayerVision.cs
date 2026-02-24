using UnityEngine;

namespace GhostStory
{
    public class PlayerVision : MonoBehaviour
    {
        [Header("시야 거리")]
        [SerializeField] private float _viewDistance = 5f;
        [Range(0, 180)]
        [SerializeField] private float _viewAngle = 90f;


        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private LayerMask _npcLayer;

        private PlayerMovement _player;
        [SerializeField] private LJH_VisibilityHandler _lastNPC;

        private void Awake()
        {
            _player = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            FindVisibleNPC();
        }

        private void FindVisibleNPC()
        {
            // 현재 바라보는 방향 계산
            Vector3 lookDir = new Vector3(_player.lastMoveX, 0, _player.lastMoveZ).normalized;
            if (lookDir == Vector3.zero) lookDir = transform.forward;

            // 주변 반경 내의 콜라이더를 가져옴
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, _viewDistance, _npcLayer);

            LJH_VisibilityHandler currentFoundNpc = null;

            foreach (Collider col in targetsInRadius)
            {
                LJH_VisibilityHandler npcScript = col.GetComponent<LJH_VisibilityHandler>();

                if (npcScript != null)
                {
                    Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
                    float angleToTarget = Vector3.Angle(lookDir, dirToTarget);

                    // 부채꼴 각도 내에 있는지 확인
                    if (angleToTarget < _viewAngle / 2f)
                    {
                        float distToTarget = Vector3.Distance(transform.position, col.transform.position);

                        // 벽에 가려졌는지 최종 확인
                        if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, _obstacleLayer))
                        {
                            currentFoundNpc = npcScript;
                            break; // 이자헌 한명만 찾으면 돼서 break 로 루프 탈출해줌
                        }
                    }
                }
            }
            // 상태 업데이트
            if (currentFoundNpc != null)
            {
                if (_lastNPC != null && _lastNPC != currentFoundNpc)
                {
                    _lastNPC.SetHidden(false);
                }
                currentFoundNpc.SetHidden(true);
                _lastNPC = currentFoundNpc;
            }
            else
            {
                if (_lastNPC != null)
                {
                    _lastNPC.SetHidden(false);
                    _lastNPC = null;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_player == null) return;

            Gizmos.color = Color.yellow;
            Vector3 lookDir = new Vector3(_player.lastMoveX, 0, _player.lastMoveZ).normalized;
            if (lookDir == Vector3.zero) lookDir = transform.forward;

            // 부채꼴 범위 시각화
#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up,
                Quaternion.Euler(0, -_viewAngle / 2f, 0) * lookDir, _viewAngle, _viewDistance);
#endif

            Vector3 leftRay = Quaternion.Euler(0, -_viewAngle / 2f, 0) * lookDir;
            Vector3 rightRay = Quaternion.Euler(0, _viewAngle / 2f, 0) * lookDir;

            Gizmos.DrawRay(transform.position, leftRay * _viewDistance);
            Gizmos.DrawRay(transform.position, rightRay * _viewDistance);
        }
    }
}
