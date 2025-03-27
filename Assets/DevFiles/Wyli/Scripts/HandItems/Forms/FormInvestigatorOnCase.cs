using UnityEngine;

using TMPro;



public class FormInvestigatorOnCase : Form {

    [SerializeField] private TextMeshProUGUI txtDateTimeFilledUp;



    public override void Receive() {
        txtDateTimeFilledUp.text = $"{ManagerGlobal.Instance.DateTimeFirstResponderFilledUp:MMM dd, yyyy}";
    }

}