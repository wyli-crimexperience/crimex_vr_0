using UnityEngine;



public class EvidencePack : MonoBehaviour {

    [SerializeField] private GameObject containerOpen, containerClosed;

    public Evidence EvidenceCurrent { get; private set; }
    public Evidence EvidencePacked { get; private set; }



    private void Start() {
        containerOpen.SetActive(true);
        containerClosed.SetActive(false);
    }
    private void OnTriggerEnter(Collider other) {
        Evidence evidence = other.GetComponent<Evidence>();
        if (EvidencePacked == null && evidence != null && evidence.gameObject.activeInHierarchy) {
            EvidenceCurrent = evidence;
        }
    }
    private void OnTriggerExit(Collider other) {
        Evidence evidence = other.GetComponent<Evidence>();
        if (EvidencePacked == null && evidence != null && EvidenceCurrent != null && evidence == EvidenceCurrent) {
            EvidenceCurrent = null;
        }
    }



    public void PackEvidence() {
        if (EvidenceCurrent == null) { return; }



        EvidencePacked = EvidenceCurrent;

        EvidenceCurrent.gameObject.SetActive(false);
        EvidenceCurrent = null;



        containerOpen.SetActive(false);
        containerClosed.SetActive(true);
    }

}
