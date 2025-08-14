using UnityEngine;



public class FingerprintTapeRoll : HandItemBriefcase {

    [SerializeField] private GameObject prefabFingerprintTapeLifted;

    private FingerprintTapeLifted fingerprintTapeLifted;
    public FingerprintTapeLifted FingerprintTapeLifted => fingerprintTapeLifted;



    public void ExtendTape() {
        fingerprintTapeLifted = Instantiate(prefabFingerprintTapeLifted, transform).GetComponent<FingerprintTapeLifted>();
        fingerprintTapeLifted.SetTypeFingerprintPowder(TypeFingerprintPowder.None);
    }
    public bool CanLiftFingerprint => fingerprintTapeLifted != null && fingerprintTapeLifted.CanLiftFingerprint;
    public void LiftFingerprint() { FingerprintTapeLifted.LiftFingerprint(); }
    public bool CanAttachToForm => fingerprintTapeLifted != null && fingerprintTapeLifted.CanAttachToForm;
    public void AttachToForm() { FingerprintTapeLifted.AttachToForm(); }

}