using UnityEngine;



public class FingerprintPowderBottle : HandItemBriefcase {

    [SerializeField] private TypeFingerprintPowder typeFingerprintPowder;
    public TypeFingerprintPowder TypeFingerprintPowder => typeFingerprintPowder;

    [SerializeField] private ParticleSystem psPowder;



    private void Update() {
        
        // if opened, and tilted, pour powder

    }

}