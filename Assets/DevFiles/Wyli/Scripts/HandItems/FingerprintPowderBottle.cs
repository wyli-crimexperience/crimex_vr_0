using UnityEngine;



public class FingerprintPowderBottle : HandItemBriefcase {

    [SerializeField] private TypeFingerprintPowder typeFingerprintPowder;
    public TypeFingerprintPowder TypeFingerprintPowder => typeFingerprintPowder;

    [SerializeField] private ParticleSystem psPowder;
    private ParticleSystem.ShapeModule psPowderShape;



    private void Awake() {
        psPowderShape = psPowder.shape;

        ParticleSystem.MainModule psPowderMain = psPowder.main;
        psPowderMain.startColor = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(TypeFingerprintPowder);
    }
    private void Update() {

        // if opened, and tilted, pour powder
        if (Vector3.Angle(transform.up, Vector3.up) > 90) {
            psPowder.Play();
            psPowderShape.rotation = new Vector3(90, Vector3.Angle(transform.forward, psPowder.transform.forward), 0);
            //psPowderShape.rotation = new Vector3(0, Vector3.Angle(transform.forward, Vector3.up), 0);
        } else {
            psPowder.Stop();
        }

    }

}