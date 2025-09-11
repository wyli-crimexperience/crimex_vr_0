using UnityEngine;



public class TouchAreaPulse : MonoBehaviour {

    private void OnTriggerEnter(Collider collider) {
        if (collider.CompareTag("Fingertip")) {
            ManagerGlobal.Instance.GameStateManager.CheckPulse(gameObject);
        }
    }

}
