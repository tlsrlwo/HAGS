using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostStory
{
    public class PlayerInputReader : MonoBehaviour
    {
        // Read-Only 변수들
        public Vector2 MoveInput { get; private set; }
        public bool IsRunPressed { get; private set; }

        // 이벤트 추가하
        public event Action<Vector2> OnNavigationEvent;

        // 플레이어 이동
        private void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
        }

        // 플레이어 달리기
        private void OnRun(InputValue value)
        {
            IsRunPressed = value.isPressed;
        }

        // UI 상호작용 입력 
        private void OnSubmit(InputValue value)
        {
            if (!value.isPressed) return;

            // 대화 중이라면 매니저에게 전달
            if (DialogueManager.Instance.isDialogueActive)
            {
                Debug.Log("[PlayerInputReader] Submit 입력 감지");
                DialogueManager.Instance.HandleSubmit();
            }
        }

        // UI 방향키 입력 
        private void OnNavigate(InputValue value)
        {
            // 호출되면 입력값을 전달해줄 이벤트
            OnNavigationEvent?.Invoke(value.Get<Vector2>());
        }

        private void OnEnable()
        {
            // 씬이 시작되자마자 살아있는 DialogueManager를 찾아서 playerInput을 알려줌
            if (DialogueManager.Instance != null)
            {
                // PlayerInput 컴포넌트를 넘겨줌
                DialogueManager.Instance.ConnectPlayerInput(GetComponent<PlayerInput>());
            }
        }

        private void OnDisable()
        {
            MoveInput = Vector2.zero;
            IsRunPressed = false;
        }

    }
}
