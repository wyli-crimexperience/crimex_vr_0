using UnityEngine;



public class Wristwatch : MonoBehaviour {

    public void GazeWristwatch() {
        ManagerGlobal.Instance.GameStateManager.CheckWristwatch(gameObject);
    }

}
