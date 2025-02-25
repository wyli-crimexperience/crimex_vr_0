using UnityEngine;



public class Witness : MonoBehaviour {

    [SerializeField] private DialogueData dialogueData;
    public DialogueData DialogueData => dialogueData;

    private bool isDoneConversing;



    public void GazeWitness() {
        if (!isDoneConversing) {
            ManagerGlobal.Instance.ConverseWitness(this);
        }
    }
    public void DoneConversing() {
        isDoneConversing = true;
    }

}