using UnityEngine;

using TMPro;



public class Notepad : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI txtTime, txtPulse;

    private HandItem handItemPen;



    private void Start() {
        SetTextTime("");
        SetTextPulse("");
    }
    private void OnTriggerEnter(Collider other) {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItem.TypeItem == TypeItem.Pen) {
            handItemPen = handItem;
            ManagerGlobal.Instance.SetCanWriteNotepad(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItemPen != null && handItem == handItemPen) {
            ManagerGlobal.Instance.SetCanWriteNotepad(false);
        }
    }

    public void SetTextTime(string str) {
        txtTime.text = str;
    }
    public void SetTextPulse(string str) {
        txtPulse.text = str;
    }

}