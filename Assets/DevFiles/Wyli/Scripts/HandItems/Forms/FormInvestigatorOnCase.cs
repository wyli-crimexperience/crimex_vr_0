using UnityEngine;
using TMPro;

public class FormInvestigatorOnCase : Form
{
    [SerializeField] private TextMeshProUGUI txtDateTimeFilledUp;

    public override void Receive()
    {
        var timeline = ManagerGlobal.Instance.TimelineManager;

        if (!timeline.HasEvent(TimelineEvent.InvestigatorFilledUp))
            timeline.SetEventNow(TimelineEvent.InvestigatorFilledUp, timeline.GetEventTime(TimelineEvent.Incident).Value);

        txtDateTimeFilledUp.text = timeline.GetEventTime(TimelineEvent.InvestigatorFilledUp)?.ToString("MMM dd, yyyy");
    }
}
