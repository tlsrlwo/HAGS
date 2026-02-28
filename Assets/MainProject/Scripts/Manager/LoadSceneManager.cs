using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GhostStory
{
    public class LoadSceneManager : MonoBehaviour
    {
        public static LoadSceneManager Instance { get; private set; }

        [Header("Fade 효과")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        public string PreviousSceneName { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 엘리베이터 탑승 등 씬 전환이 필요할 때 호출
        /// </summary>    
        public void LoadNextScene(string sceneName, float fadeDuration = 1.5f)
        {
            PreviousSceneName = SceneManager.GetActiveScene().name;

            StartCoroutine(FadeAndLoadCoroutine(sceneName, fadeDuration));
        }

        private IEnumerator FadeAndLoadCoroutine(string sceneName, float fadeDuration)
        {
            yield return StartCoroutine(FadeCoroutine(0, 1, fadeDuration));

            // 씬 로드
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                yield return null;
            }

            yield return StartCoroutine(FadeCoroutine(1, 0, fadeDuration));
        }
        
        private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                // duration 시간 동안 서서히 검정색으로 바뀌게끔
                timer += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
                yield return null;
            }
            _fadeCanvasGroup.alpha = endAlpha;
        }

    }
}
