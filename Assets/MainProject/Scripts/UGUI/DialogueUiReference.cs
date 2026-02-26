using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GhostStory
{
    public class DialogueUiReference : MonoBehaviour
    {
        [Header("UI 컴포넌트들")]

        public GameObject dialoguePanel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        public Image npcSpriteImage;

        public GameObject choicePanel;
        public Button leftButton;
        public Button rightButton;        
    }
}

