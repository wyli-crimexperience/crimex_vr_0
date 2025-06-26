using UnityEngine;



public class FingerprintRecordStrip : HandItemBriefcase {

    private FingerprintSpoon fingerprintSpoonColliding, fingerprintSpoon;



    private void OnCollisionEnter(Collision collision) {
        fingerprintSpoonColliding = collision.gameObject.GetComponent<FingerprintSpoon>();
    }
    private void OnCollisionExit(Collision collision) {
        if (fingerprintSpoonColliding != null && collision.collider.gameObject == fingerprintSpoonColliding.gameObject) {
            fingerprintSpoonColliding = null;
        }
    }

    public void ToggleSpoon() {
        if (fingerprintSpoon == null) {
            if (fingerprintSpoonColliding != null) {
                fingerprintSpoon = fingerprintSpoonColliding;
                fingerprintSpoon.SetStrip(this);
            }
        } else {
            fingerprintSpoon.SetStrip(null);
            fingerprintSpoon = null;
        }
    }

}