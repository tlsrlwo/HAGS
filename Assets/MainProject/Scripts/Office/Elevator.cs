using System.Collections;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace GhostStory
{
    [RequireComponent(typeof(BoxCollider), typeof(Animator))]
    public class Elevator : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueSO _elevatorDialogue;
        [SerializeField] private Transform _entrancePoint;

        [SerializeField] private GameObject _sceneSelectUiPrefab;

        private Animator _anim;
        private BoxCollider _col;

        public bool isElevatorDoorOpen { get; private set; }

        public void Awake()
        {
            isElevatorDoorOpen = false;
            _anim = GetComponent<Animator>();
            _col = GetComponent<BoxCollider>();
        }

        public void Start()
        {
            isElevatorDoorOpen = false;

            if(_anim == null)_anim = GetComponent<Animator>();
            if (_col == null) _col = GetComponent<BoxCollider>();

            if (_anim == null) Debug.LogError("[Elevator] 애니메이션 컴포넌트를 찾지 못함.");
        }

        public void Interact(GameObject player)
        {
            if (isElevatorDoorOpen) return;

            // 매니저의 왼쪽 버튼 기능을 싹 비우고, '나(이 프리팹)'의 문 여는 함수를 연결함 
            DialogueManager.Instance.leftButton.onClick.RemoveAllListeners();
            DialogueManager.Instance.leftButton.onClick.AddListener(OpenElevatorDoor);

            // 대화 시작
            DialogueManager.Instance.StartDialogue(_elevatorDialogue);
        }

        // 엘리베이터 문이 열리는 애니메이션과 코루틴 실행
        public void OpenElevatorDoor()
        {
            Debug.Log("[Elevator] 리스너 호출됨: OpenElevatorDoor 실행");
            if (isElevatorDoorOpen) return;

            isElevatorDoorOpen = true;
            _anim.SetBool("isOpen", isElevatorDoorOpen);
            _col.enabled = false;

            Debug.Log("[Elevator] 문이 열립니다");

            StartCoroutine(MovePlayerInside());
        }

        // 플레이어를 엘리베이터 안으로 넣기
        private IEnumerator MovePlayerInside()
        {
            // 플레이어 제어권 획득
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) yield break;

            PlayerMovement playerMove = player.GetComponent<PlayerMovement>();
            CharacterController playerCon = player.GetComponent<CharacterController>();
            NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
            Animator playerAnim = player.GetComponent<Animator>();
            PlayerInput pInput = player.GetComponent<PlayerInput>();

            if (pInput != null) pInput.enabled = false;

            // 문이 열리는 시간동안 잠시 대기
            yield return new WaitForSeconds(1.2f);

            if (_entrancePoint != null && agent != null)
            {
                if (playerMove != null) playerMove.enabled = false;
                if (playerCon != null) playerCon.enabled = false;

                playerAnim.SetInteger("State", 1);

                // 목적지 설정
                agent.enabled = true;
                agent.updateRotation = false;
                agent.SetDestination(_entrancePoint.position);

                while (agent.pathPending || agent.remainingDistance > 0.15f)
                {
                    if (agent.velocity.sqrMagnitude > 0.1f)
                    {
                        Vector3 direction = agent.velocity.normalized;

                        // 플레이어의 애니메이션 갱신
                        playerAnim.SetFloat("xInput", direction.x);
                        playerAnim.SetFloat("zInput", direction.z);

                        // 정지했을 때를 위해 마지막 방향 저장
                        playerMove.lastMoveX = direction.x;
                        playerMove.lastMoveZ = direction.z;
                    }
                    yield return null;
                }

                playerAnim.SetInteger("State", 0);

                // 도착 후 처리
                agent.isStopped = true;
                agent.enabled = false;

                Debug.Log("[Elevator] 플레이어 탑승 지점으로 이동 완료.");

            }

            if (playerMove != null) playerMove.enabled = true;
            if (playerCon != null) playerCon.enabled = true;
            Debug.Log("[Elevator] 다시 플레이어 움직임 제어 가능.");

            StartCoroutine(StartSceneSelecting());
        }
        
        // 플레이어가 엘리베이터 안으로 이동하면, 씬 선택하는 ui 가 나타나는 로직
        private IEnumerator StartSceneSelecting()
        {
            yield return new WaitForSeconds(0.5f);

            if (SceneSelectorUI.Instance != null)
            {
                SceneSelectorUI.Instance.SetupButtons();
                SceneSelectorUI.Instance.Open();
            }
            else
            {
                Debug.Log("[Elevator] StartSceneSelecting() : 씬 선택 UI 를 찾지 못해, 직접 생성해냅니다."); 
                GameObject obj = Instantiate(_sceneSelectUiPrefab);
                SceneSelectorUI selector = obj.GetComponent<SceneSelectorUI>();
                selector.SetupButtons();
                selector.Open();
            }
        }
    }
}
