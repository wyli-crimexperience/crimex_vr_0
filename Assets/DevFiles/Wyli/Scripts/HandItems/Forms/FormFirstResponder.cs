using System;

using UnityEngine;

using TMPro;



public class FormFirstResponder : Form {

    [SerializeField] private TextMeshProUGUI txtDateTimeFilledUp, txtDateTimeReported, txtDateTimeFirstResponderArrived, txtDateTimeCordoned, txtDateTimeCalledTOC, txtDateTimeInvestigatorArrived, txtDateTimeInvestigatorReceived;
    [SerializeField] private TextMeshProUGUI[] txtInterviewed;



    public override void Receive() {
        txtDateTimeFilledUp.text = $"{ManagerGlobal.Instance.DateTimeFirstResponderFilledUp:MMM dd, yyyy}";
        txtDateTimeReported.text = $"{ManagerGlobal.Instance.DateTimeReported:HH}{ManagerGlobal.Instance.DateTimeReported:mm}H, {ManagerGlobal.Instance.DateTimeReported:MMM dd, yyyy}";
        txtDateTimeFirstResponderArrived.text = $"{ManagerGlobal.Instance.DateTimeFirstResponderArrived:HH}{ManagerGlobal.Instance.DateTimeFirstResponderArrived:mm}H, {ManagerGlobal.Instance.DateTimeFirstResponderArrived:MMM dd, yyyy}";

        txtDateTimeCordoned.text = ManagerGlobal.Instance.DateTimeCordoned == DateTime.MinValue ? "N/A" :
            $"{ManagerGlobal.Instance.DateTimeCordoned:HH}{ManagerGlobal.Instance.DateTimeCordoned:mm}H, {ManagerGlobal.Instance.DateTimeCordoned:MMM dd, yyyy}";

        txtDateTimeCalledTOC.text = ManagerGlobal.Instance.DateTimeCalledTOC == DateTime.MinValue ? "N/A" :
            $"{ManagerGlobal.Instance.DateTimeCalledTOC:HH}{ManagerGlobal.Instance.DateTimeCalledTOC:mm}H, {ManagerGlobal.Instance.DateTimeCalledTOC:MMM dd, yyyy}";

        // todo: only make interviewed witnesses writeable if they were actually interviewed

        txtDateTimeInvestigatorArrived.text = $"{ManagerGlobal.Instance.DateTimeInvestigatorArrived:HH}{ManagerGlobal.Instance.DateTimeInvestigatorArrived:mm}H, {ManagerGlobal.Instance.DateTimeInvestigatorArrived:MMM dd, yyyy}";

        ManagerGlobal.Instance.SetDateTimeInvestigatorReceived();
        txtDateTimeInvestigatorReceived.text = $"{ManagerGlobal.Instance.DateTimeInvestigatorReceived:HH}{ManagerGlobal.Instance.DateTimeInvestigatorReceived:mm}H, {ManagerGlobal.Instance.DateTimeInvestigatorReceived:MMM dd, yyyy}";
    }

}