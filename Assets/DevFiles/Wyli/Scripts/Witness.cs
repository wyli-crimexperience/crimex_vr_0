using UnityEngine;



public class Witness : MonoBehaviour {

    public void GazeWitness()
    {
        ManagerGlobal.Instance.ConverseWitness(this);
    }

}