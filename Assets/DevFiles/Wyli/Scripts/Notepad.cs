using UnityEngine;

using TMPro;



public class Notepad : HandItem {

    [SerializeField] private TextMeshProUGUI txtTime, txtPulse;

    private GameObject penTip;



    private void Start() {
        SetTextTime("");
        SetTextPulse("");
    }
    private void OnCollisionEnter(Collision collision) {
        if (penTip == null && collision.collider.CompareTag("PenTip")) {
            penTip = collision.collider.gameObject;
            ManagerGlobal.Instance.SetCanWriteNotepad(true);
        }
    }
    private void OnCollisionExit(Collision collision) {
        if (collision.collider.CompareTag("PenTip") && collision.collider.gameObject == penTip) {
            ManagerGlobal.Instance.SetCanWriteNotepad(false);
            penTip = null;
        }
    }
    //private void OnTriggerEnter(Collider other) {
    //    if (penTip == null && other.CompareTag("PenTip")) {
    //        penTip = other.gameObject;
    //        ManagerGlobal.Instance.SetCanWriteNotepad(true);
    //    }
    //}
    //private void OnTriggerExit(Collider other) {
    //    if (other.CompareTag("PenTip") && other.gameObject == penTip) {
    //        ManagerGlobal.Instance.SetCanWriteNotepad(false);
    //        penTip = null;
    //    }
    //}

    public void SetTextTime(string str) {
        txtTime.text = str;
    }
    public void SetTextPulse(string str) {
        txtPulse.text = str;
    }

}