using UnityEngine;

using TMPro;



public class PageFingerprintSpecialist : Page {

    [SerializeField] private TextMeshProUGUI txtDateTimeFilledUp;
    [SerializeField] private GameObject goSketch;

    private bool hasWrittenDateTimeFilledUp, hasWrittenSketch;



    private void Awake() {
        txtDateTimeFilledUp.text = "";
        goSketch.SetActive(false);
    }



    public override void WriteNext() {
        base.WriteNext();


        while (true) {
            if (!hasWrittenDateTimeFilledUp/* && hasCheckedTime*/) {
                txtDateTimeFilledUp.text = $"{StaticUtils.DateTimeNowInEvening(ManagerGlobal.Instance.DateTimeIncident):MMM dd, yyyy}";
                hasWrittenDateTimeFilledUp = true;
                break;
            }
            if (!hasWrittenSketch) {
                goSketch.SetActive(true);
                hasWrittenSketch = true;
                break;
            }
            break;
        }
    }

}