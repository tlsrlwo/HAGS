using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        #region 참조할 prefab 내 ui 컴포넌트들
        [SerializeField] private GameObject _dialoguePanel;             // 대사창 오브젝트
        [SerializeField] private TextMeshProUGUI _nameText;             // 이름 TextMeshProUGUI
        [SerializeField] private TextMeshProUGUI _dialogueText;         // 대화내용 TextMeshProUGUI
        [SerializeField] private Image _npcSpriteImage;                 // 사진 
        #endregion


        [Header("선택지 UI")]

        #region 선택지 ui 내 참조할 컴포넌트들
        [SerializeField] private GameObject _choicePanel;      // 선택지 패널
        [SerializeField] private Button _leftButton;           // 왼쪽 버튼
        [SerializeField] private Button _rightButton;          // 오른쪽 버튼
        #endregion

        public Button leftButton => _leftButton;

        private int _currentSelection = 0;
        private bool _isChoosing = false;

        [Header("참조")]
        [SerializeField] private PlayerInput _playerInput;

        [Header("대화창 씹힘방지")]
        private float _lastDialogueStartTime;
        [SerializeField] private float _closeDelay = 0.15f;

        [Header("대사 변수")]
        private Queue<string> _sentences = new Queue<string>();
        private DialogueSO _currentDialogueSO;

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
                FindPlayerInput();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            _dialoguePanel.SetActive(false);
        }

        private void Start()
        {
            var reader = FindAnyObjectByType<PlayerInputReader>();
            if (reader != null)
            {
                reader.OnNavigationEvent += HandleNavigation;
            }
        }

        private void OnDestroy()
        {
            var reader = FindAnyObjectByType<PlayerInputReader>();
            if (reader != null)
            {
                reader.OnNavigationEvent -= HandleNavigation; // 구독 해제 [cite: 2026-02-24]
            }
        }

        private void FindPlayerInput()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                _playerInput = playerObj.GetComponent<PlayerInput>();
                Debug.Log("[DialogueManager] PlayerInput 컴포넌트 연결 완료.");
            }
            else if (playerObj == null)
            {
                Debug.LogWarning("[DialogueManager] PlayerInput 을 찾을 수 없음.");
            }
        }

        public void ConnectPlayerInput(PlayerInput input)
        {
            _playerInput = input;
        }


        private void InitializeGlobalUI()
        {
            // UI 프리팹을 생성해서 자식으로 둠
            GameObject uiObj = Instantiate(_dialoguePanelPrefab, transform);
            uiObj.name = "DialogueCanvas_Global";

            DialogueUiReference refs = uiObj.GetComponent<DialogueUiReference>();

            if (refs == null) return;

            // UI 오브젝트에서 필요한 요소 연결
            _dialoguePanel = refs.dialoguePanel;
            _nameText = refs.nameText;
            _dialogueText = refs.dialogueText;
            _npcSpriteImage = refs.npcSpriteImage;


            // 선택지 UI 오브텍트에서 필요한 요소 연결
            _choicePanel = refs.choicePanel;
            _leftButton = refs.leftButton;
            _rightButton = refs.rightButton;

            // 처음에는 비활성화
            _dialoguePanel.SetActive(false);
            _choicePanel.SetActive(false);
        }


        public void StartDialogue(DialogueSO selectedDialogue)
        {
            // NPC 자리에 null을 넣어서 기존 함수를 호출
            StartDialogue(null, selectedDialogue);
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

            // 컴포넌트 할당
            if (npc != null)
            {
                if (!string.IsNullOrEmpty(npc.npcName)) _nameText.text = npc.npcName;
                else _nameText.text = "";

                if (npc.npcSprite != null)
                {
                    _npcSpriteImage.gameObject.SetActive(true);
                    _npcSpriteImage.sprite = npc.npcSprite;
                }
                else
                {
                    _npcSpriteImage.gameObject.SetActive(false);
                }

                // npc 컨틀롤러 처리
                _currentInteractingNpc = npc.GetComponent<NpcController>();

                if (_currentInteractingNpc != null)
                {
                    _currentInteractingNpc.OnDialogueStart();
                }
            }
            else
            {
                _nameText.text = "";
                _npcSpriteImage.gameObject.SetActive(false);
                _currentInteractingNpc = null;
            }

            // 플레이어의 인풋맵 변경
            if (_playerInput != null) _playerInput.SwitchCurrentActionMap("PlayerUI");

            // 현재 지정된 대화를 _currentDialogueSO 변수에 저장
            _currentDialogueSO = selectedDialogue;

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
                if (_currentDialogueSO != null && _currentDialogueSO.dialogueChoices.Length > 0)
                {
                    ShowChoiceUI();
                }
                else
                {
                    EndDialogue();
                }
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

            if (_currentInteractingNpc != null)
            {
                _currentInteractingNpc.OnDialogueEnd();
                _currentInteractingNpc = null;
            }

            _currentDialogueSO = null;

            // Debug.Log("[DialogueManager] 대사 종료.");
        }


        #region 선택지 
        public void HandleNavigation(Vector2 direction)
        {
            if (!isDialogueActive || !_isChoosing) return;

            // 좌우 입력 처리
            if (direction.x < 0) _currentSelection = 0;
            else if (direction.x > 0) _currentSelection = 1;

            UpdateSelectionVisuals();
        }

        private void UpdateSelectionVisuals()
        {
            Color selectedCol = Color.yellow;
            Color defaultCol = Color.white;

            TextMeshProUGUI leftText = _leftButton.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI rightText = _rightButton.GetComponentInChildren<TextMeshProUGUI>();            

            // 선택된 값에 따라 색상 변경
            if (leftText != null) leftText.color = (_currentSelection == 0) ? selectedCol : defaultCol;
            if (rightText != null) rightText.color = (_currentSelection == 1) ? selectedCol : defaultCol;
        }

        public void HandleSubmit()
        {
            if (!_isChoosing)
            {
                DisplayNextLine();
                return;
            }

            // 현재 선택지 index 에 따라 해당 버튼 작동
            if (_currentSelection == -1) return;

            if (_currentSelection == 0) _leftButton.onClick.Invoke();
            else if(_currentSelection == 1) _rightButton.onClick.Invoke();

            _leftButton.onClick.RemoveAllListeners();
            _rightButton.onClick.RemoveAllListeners();


            // 선택 완료 후 초기화
            _isChoosing = false;
            _choicePanel.SetActive(false);

            EndDialogue();
        }

        private void ShowChoiceUI()
        {
            _isChoosing = true;
            _choicePanel.SetActive(true);
            _currentSelection = -1; // 기본 선택을 아무것도 선택 안됨으로 초기화
            UpdateSelectionVisuals(); // 초기 하이라이트 적용 
        }
        #endregion
    }
}