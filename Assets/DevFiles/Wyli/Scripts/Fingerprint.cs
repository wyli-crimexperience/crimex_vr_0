using UnityEngine;



public class Fingerprint : MonoBehaviour {

    [SerializeField] private GameObject objFingerprint;
    [SerializeField] private MeshRenderer mrFingerprint;
    [SerializeField] private bool isDisplayOnly;
    public bool IsDisplayOnly => isDisplayOnly;

    private Material matFingerprint;



    public bool IsLifted { get; private set; }
    public void Lift() {
        IsLifted = true;
        SetTypeFingerprintPowder(TypeFingerprintPowder.None);
    }
    public TypeFingerprintPowder TypeFingerprintPowder;



    private void Awake() {
        matFingerprint = mrFingerprint.material;
    }
    private void Start() {
        IsLifted = false;

        UpdateVisual();
    }
    private void OnTriggerEnter(Collider other) {
        if (!IsLifted && !IsDisplayOnly) {
            FingerprintBrush fingerprintBrush = other.GetComponent<FingerprintBrush>();
            if (fingerprintBrush != null && fingerprintBrush.TypeFingerprintPowder != TypeFingerprintPowder.None) {
                objFingerprint.SetActive(true);

                SetTypeFingerprintPowder(fingerprintBrush.TypeFingerprintPowder);
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
