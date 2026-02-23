using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GhostStory
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public bool isDialogueActive { get; private set; }

        private NpcController _currentInteractingNpc;

        [Header("대사창 UI")]
        [SerializeField] private GameObject _dialoguePanelPrefab;


        [Header("참조")]
        [SerializeField] private PlayerInput _playerInput;


        // 참조할 prefab 내 ui 컴포넌트들
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _nameText; 
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image _npcSpriteImage;

        [Header("대화창 씹힘방지")]
        private float _lastDialogueStartTime;
        [SerializeField] private float _closeDelay = 0.15f;

        [Header("대사 변수")]
        private Queue<string> _sentences = new Queue<string>();

        [Header("타이핑 효과")]
        [SerializeField] private float _typingSpeed = 0.03f;
        private Coroutine _typingCoroutine;
        private bool _isTyping = false;
        private string _currentFullSentence;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                InitializeGlobalUI();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            _dialoguePanel.SetActive(false);
        }

        private void InitializeGlobalUI()
        {
            // UI 프리팹을 생성해서 자식으로 둠
            GameObject uiObj = Instantiate(_dialoguePanelPrefab, transform);
            uiObj.name = "DialogueCanvas_Global";

            // 생성된 UI오브젝트에서 각 필요한 요소 찾기
            _dialoguePanel = uiObj.transform.Find("DialoguePanel").gameObject; // 이름으로 찾기 예시
            _nameText = _dialoguePanel.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            _dialogueText = _dialoguePanel.transform.Find("DialogueText").GetComponent<TextMeshProUGUI>();
            _npcSpriteImage = _dialoguePanel.transform.Find("NpcImage").GetComponent<Image>();

            // 처음에는 비활성화
            _dialoguePanel.SetActive(false);
        }

        public void ConnectPlayerInput(PlayerInput input)
        {
            _playerInput = input;
        }

        public void StartDialogue(Npc npc, DialogueSO selectedDialogue)
        {
            // 플레이어를 Initialize 에서 찾지 못했을 시를 위한 방지책
            if (_playerInput == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) _playerInput = playerObj.GetComponent<PlayerInput>();                
            }

            // 그래도 없으면 로그 
            if (_playerInput == null)
            {
                Debug.LogError("[DialogueManager] 플레이어 인풋을 찾을 수 없어 대화를 시작할 수 없습니다!");
                return;
            }

            // NPC 에 DialogueSO 가 있는지 확인
            if (selectedDialogue == null)
            {
                Debug.LogWarning($"[DialogueManager] {npc.npcName}의 전달된 대사 데이터가 null입니다.");
                return;
            }

            isDialogueActive = true;
            _lastDialogueStartTime = Time.realtimeSinceStartup;             // 실제 시간 지정

            // 대사창 활성화
            if (_dialoguePanel != null) _dialoguePanel.SetActive(true);
            else Debug.LogWarning("[DialogueManager] DialoguePanel 을 찾지 못함. Null Reference");

            // 컴포넌트 할당
            _nameText.text = npc.npcName;
            _npcSpriteImage.sprite = npc.npcSprite;

            _currentInteractingNpc = npc.GetComponent<NpcController>();
            if(_currentInteractingNpc != null)
            {
                _currentInteractingNpc.OnDialogueStart();
            }

            // 플레이어의 인풋맵 변경
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("PlayerUI");
            
            Debug.Log("[DialogueManager]" + npc.npcName + " 대사 시작." );

            // _sentences 에 기존에 있던 내용 제거 후, 각 대사들 변수에 추가
            _sentences.Clear();
            foreach (string senctence in selectedDialogue.dialogueLines)
            {
                _sentences.Enqueue(senctence);
            }

            // 첫번째 대사 한 프레임 이후 출력
            StartCoroutine(StartFirstDialogueDelay());
        }

        public void DisplayNextLine(bool isFirstLine = false)
        {
            if (_isTyping)
            {
                CompleteSentence();
                return;
            }

            if (!isFirstLine)
            {
                if (Time.realtimeSinceStartup - _lastDialogueStartTime < _closeDelay) return;
            }

            // 남은 대사가 없으면 종료
            if (_sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            // 시간을 갱신해서 다음 대사로 넘어갈 때도 중복 입력을 방지함
            _lastDialogueStartTime = Time.realtimeSinceStartup;

            _currentFullSentence = _sentences.Dequeue();

            // 글자가 순차적으로 나오는 효과 추가하기
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeSentenceCoroutine(_currentFullSentence));
        }

        private IEnumerator StartFirstDialogueDelay()
        {
            yield return null;
            DisplayNextLine(true);
        }

        private IEnumerator TypeSentenceCoroutine(string sentence)
        {
            _isTyping = true;

            // 텍스트 초기화
            _dialogueText.text = "";

            foreach (char letter in sentence.ToCharArray())
            {
                _dialogueText.text += letter;
                yield return new WaitForSeconds(_typingSpeed);
            }

            _isTyping = false;
            _typingCoroutine = null;
        }


        private void CompleteSentence()
        {
            // 타이핑 중단하고 바로 전체 문장 표시
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _dialogueText.text = _currentFullSentence;
            _isTyping = false;
            _typingCoroutine = null;

            // 즉시 완성 후 바로 다음 대사로 넘어가는 걸 막기 위한 살짝의 시간 갱신
            _lastDialogueStartTime = Time.realtimeSinceStartup;

        }

        public void EndDialogue()
        {
            // UI 가 켜지고 일정 시간 안에 바로 꺼짐 방지
            if (Time.realtimeSinceStartup - _lastDialogueStartTime < _closeDelay) return;

            isDialogueActive = false;

            // 대사창 비활성화
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);

            // 시간 다시 흐르게 함
            // Time.timeScale = 1f;

            // 플레이어의 인풋맵 변경
            if (_playerInput != null)
            {
                _playerInput.SwitchCurrentActionMap("PlayerMap");
            }

            if(_currentInteractingNpc != null)
            {
                _currentInteractingNpc.OnDialogueEnd();
                _currentInteractingNpc = null;
            }

            Debug.Log("[DialogueManager] 대사 종료.");
        }
    }
}
