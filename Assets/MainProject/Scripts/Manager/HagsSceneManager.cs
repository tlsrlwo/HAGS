using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GhostStory
{
    public class HagsSceneManager : MonoBehaviour
    {
        public static HagsSceneManager Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private float _fadeInDuration = 1.0f;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 타이틀 씬 예외 처리
            if (scene.name == "TitleScene")
            {
                _fadeCanvasGroup.alpha = 0;
                return;
            }

            StartCoroutine(SetupSceneProcess());
        }

        private IEnumerator SetupSceneProcess()
        {
            _fadeCanvasGroup.alpha = 1f;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = Instantiate(_playerPrefab);
            }

            var pInput = player.GetComponent<PlayerInput>();
            if (DialogueManager.Instance != null) DialogueManager.Instance.ConnectPlayerInput(pInput);
            if (SceneSelectorUI.Instance != null) SceneSelectorUI.Instance.ConnectPlayerInput(pInput);

            // 스폰 지점 찾기
            GameObject spawnPos = GameObject.FindWithTag("StartPosition");
            if (spawnPos != null)
            {
                var charCon = player.GetComponent<CharacterController>();
                if (charCon != null) charCon.enabled = false;

                player.transform.position = spawnPos.transform.position;

                if (charCon != null) charCon.enabled = true;
            }

            // 입력 시 강제 맵 초기화
            var inputReader = player.GetComponent<PlayerInputReader>();
            if (inputReader != null)
            {
                pInput.SwitchCurrentActionMap("PlayerMap");
            }

            // 씬 안정화 대기
            yield return new WaitForSeconds(0.3f);

            StartCoroutine(FadeIn(0));
        }
        
        private IEnumerator FadeIn(float targetAlpha)
        {
            if (_fadeCanvasGroup == null) yield break;

            float startAlpha = _fadeCanvasGroup.alpha;
            float timer = 0;

            while (timer < _fadeInDuration)
            {
                timer += Time.deltaTime;

                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / _fadeInDuration);

                yield return null;
            }

            _fadeCanvasGroup.alpha = targetAlpha;

            if (targetAlpha == 0) _fadeCanvasGroup.blocksRaycasts = false;
            else _fadeCanvasGroup.blocksRaycasts = true;
        }
    }
}
