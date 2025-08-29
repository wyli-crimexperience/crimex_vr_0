using System;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    private DateTime dateTimeIncident;

    private DateTime dateTimeReported;
    private DateTime dateTimeFirstResponderArrived;
    private DateTime dateTimeCordoned;
    private DateTime dateTimeCalledTOC;
    private DateTime dateTimeFirstResponderFilledUp;
    private DateTime dateTimeInvestigatorArrived;
    private DateTime dateTimeInvestigatorReceived;
    private DateTime dateTimeInvestigatorFilledUp;

    // Properties
    public DateTime DateTimeIncident => dateTimeIncident;
    public DateTime DateTimeReported => dateTimeReported;
    public DateTime DateTimeFirstResponderArrived => dateTimeFirstResponderArrived;
    public DateTime DateTimeCordoned => dateTimeCordoned;
    public DateTime DateTimeCalledTOC => dateTimeCalledTOC;
    public DateTime DateTimeFirstResponderFilledUp => dateTimeFirstResponderFilledUp;
    public DateTime DateTimeInvestigatorArrived => dateTimeInvestigatorArrived;
    public DateTime DateTimeInvestigatorReceived => dateTimeInvestigatorReceived;
    public DateTime DateTimeInvestigatorFilledUp => dateTimeInvestigatorFilledUp;

    public void Init()
    {
        // Scene 1 default setup
        dateTimeIncident = DateTime.Now.AddHours(-0.5f);
        dateTimeReported = dateTimeIncident.AddHours(0.25f);
        dateTimeFirstResponderArrived = dateTimeReported.AddHours(0.25f);
    }

    // Setters (called by ManagerGlobal or gameplay scripts)
    public void SetDateTimeFirstResponderArrived()
    {
        dateTimeFirstResponderArrived = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }

    public void SetDateTimeCordoned()
    {
        dateTimeCordoned = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }

    public void SetDateTimeCalledTOC()
    {
        dateTimeCalledTOC = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }

    public void SetDateTimeFirstResponderFilledUp()
    {
        dateTimeFirstResponderFilledUp = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }

    public void SetDateTimeInvestigatorArrived()
    {
        dateTimeInvestigatorArrived = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }

    public void SetDateTimeInvestigatorReceived()
    {
        dateTimeInvestigatorReceived = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }

    public void SetDateTimeInvestigatorFilledUp()
    {
        dateTimeInvestigatorFilledUp = StaticUtils.DateTimeNowInEvening(dateTimeIncident);
    }
}
