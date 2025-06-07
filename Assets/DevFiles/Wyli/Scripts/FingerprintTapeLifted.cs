using UnityEngine;



public class FingerprintTapeLifted : HandItem {

    [SerializeField] private Fingerprint fingerprint;

    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        fingerprint.SetTypeFingerprintPowder(typeFingerprintPowder);
    }

}
