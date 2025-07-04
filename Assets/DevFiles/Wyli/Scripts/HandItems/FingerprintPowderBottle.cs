using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class FingerprintPowderBottle : HandItemBriefcase {

    [SerializeField] private TypeFingerprintPowder typeFingerprintPowder;
    public TypeFingerprintPowder TypeFingerprintPowder => typeFingerprintPowder;

    [SerializeField] private ParticleSystem psPowder;
    private ParticleSystem.ShapeModule psPowderShape;

    [SerializeField] private XRSocketInteractor socket;
    [SerializeField] private Transform bottleCap;



    protected override void Awake() {
        base.Awake();

        psPowderShape = psPowder.shape;

        ParticleSystem.MainModule psPowderMain = psPowder.main;
        psPowderMain.startColor = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(TypeFingerprintPowder);

        bottleCap.parent = transform.parent;
    }

    private void Update() {

        // if opened, and tilted, pour powder
        if (!socket.hasSelection && Vector3.Angle(transform.up, Vector3.up) > 90) {
            psPowder.Play();
            // todo: figure out how to rotate the particle system such that it's always on the lower lip
            psPowderShape.rotation = new Vector3(90, Vector3.Angle(transform.forward, psPowder.transform.forward), 0);
            //psPowderShape.rotation = new Vector3(0, Vector3.Angle(transform.forward, Vector3.up), 0);
        } else {
            psPowder.Stop();
        }

    }

}