using UnityEngine;



public class Fingerprint : MonoBehaviour {

    [SerializeField] private GameObject objFingerprint;
    [SerializeField] private MeshRenderer mrFingerprint;

    [SerializeField] private bool isDisplayOnly;
    public bool IsDisplayOnly => isDisplayOnly;

    public TypeFingerprintPowder TypeFingerprintPowder;
    public bool IsShowing => TypeFingerprintPowder != TypeFingerprintPowder.None;

    private Material matFingerprint;

    private bool isFingertip;
    public void SetFingertip() {
        isFingertip = true;
    }



    private void Awake() {
        matFingerprint = mrFingerprint.material;
    }
    private void Start() {
        SetTypeFingerprintPowder(TypeFingerprintPowder.None);
    }

    private void OnTriggerEnter(Collider other) {
        if (IsDisplayOnly) { return; }

        if (isFingertip) {
            if (objFingerprint.activeSelf) {
                FingerprintRecordStrip fingerprintRecordStrip = other.GetComponent<FingerprintRecordStrip>();
                if (fingerprintRecordStrip != null) {
                    if (fingerprintRecordStrip.LiftFingerprint()) {
                        SetTypeFingerprintPowder(TypeFingerprintPowder.None);
                    }
                }
            } else {
                FingerprintInkingSlab fingerprintInkingSlab = other.GetComponent<FingerprintInkingSlab>();
                if (fingerprintInkingSlab != null && fingerprintInkingSlab.IsInked) {
                    SetTypeFingerprintPowder(TypeFingerprintPowder.Ink);
                }
            }
        } else {
            if (TypeFingerprintPowder == TypeFingerprintPowder.None) {
                if (other.CompareTag("BrushTip")) {
                    FingerprintBrush fingerprintBrush = other.GetComponentInParent<FingerprintBrush>();
                    if (fingerprintBrush != null && fingerprintBrush.TypeFingerprintPowder != TypeFingerprintPowder.None) {
                        SetTypeFingerprintPowder(fingerprintBrush.TypeFingerprintPowder);
                    }
                }
            }
        }
    }



    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        TypeFingerprintPowder = typeFingerprintPowder;
        UpdateVisual();
    }
    private void UpdateVisual() {
        if (TypeFingerprintPowder == TypeFingerprintPowder.None) {
            objFingerprint.SetActive(false);
        } else {
            objFingerprint.SetActive(true);
            matFingerprint.color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(TypeFingerprintPowder);
        }
    }

}
