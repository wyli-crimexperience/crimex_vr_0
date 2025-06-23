using UnityEngine;



public class FingerprintTapeRoll : HandItemBriefcase {

    [SerializeField] private GameObject prefabFingerprintTapeLifted;

    public Fingerprint FingerprintCurrent { get; private set; }
    public FormFingerprintSpecialist FormCurrent { get; private set; }

    private FingerprintTapeLifted fingerprintTapeLifted;
    public FingerprintTapeLifted FingerprintTapeLifted => fingerprintTapeLifted;



    private void OnTriggerEnter(Collider other) {
        if (fingerprintTapeLifted != null) {
            // enter fingerprint
            Fingerprint fingerprint = other.GetComponent<Fingerprint>();
            if (fingerprint != null && !fingerprint.IsLifted && !fingerprint.IsDisplayOnly && fingerprint.TypeFingerprintPowder != TypeFingerprintPowder.None) {
                FingerprintCurrent = fingerprint;
            }
        } else {
            // enter form
            FormFingerprintSpecialist form = other.GetComponent<FormFingerprintSpecialist>();
            if (form != null) {
                FormCurrent = form;
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        if (fingerprintTapeLifted != null) {
            // exit fingerprint
            Fingerprint fingerprint = other.GetComponent<Fingerprint>();
            if (fingerprint != null && FingerprintCurrent != null && fingerprint == FingerprintCurrent) {
                FingerprintCurrent = null;
            }
        } else {
            // exit form
            FormFingerprintSpecialist form = other.GetComponent<FormFingerprintSpecialist>();
            if (form != null && FormCurrent != null && form == FormCurrent) {
                FormCurrent = null;
            }
        }
    }



    public void ExtendTape() {
        fingerprintTapeLifted = Instantiate(prefabFingerprintTapeLifted, transform).GetComponent<FingerprintTapeLifted>();
        fingerprintTapeLifted.SetTypeFingerprintPowder(TypeFingerprintPowder.None);
    }
    public void LiftFingerprint() {
        fingerprintTapeLifted.SetTypeFingerprintPowder(FingerprintCurrent.TypeFingerprintPowder);
        FingerprintCurrent.Lift();
    }
    public void AttachToForm() {
        FormCurrent.AttachFingerprintTapeLifted(fingerprintTapeLifted);
    }

}