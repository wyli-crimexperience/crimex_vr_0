using UnityEngine;



[System.Serializable]
public class Dialogue {
    public string speakerName, speakerText; 
}

[CreateAssetMenu(fileName = "ConversationData", menuName = "Scriptable Objects/Conversation Data")]
public class DialogueData : ScriptableObject {

    [SerializeField] private Dialogue[] dialogue;
    public Dialogue[] Dialogue => dialogue;

}