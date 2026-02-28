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
            var vCam = GameObject.FindAnyObjectByType<CinemachineCamera>();

            if (vCam != null)
            {
                vCam.Follow = this.transform;
                vCam.LookAt = this.transform;
            }
            else
            {
                Debug.LogError("SetPlayerToCamera] 현재 씬에서 Cinemachine 카메라를 찾을 수 없습니다");
            }
        }
    }
}
