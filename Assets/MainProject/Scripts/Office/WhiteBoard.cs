using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostStory
{
    public class WhiteBoard : MonoBehaviour, IInteractable
    {
        [Header("카메라 설정")]
        [SerializeField] private CinemachineCamera _whiteBoardCam;
        [SerializeField] private float _focusPriority = 26f;

        [SerializeField] private bool _isFocusing = false;
        private PlayerInputReader inputReader;

        public void Interact(GameObject player)
        {
            if(inputReader == null)
            {
                inputReader = player.GetComponent<PlayerInputReader>();
                // Debug.Log("[WhiteBoard] PlayerInputReader 와 연결됨.");
            }

            if (!_isFocusing) FocusOnBoard(player);         
        }

        // 카메라 시점을 칠판 전용으로 변경
        private void FocusOnBoard(GameObject player)
        {
            _isFocusing = true;

            // 카메라의 우선순위를 높여 제어권 확보
            _whiteBoardCam.Priority = (int)_focusPriority;

            // 플레이어의 움직임 멈춤
            PlayerInput pInput = player.GetComponent<PlayerInput>();
            pInput.SwitchCurrentActionMap("PlayerUI");

            if (inputReader != null)
            {
                inputReader.OnSubmitEvent -= HandleSubmit;
                inputReader.OnSubmitEvent += HandleSubmit;
            }
        }

        // PlayerInputReader 에서 OnSubmitEvent 를 구독해서 e 키를 사용시의 동작을 연결함
        private void HandleSubmit()
        {
            // Debug.Log("[WhiteBoard] HandleSubmit 이 불림. UnfocusBoard 로직 실행예정.");
            if (_isFocusing)
            {
                UnfocusBoard();
            }
        }
        
        // 카메라를 다시 게임 카메라 시점으로 변경
        private void UnfocusBoard()
        {
            if(inputReader != null)
            {
                inputReader.OnSubmitEvent -= HandleSubmit;
            }
            _isFocusing = false;

            _whiteBoardCam.Priority = 0;

            if(inputReader != null)
            {
                PlayerInput pInput = inputReader.GetComponent<PlayerInput>();
                pInput.SwitchCurrentActionMap("PlayerMap");
            }
        }
    }
}
