using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GhostStory
{
    [Serializable]
    public class SceneData
    {
        public string displayName;
        public string sceneName;
    }

    public class SceneSelectorUI : MonoBehaviour
    {
        public static SceneSelectorUI Instance { get; private set; }

        [Header("데이터 설정")]
        [SerializeField] private List<SceneData> _availableScenes;

        [Header("UI 구성 요소")]
        [SerializeField] private GameObject _uiPanel;
        [SerializeField] private List<Button> _sceneButtons;

        [Header("색상 설정")]
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;

        private int _currentSelection = 0;
        private PlayerInputReader _input;
        private PlayerInput pInput;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _uiPanel.SetActive(false);

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) _input = player.GetComponent<PlayerInputReader>();
        }

        public void SetupButtons()
        {
            for(int i = 0; i < _sceneButtons.Count; i++)
            {
                if (i < _availableScenes.Count)
                {
                    _sceneButtons[i].gameObject.SetActive(true);

                    // 자식에 있는 TMP 를 찾아 텍스트에 할당
                    var text = _sceneButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    // 표시할 씬 이름을 지정
                    if (text != null) text.text = _availableScenes[i].displayName;

                    // 해당 씬을 열음
                    string targetScene = _availableScenes[i].sceneName;
                    _sceneButtons[i].onClick.RemoveAllListeners();
                    _sceneButtons[i].onClick.AddListener(() =>
                    {
                        LoadSceneManager.Instance.LoadNextScene(targetScene);
                        Close();
                    });
                }
                else
                {
                    _sceneButtons[i].gameObject.SetActive(false);
                }
            }
        }


        public void Open()
        {
            if (_uiPanel.activeSelf) return;

            // awake 에서 찾지 못했을 경우를 대비해 다시 찾음
            if (_input == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                _input = player.GetComponent<PlayerInputReader>();
                Debug.Log("[SceneSelectorUI] : PlayerInputReader 연결완료.");

            }

            if (_input == null)
            {
                Debug.LogError("[SceneSelectorUI] : PlayerInputReader 을 찾을 수 없습니다.");
            }
            if(pInput == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                pInput = player.GetComponent<PlayerInput>();
                Debug.Log("[SceneSelectorUI] : PlayerInput 연결완료.");
            }
            if(pInput == null)
            {
                Debug.LogError("[SceneSelectorUI] : 플레이어 인풋을 찾을 수 없습니다.");
            }


            _uiPanel.SetActive(true);
            _currentSelection = 0;                  // 처음에는 항상 1번

            if (_input != null)
            {
                if (pInput != null)
                {
                    pInput.enabled = true;
                    pInput.SwitchCurrentActionMap("PlayerUI");
                    Debug.Log("[SceneSelectorUI] 플레이어 인풋을 UI 전용으로 변경하였습니다");
                }
            }

            UpdateVisuals();

            _input.OnNavigationEvent -= HandleNavigate;
            _input.OnNavigationEvent += HandleNavigate;
            _input.OnSubmitEvent -= HandleSubmit;
            _input.OnSubmitEvent += HandleSubmit;
        }

        public void ConnectPlayerInput(PlayerInput input)
        {
            pInput = input;
        }
        
        private void HandleNavigate(Vector2 direction)
        {
            Debug.Log($"[SceneSelectorUI] 입력 감지: {direction}");

            if (direction.magnitude < 0.1f) return;

            // 입력값에 따른 이동
            if (direction.y > 0.5f) _currentSelection--;
            else if (direction.y < -0.5f) _currentSelection++;

            // 최대 가능한 값을 0과 _availableScenes 사이로 설정
            _currentSelection = Math.Clamp(_currentSelection, 0, _availableScenes.Count - 1);
            UpdateVisuals();
        }

        private void HandleSubmit()
        {
            if (!_uiPanel.activeSelf) return;

            if (_currentSelection >= 0 && _currentSelection < _sceneButtons.Count)
            {
                _sceneButtons[_currentSelection].onClick.Invoke();
            }
        }

        private void UpdateVisuals()
        {
            for(int i =0; i < _sceneButtons.Count; i++)
            {
                // 리스트 요소 자체가 null 인지 체크
                if (_sceneButtons[i] == null) continue;

                // 선택된 버튼에 색상 지정
                if (_sceneButtons[i].gameObject.activeSelf)
                {
                    TextMeshProUGUI text = _sceneButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                    if (text != null)
                    {
                        text.color = (i == _currentSelection) ? _selectedColor : _defaultColor;
                    }
                }
                else
                {
                    Debug.LogError("[SceneSelectorUI] 선택지에서 TextMeshProUGUI 컴포넌트가 발견되지 않음");
                    if(_sceneButtons[i].image != null)
                    {
                        _sceneButtons[i].image.color = (i == _currentSelection) ? _selectedColor : _defaultColor;
                    }
                }
            }
        }

        public void Close()
        {
            if (_input != null)
            {
                _input.OnNavigationEvent -= HandleNavigate;
                _input.OnSubmitEvent -= HandleSubmit;
            }

            _uiPanel.SetActive(false);
        }


    }
}
