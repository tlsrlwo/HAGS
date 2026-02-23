using UnityEngine;

namespace GhostStory
{
    public enum DialogueAction
    {
        None,
        Accepted,
        Declined,
        CompletedQuest,
        TriggerEvent
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "NpcDialogue/Dialogue/DialogueSO")]
    public class DialogueSO : ScriptableObject
    {
        [Header("대화 내용")]
        [TextArea(3, 5)]
        public string[] dialogueLines;

        [Header("선택지")]
        public string[] dialogueChoices;
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public DialogueSO nextDialogue;
        public DialogueAction action;

        [Header("보상")]
        // public Item item;
        public int itemCount = 1; 
    }
}
