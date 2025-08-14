using UnityEngine;



public class FingerprintTapeLifted : HandItem {

    [SerializeField] private Fingerprint fingerprint;

    public Fingerprint FingerprintCurrent { get; private set; }
    public bool CanLiftFingerprint => FingerprintCurrent != null;
    public FormFingerprintSpecialist FormCurrent { get; private set; }
    public bool CanAttachToForm => FormCurrent != null && fingerprint.TypeFingerprintPowder != TypeFingerprintPowder.None;
    public TypeFingerprintPowder TypeFingerprintPowder => fingerprint.TypeFingerprintPowder;



    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        fingerprint.SetTypeFingerprintPowder(typeFingerprintPowder);
    }



    private void OnTriggerEnter(Collider other) {
        // enter fingerprint
        Fingerprint fingerprint = other.GetComponent<Fingerprint>();
        if (fingerprint != null && fingerprint.IsShowing && !fingerprint.IsDisplayOnly && fingerprint.TypeFingerprintPowder != TypeFingerprintPowder.None) {
            FingerprintCurrent = fingerprint;
        }

        // enter form
        FormFingerprintSpecialist form = other.GetComponent<FormFingerprintSpecialist>();
        if (form != null) {
            FormCurrent = form;
        }
    }
    private void OnTriggerExit(Collider other) {
        if (FingerprintCurrent != null) {
            // exit fingerprint
            Fingerprint fingerprint = other.GetComponent<Fingerprint>();
            if (fingerprint != null && FingerprintCurrent != null && fingerprint == FingerprintCurrent) {
                FingerprintCurrent = null;
            }
        }

        if (FormCurrent != null) {
            // exit form
            FormFingerprintSpecialist form = other.GetComponent<FormFingerprintSpecialist>();
            if (form != null && FormCurrent != null && form == FormCurrent) {
                FormCurrent = null;
            }
        }
    }



    public void LiftFingerprint() {
        if (CanLiftFingerprint) {
            SetTypeFingerprintPowder(FingerprintCurrent.TypeFingerprintPowder);
            FingerprintCurrent.SetTypeFingerprintPowder(TypeFingerprintPowder.None);
        }
    }
    public void AttachToForm() {
        if (CanAttachToForm) {
            FormCurrent.AttachFingerprintTapeLifted(this);
        }
    }

}