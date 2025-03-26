using UnityEngine;

using TMPro;



public class FormInvestigatorOnCase : Form {

    [SerializeField] private TextMeshProUGUI txtDateTimeFilledUp;



    public void Receive() {
        txtDateTimeFilledUp.text = $"{ManagerGlobal.Instance.DateTimeFirstResponderFilledUp:MMM dd, yyyy}";
    }

}