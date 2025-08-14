using UnityEngine;



public class FormFingerprintSpecialist : Form {

    [SerializeField] private Fingerprint fingerprintDisplay;



    public void Init(Fingerprint fingerprintSource) {
        fingerprintDisplay.SetTypeFingerprintPowder(TypeFingerprintPowder.None);
    }

    public void AttachFingerprintTapeLifted(FingerprintTapeLifted _fingerprintTapeLifted) {
        fingerprintDisplay.SetTypeFingerprintPowder(_fingerprintTapeLifted.TypeFingerprintPowder);
        Destroy(_fingerprintTapeLifted.gameObject);
    }

}