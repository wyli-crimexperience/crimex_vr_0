using UnityEngine;



public class FingerprintBrush : HandItemBriefcase {

    [SerializeField] TypeFingerprintBrush typeFingerprintBrush;
    [SerializeField] TypeFingerprintBrush TypeFingerprintBrush => typeFingerprintBrush;
    [SerializeField] private MeshRenderer mrBrushTip;

    private Material matBrushTip;

    public TypeFingerprintPowder TypeFingerprintPowder { get; private set; }



    private void Start() {
        matBrushTip = mrBrushTip.material;
    }
    private void OnCollisionEnter(Collision collision) {
        FingerprintPowderBowl fingerprintPowderBowl = collision.gameObject.GetComponent<FingerprintPowderBowl>();
        if (fingerprintPowderBowl != null) {
            SetTypeFingerprintPowder(fingerprintPowderBowl.TypeFingerprintPowder);
        }
    }

    public void SetTypeFingerprintPowder(TypeFingerprintPowder typeFingerprintPowder) {
        TypeFingerprintPowder = typeFingerprintPowder;
        matBrushTip.color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(typeFingerprintPowder);
    }

}