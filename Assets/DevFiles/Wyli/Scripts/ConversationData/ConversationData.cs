using UnityEngine;



[CreateAssetMenu(fileName = "ConversationData", menuName = "Scriptable Objects/Conversation Data")]
public class ConversationData : ScriptableObject {

    [SerializeField] private string[] dialogue;

}
