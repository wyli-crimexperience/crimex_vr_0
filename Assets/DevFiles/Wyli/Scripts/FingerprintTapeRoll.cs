using UnityEngine;



public class FingerprintTapeRoll : MonoBehaviour {

    public Fingerprint FingerprintCurrent { get; private set; }



    private void OnTriggerEnter(Collider other) {
        Fingerprint fingerprint = other.GetComponent<Fingerprint>();
        if (fingerprint != null && !fingerprint.IsLifted && !fingerprint.IsDisplayOnly && fingerprint.TypeFingerprintPowder != TypeFingerprintPowder.None) {
            FingerprintCurrent = fingerprint;
        }
    }
    private void OnTriggerExit(Collider other) {
        Fingerprint fingerprint = other.GetComponent<Fingerprint>();
        if (fingerprint != null && FingerprintCurrent != null && fingerprint == FingerprintCurrent) {
            FingerprintCurrent = null;
        }
    }

}