using System;
using UnityEngine;
using TMPro;

public class FormFirstResponder : Form
{
    [SerializeField]
    private TextMeshProUGUI txtDateTimeFilledUp, txtDateTimeReported,
        txtDateTimeFirstResponderArrived, txtDateTimeCordoned, txtDateTimeCalledTOC,
        txtDateTimeInvestigatorArrived, txtDateTimeInvestigatorReceived;

    [SerializeField] private TextMeshProUGUI[] txtInterviewed;

    public override void Receive()
    {
        var timeline = ManagerGlobal.Instance.TimelineManager;

        txtDateTimeFilledUp.text = FormatDate(timeline.GetEventTime(TimelineEvent.FirstResponderFilledUp));
        txtDateTimeReported.text = FormatDate(timeline.GetEventTime(TimelineEvent.Reported), "HHmmH, MMM dd, yyyy");
        txtDateTimeFirstResponderArrived.text = FormatDate(timeline.GetEventTime(TimelineEvent.FirstResponderArrived), "HHmmH, MMM dd, yyyy");
        txtDateTimeCordoned.text = FormatDate(timeline.GetEventTime(TimelineEvent.Cordoned), "HHmmH, MMM dd, yyyy", "N/A");
        txtDateTimeCalledTOC.text = FormatDate(timeline.GetEventTime(TimelineEvent.CalledTOC), "HHmmH, MMM dd, yyyy", "N/A");
        txtDateTimeInvestigatorArrived.text = FormatDate(timeline.GetEventTime(TimelineEvent.InvestigatorArrived), "HHmmH, MMM dd, yyyy");

        // Ensure InvestigatorReceived is logged
        if (!timeline.HasEvent(TimelineEvent.InvestigatorReceived))
            timeline.SetEventNow(TimelineEvent.InvestigatorReceived, timeline.GetEventTime(TimelineEvent.Incident).Value);

        txtDateTimeInvestigatorReceived.text = FormatDate(timeline.GetEventTime(TimelineEvent.InvestigatorReceived), "HHmmH, MMM dd, yyyy");
    }

    private string FormatDate(DateTime? dateTime, string format = "MMM dd, yyyy", string fallback = "")
    {
        if (dateTime == null || dateTime == DateTime.MinValue)
            return fallback;

        return dateTime.Value.ToString(format);
    }
}
