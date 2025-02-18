using UnityEngine;



public class Witness : MonoBehaviour {

    [SerializeField] private DialogueData dialogueData;
    public DialogueData DialogueData => dialogueData;



    public void GazeWitness() {
        ManagerGlobal.Instance.ConverseWitness(this);
    }

}