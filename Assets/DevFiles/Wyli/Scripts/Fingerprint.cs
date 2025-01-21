using UnityEngine;

public class Fingerprint : MonoBehaviour {

    [SerializeField] private GameObject objFingerprint;
    [SerializeField] private MeshRenderer mrFingerprint;

    private Material matFingerprint;



    public TypeFingerprintPowder TypeFingerprintPowder { get; private set; }

    private void Start() {
        matFingerprint = mrFingerprint.material;

        objFingerprint.SetActive(false);
    }
    private void OnTriggerEnter(Collider other) {
        FingerprintBrush fingerprintBrush = other.GetComponent<FingerprintBrush>();
        if (fingerprintBrush != null && fingerprintBrush.TypeFingerprintPowder != TypeFingerprintPowder.None) {
            objFingerprint.SetActive(true);

            SetTypeFingerprintPowder(fingerprintBrush.TypeFingerprintPowder);
        }
    }

    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        TypeFingerprintPowder = typeFingerprintPowder;
        matFingerprint.color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(typeFingerprintPowder);
    }
}
