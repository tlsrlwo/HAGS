using TMPro;
using UnityEngine;

namespace GhostStory
{
    [RequireComponent(typeof(BoxCollider), typeof(Animator))]
    public class Elevator : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueSO _elevatorDialogue;
        private Animator _anim;
        [SerializeField] private BoxCollider _col;

        public bool isElevatorDoorOpen { get; private set; }

        public void Awake()
        {
            isElevatorDoorOpen = false;
            _anim = GetComponent<Animator>();
            _col = GetComponent<BoxCollider>();
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

        public void OpenElevatorDoor()
        {
            if (isElevatorDoorOpen) return;

            isElevatorDoorOpen = true;
            _anim.SetBool("isOpen", isElevatorDoorOpen);
            _col.enabled = false;

            Debug.Log("[Elevator] 문이 열립니다");
        }
    }
}
