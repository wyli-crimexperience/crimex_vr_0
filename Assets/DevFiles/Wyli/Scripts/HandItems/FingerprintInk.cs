using UnityEngine;



public class FingerprintInk : HandItemBriefcase {

    private FingerprintInkingSlab fingerprintInkingSlab;



    private void OnCollisionEnter(Collision collision) {
        fingerprintInkingSlab = collision.gameObject.GetComponent<FingerprintInkingSlab>();
    }
    private void OnCollisionExit(Collision collision) {
        if (fingerprintInkingSlab != null && collision.gameObject == fingerprintInkingSlab.gameObject) {
            fingerprintInkingSlab = null;
        }
    }

    public void ApplyInk() {
        if (fingerprintInkingSlab != null) {
            fingerprintInkingSlab.ApplyInk();
        }
    }

}