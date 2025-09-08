using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all time-based events for a crime scene timeline.
/// </summary>
/// 
public enum TimelineEvent
{
    Incident,
    Reported,
    FirstResponderArrived,
    Cordoned,
    CalledTOC,
    FirstResponderFilledUp,
    InvestigatorArrived,
    InvestigatorReceived,
    InvestigatorFilledUp,
    SketchFilledUp,

    // Extend with more events (e.g. EvidenceCollected, SceneReleased)
}
public class TimelineManager : MonoBehaviour
{
    private readonly Dictionary<TimelineEvent, DateTime> _events
        = new Dictionary<TimelineEvent, DateTime>();

    public DateTime? GetEventTime(TimelineEvent evt)
    {
        return _events.TryGetValue(evt, out var time) ? time : null;
    }

    public void SetEventTime(TimelineEvent evt, DateTime time)
    {
        _events[evt] = time;
    }

    public void SetEventNow(TimelineEvent evt, DateTime baseIncident)
    {
        _events[evt] = StaticUtils.DateTimeNowInEvening(baseIncident);
    }

    public bool HasEvent(TimelineEvent evt) => _events.ContainsKey(evt);

    public void RemoveEvent(TimelineEvent evt)
    {
        if (_events.ContainsKey(evt))
            _events.Remove(evt);
    }

    /// <summary>
    /// Initializes the timeline with a base incident time.
    /// </summary>
    public void InitIncident(DateTime incidentTime)
    {
        SetEventTime(TimelineEvent.Incident, incidentTime);
        SetEventTime(TimelineEvent.Reported, incidentTime.AddHours(0.25f));
        SetEventTime(TimelineEvent.FirstResponderArrived, incidentTime.AddHours(0.5f));
    }
}
