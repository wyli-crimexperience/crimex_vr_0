using UnityEngine;



public class FingerprintBrush : HandItemBriefcase {

    [SerializeField] private MeshRenderer mrBrushTip;

    private Material matBrushTip;

    public TypeFingerprintPowder TypeFingerprintPowder { get; private set; }



    private void Start() {
        matBrushTip = mrBrushTip.material;
    }
    private void OnTriggerEnter(Collider other) {
        FingerprintPowder fingerprintPowder = other.GetComponent<FingerprintPowder>();
        if (fingerprintPowder != null) {
            SetTypeFingerprintPowder(fingerprintPowder.TypeFingerprintPowder);
        }
    }

    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        TypeFingerprintPowder = typeFingerprintPowder;
        matBrushTip.color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(typeFingerprintPowder);
    }

}