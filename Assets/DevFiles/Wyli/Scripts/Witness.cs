using UnityEngine;



public class Witness : MonoBehaviour {

    [SerializeField] private DialogueData dialogueData;
    public DialogueData DialogueData => dialogueData;

    private bool isDoneConversing;



    public void GazeWitness() {
        if (isDoneConversing) {
            ManagerGlobal.Instance.ShowThought(gameObject, "I've already talked to them...");
        } else {
            ManagerGlobal.Instance.StartConversation(this);
        }
    }
    public void DoneConversing() {
        isDoneConversing = true;
    }

}