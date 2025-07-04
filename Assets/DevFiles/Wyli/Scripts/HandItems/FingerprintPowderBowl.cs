using UnityEngine;



public class FingerprintPowderBowl : HandItemBriefcase {

    private TypeFingerprintPowder typeFingerprintPowder;
    public TypeFingerprintPowder TypeFingerprintPowder => typeFingerprintPowder;

    [SerializeField] private MeshRenderer mrPowder;
    private Material matPowder;



    protected override void Awake() {
        base.Awake();

        matPowder = mrPowder.material;
    }
    private void Start() {
        SetPowder(TypeFingerprintPowder.None);
    }

    public void SetPowder(TypeFingerprintPowder _typeFingerprintPowder) {
        typeFingerprintPowder = _typeFingerprintPowder;
        mrPowder.gameObject.SetActive(typeFingerprintPowder != TypeFingerprintPowder.None);
        matPowder.color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(TypeFingerprintPowder);
    }

    private void OnParticleCollision(GameObject other) {
        if (other.CompareTag("FingerprintPowder")) {
            FingerprintPowderBottle fingerprintPowderBottle = other.GetComponentInParent<FingerprintPowderBottle>();
            SetPowder(fingerprintPowderBottle.TypeFingerprintPowder);
        }
    }

}