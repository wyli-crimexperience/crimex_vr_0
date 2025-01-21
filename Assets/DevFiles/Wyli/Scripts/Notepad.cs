using UnityEngine;

using TMPro;



public class Notepad : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI txtTime, txtPulse;

    private HandItem pen;



    private void Start() {
        SetTextTime("");
        SetTextPulse("");
    }

    public void SetTextTime(string str) {
        txtTime.text = str;
    }
    public void SetTextPulse(string str) {
        txtPulse.text = str;
    }

    private void OnTriggerEnter(Collider other) {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItem.TypeItem == TypeItem.Pen) {
            pen = handItem;
            ManagerGlobal.Instance.SetCanWriteNotepad(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.gameObject == pen.gameObject) {
            ManagerGlobal.Instance.SetCanWriteNotepad(false);
        }
    }

}