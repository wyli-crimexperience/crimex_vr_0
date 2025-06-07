using UnityEngine;



public class FingerprintPowderBowl : HandItemBriefcase {

    [SerializeField] private TypeFingerprintPowder typeFingerprintPowder;
    public TypeFingerprintPowder TypeFingerprintPowder => typeFingerprintPowder;

    [SerializeField] private MeshRenderer mrPowder;
    private Material matPowder;



    public void SetPowder(TypeFingerprintPowder _typeFingerprintPowder) {
        typeFingerprintPowder = _typeFingerprintPowder;
        mrPowder.gameObject.SetActive(typeFingerprintPowder != TypeFingerprintPowder.None);
        matPowder.color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(TypeFingerprintPowder);
    }

    private void Awake() {
        matPowder = mrPowder.material;
    }
    private void Start() {
        SetPowder(TypeFingerprintPowder.None);
    }

}