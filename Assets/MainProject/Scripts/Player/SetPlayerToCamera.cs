using Unity.Cinemachine;
using UnityEngine;

namespace GhostStory
{
    public class SetPlayerToCamera : MonoBehaviour
    {
        void Start()
        {
            AssignCameraTarget();
        }

        public void AssignCameraTarget()
        {           
            var cameras = GameObject.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

            if (cameras.Length == 0)
            {
                Debug.LogError("[SetPlayerToCamera] 씬에서 Cinemachine 카메라를 찾을 수 없습니다.");
                return;
            }

            foreach (var cam in cameras)
            {

                if (cam.CompareTag("MainCamera"))
                {
                    cam.Follow = this.transform;
                    cam.LookAt = this.transform;
                    Debug.Log($"[SetPlayerToCamera] {cam.name}에 플레이어 등록 완료!");
                    return;
                }
            }

            Debug.LogWarning("[SetPlayerToCamera] MainCamera 태그를 가진 카메라를 찾지 못해 첫 번째 카메라를 할당합니다.");
        }
    }
}
