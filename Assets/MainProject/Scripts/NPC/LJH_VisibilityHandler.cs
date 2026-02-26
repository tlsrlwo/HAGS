using UnityEngine;
using UnityEngine.U2D.Animation;

namespace GhostStory
{
    public class LJH_VisibilityHandler : MonoBehaviour
    {
        private SpriteResolver _sResolver;
        private bool _shouldBeHidden = false;

        private void Awake()
        {
            _sResolver = GetComponent<SpriteResolver>();
        }


        // sprite 를 Hidden 버전으로 교체하는 함수
        public void SetHidden(bool isHidden)
        {
            _shouldBeHidden = isHidden;
        }

        private void LateUpdate()
        {
            if (_sResolver == null) return;

            string currentCat = _sResolver.GetCategory();
            string currentLabel = _sResolver.GetLabel();

            // 카테고리 이름이 비어있는 예외 처리
            if (string.IsNullOrEmpty(currentCat)) return;


            if (_shouldBeHidden)
            {
                // 현재 카테고리에 H_ 가 없다면 붙여서 변경
                if (!currentCat.StartsWith("H_"))
                {
                    string targetCat = "H_" + currentCat;
                    _sResolver.SetCategoryAndLabel(targetCat, currentLabel);
                    // Debug.Log($"[LateUpdate] {currentCat} -> {targetCat} 교체 완료");
                }
            }
            else
            {
                if (currentCat.StartsWith("H_"))
                {
                    string targetCat = currentCat.Replace("H_", "");
                    _sResolver.SetCategoryAndLabel(targetCat, currentLabel);
                    // Debug.Log($"[LateUpdate] {currentCat} -> {targetCat} 복구 완료");
                }
            }
        }
    }
}