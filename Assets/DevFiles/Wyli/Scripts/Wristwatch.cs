using UnityEngine;



public class Wristwatch : MonoBehaviour {
    public void ActivateWristwatch(bool isActive) {
        ManagerGlobal.Instance.ActivateWristwatch(isActive);
    }

}
