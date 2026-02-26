using UnityEngine;

namespace GhostStory
{
    public class CityParallaxBackground : MonoBehaviour
    {
        [Header("카메라")]
        [SerializeField] private Transform cam;
        private Vector3 _cameraLastPos;

        [Header("변수")]
        [SerializeField] private float parallxValue;
        [SerializeField] private float negativeValue;

        private void Awake()
        {
            if (cam == null) cam = Camera.main.transform;

            _cameraLastPos = cam.position;
        }


        private void LateUpdate()
        {
            // 카메라가 움직인 양 
            Vector3 deltaMovement = cam.position - _cameraLastPos;

            // 배경을 카메라와 반대되는 방향으로 이동
            transform.position += new Vector3(deltaMovement.x * parallxValue, 0,deltaMovement.z * parallxValue * negativeValue);

            _cameraLastPos = cam.position;
        }
    }
}
