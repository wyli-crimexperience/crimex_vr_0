using UnityEngine;



public class FingerprintInkRoller : HandItemBriefcase {

    private FingerprintInkingSlab fingerprintInkingSlab;



    private void OnCollisionEnter(Collision collision) {
        fingerprintInkingSlab = collision.gameObject.GetComponent<FingerprintInkingSlab>();
    }
    private void OnCollisionExit(Collision collision) {
        if (fingerprintInkingSlab != null && collision.collider.gameObject == fingerprintInkingSlab.gameObject) {
            fingerprintInkingSlab = null;
        }
    }

    public void SpreadInk() {
        if (fingerprintInkingSlab != null) {
            fingerprintInkingSlab.SpreadInk();
        }
    }

}