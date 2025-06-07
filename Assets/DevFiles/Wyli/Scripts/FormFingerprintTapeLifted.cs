using UnityEngine;



public class FormFingerprintTapeLifted : HandItem {

    [SerializeField] private Fingerprint fingerprintDisplay;



    public void Init(Fingerprint fingerprintSource) {
        fingerprintDisplay.SetTypeFingerprintPowder(fingerprintSource.TypeFingerprintPowder);
    }

}