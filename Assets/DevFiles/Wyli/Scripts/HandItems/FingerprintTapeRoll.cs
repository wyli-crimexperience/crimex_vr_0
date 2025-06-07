using UnityEngine;



public class FingerprintTapeRoll : HandItemBriefcase {

    [SerializeField] private FingerprintTapeLifted fingerprintTapeLifted;

    public Fingerprint FingerprintCurrent { get; private set; }



    private void Awake() {

        fingerprintTapeLifted.gameObject.SetActive(false);
    }
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



    public void LiftFingerprint(Fingerprint fingerprintSource) {
        fingerprintTapeLifted.SetTypeFingerprintPowder(fingerprintSource.TypeFingerprintPowder);
        fingerprintTapeLifted.gameObject.SetActive(true);
    }

}