using UnityEngine;



public class LiftedFingerprint : MonoBehaviour {

    public HandItem HandItem;
    [SerializeField] private Fingerprint fingerprintDisplay;



    public void Init(Fingerprint fingerprintSource) {
        fingerprintDisplay.SetTypeFingerprintPowder(fingerprintSource.TypeFingerprintPowder);
        fingerprintDisplay.IsDisplayOnly = true;
    }

}