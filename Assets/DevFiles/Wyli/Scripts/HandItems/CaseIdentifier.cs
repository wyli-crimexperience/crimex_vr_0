using System;
using UnityEngine;
using TMPro;

public class CaseIdentifier : HandItemBriefcase
{
    [SerializeField] private TextMeshProUGUI txtLabel;

    private void Start()
    {
        var timeline = ManagerGlobal.Instance.TimelineManager;

        // Make sure the incident event exists
        if (!timeline.HasEvent(TimelineEvent.Incident))
            timeline.InitIncident(DateTime.Now);

        var dateTimeIncident = timeline.GetEventTime(TimelineEvent.Incident).Value;

        txtLabel.text = $"CXP-{dateTimeIncident:MM}-{dateTimeIncident:yyyy}";
    }
}
