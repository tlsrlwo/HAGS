using System.Collections.Generic;
using UnityEngine;

namespace GhostStory
{
    public class Npc : MonoBehaviour, IInteractable
    {
        public string npcName;
        public Sprite npcSprite;

        private NpcAnimationController _npcAnim;


        [Header("대사 모음")]
        public List<DialogueSO> allDialogues = new List<DialogueSO>();

        private void Awake()
        {
            _npcAnim = GetComponent<NpcAnimationController>();
        }

        public DialogueSO GetRandomDialogue()
        {
            if (allDialogues == null || allDialogues.Count == 0)
            {
                Debug.LogWarning($"[NPC] {npcName} 의 대사 목록이 비어있습니다");
                return null;
            }

            // 랜덤으로 리스트 중 하나 선택
            int randomIndex = Random.Range(0, allDialogues.Count);
            return allDialogues[randomIndex];            
        }

        // 플레이어가 Interact 하면 실행될 함수
        public void Interact(GameObject player)
        {
            //Debug.Log($"[NPC] {npcName} 플레이어와 상호작용중");

            DialogueSO selectedDialogue = GetRandomDialogue();

            if (selectedDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(this, selectedDialogue);
            }

            // Interact 와 동시에 플레이어를 향하게 방향을 돌림
            if(_npcAnim != null && player != null)
            {
                _npcAnim.LookAtPlayer(player.transform.position);
            }
        }
    }
}
