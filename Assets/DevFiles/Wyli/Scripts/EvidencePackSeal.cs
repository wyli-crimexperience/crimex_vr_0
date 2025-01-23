using UnityEngine;



public class EvidencePackSeal : MonoBehaviour {

    [SerializeField] private GameObject containerTape, containerMarking;

    private HandItem handItemPen;



    public bool IsTaped;
    public void SetTaped(bool b) {
        IsTaped = b;
        containerTape.SetActive(b);
    }

    public bool IsMarked;
    public void SetMarked(bool b) {
        IsMarked = b;
        containerMarking.SetActive(b);
    }



    private void Start() {
        SetTaped(false);
        SetMarked(false);
    }
    private void OnTriggerEnter(Collider other) {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItem.TypeItem == TypeItem.Pen) {
            handItemPen = handItem;
            ManagerGlobal.Instance.SetCanWriteEvidencePackSeal(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItemPen != null && handItem == handItemPen) {
            ManagerGlobal.Instance.SetCanWriteEvidencePackSeal(false);
        }
    }

}