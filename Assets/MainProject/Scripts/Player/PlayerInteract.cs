using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostStory
{
    public class PlayerInteract : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [SerializeField] private float interactRange = 1f;
        [SerializeField] private LayerMask interactLayer;

        [Header("디버그 색상")]
        private Color _successColor = Color.green;
        private Color _failColor = Color.red;

        private PlayerMovement _player;

        private void Awake()
        {
            _player = GetComponent<PlayerMovement>();

            if (_player == null) Debug.Log("[PlayerInteract] PlayerMovement 컴포넌트를 찾을 수 없습니다");
        }


        // PlayerMovement 에서 호출
        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                DoInteract();
            }
        }
        
        public void DoInteract()
        {
            // 플레이어 위치와, Interact 할 방향을 지정
            Vector3 startPos = transform.position + Vector3.up * 0.2f;
            Vector3 dir = new Vector3(_player.lastMoveX, 0, _player.lastMoveZ).normalized;

            if(dir.sqrMagnitude < 0.1f)
            {
                dir = transform.forward;
            }

            Ray ray = new Ray(startPos, dir);
            RaycastHit hit;

            // hit 한 것이 Interact 가능하다면
            if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
            {
                IInteractable interactable = hit.transform.gameObject.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    interactable.Interact(this.gameObject);
                    Debug.DrawRay(startPos, dir * hit.distance, _successColor, 0.5f);
                }
            }
            else
            {
                Debug.Log("[PlayerInteraction] 상호작용할 오브젝트를 찾지 못했습니다");
                Debug.DrawRay(startPos, dir * interactRange, _failColor, 0.5f);
            }
        }
    }
}
