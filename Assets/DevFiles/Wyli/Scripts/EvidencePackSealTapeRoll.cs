using UnityEngine;



public class EvidencePackSealTapeRoll : MonoBehaviour {

    public EvidencePackSeal EvidencePackSealCurrent { get; private set; }



    private void OnTriggerEnter(Collider other) {
        EvidencePackSeal evidencePackSeal = other.GetComponent<EvidencePackSeal>();
        if (evidencePackSeal != null && !evidencePackSeal.IsTaped) {
            EvidencePackSealCurrent = evidencePackSeal;
        }
    }
    private void OnTriggerExit(Collider other) {
        EvidencePackSeal evidencePackSeal = other.GetComponent<EvidencePackSeal>();
        if (evidencePackSeal != null && EvidencePackSealCurrent != null && evidencePackSeal == EvidencePackSealCurrent) {
            EvidencePackSealCurrent = null;
        }
    }

}