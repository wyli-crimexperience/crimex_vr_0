using UnityEngine;



public class FingerprintTapeLifted : HandItem {

    [SerializeField] private Fingerprint fingerprint;

    public TypeFingerprintPowder typeFingerprintPowder => fingerprint.TypeFingerprintPowder;



    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        fingerprint.SetTypeFingerprintPowder(typeFingerprintPowder);
    }

}
