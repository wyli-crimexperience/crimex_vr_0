using System;
using TMPro;
using UnityEngine;

public class PageFingerprintSpecialist : Page
{
    [SerializeField]
    private TextMeshProUGUI txtCaseNumber, txtNatureOfCase, txtDate, txtWeatherCondition,
        txtNameOfVictim, txtTimeOfArrival, txtTimeDatePlaceOfOccurrence, txtLocationOfFingerprint;

    [SerializeField] private GameObject goSketch;

    private bool hasWrittenCaseNumber, hasWrittenNatureOfCase, hasWrittenDate, hasWrittenWeatherCondition,
        hasWrittenNameOfVictim, hasWrittenTimeOfArrival, hasWrittenTimeDatePlaceOfOccurrence,
        hasWrittenLocationOfFingerprint, hasWrittenSketch;

    private void Awake()
    {
        txtCaseNumber.text = "";
        txtNatureOfCase.text = "";
        txtDate.text = "";
        txtWeatherCondition.text = "";
        txtNameOfVictim.text = "";
        txtTimeOfArrival.text = "";
        txtTimeDatePlaceOfOccurrence.text = "";
        txtLocationOfFingerprint.text = "";
        goSketch.SetActive(false);
    }

    public override void WriteNext()
    {
        base.WriteNext();

        var timeline = ManagerGlobal.Instance.TimelineManager;

        while (true)
        {
            if (!hasWrittenCaseNumber)
            {
                // Ensure Incident exists
                if (!timeline.HasEvent(TimelineEvent.Incident))
                    timeline.InitIncident(DateTime.Now);

                var incident = timeline.GetEventTime(TimelineEvent.Incident).Value;
                txtCaseNumber.text = $"CXP-{incident:MM}-{incident:yyyy}";
                hasWrittenCaseNumber = true;
                break;
            }

            if (!hasWrittenNatureOfCase)
            {
                txtNatureOfCase.text = "Alleged Homicide Stabbing Incident";
                hasWrittenNatureOfCase = true;
                break;
            }

            if (!hasWrittenDate)
            {
                var incident = timeline.GetEventTime(TimelineEvent.Incident);
                txtDate.text = incident?.ToString("MMM dd, yyyy");
                hasWrittenDate = true;
                break;
            }

            if (!hasWrittenWeatherCondition)
            {
                txtWeatherCondition.text = "Fair";
                hasWrittenWeatherCondition = true;
                break;
            }

            if (!hasWrittenNameOfVictim)
            {
                txtNameOfVictim.text = "Jose Martinez";
                hasWrittenNameOfVictim = true;
                break;
            }

            if (!hasWrittenTimeOfArrival)
            {
                // Ensure InvestigatorArrived exists
                if (!timeline.HasEvent(TimelineEvent.InvestigatorArrived))
                    timeline.SetEventNow(TimelineEvent.InvestigatorArrived, timeline.GetEventTime(TimelineEvent.Incident).Value);

                txtTimeOfArrival.text = timeline.GetEventTime(TimelineEvent.InvestigatorArrived)?.ToString("MMM dd, yyyy");
                hasWrittenTimeOfArrival = true;
                break;
            }

            if (!hasWrittenTimeDatePlaceOfOccurrence)
            {
                txtTimeDatePlaceOfOccurrence.text = "#132 Legarda Rd., Baguio City";
                hasWrittenTimeDatePlaceOfOccurrence = true;
                break;
            }

            if (!hasWrittenSketch)
            {
                goSketch.SetActive(true);
                hasWrittenSketch = true;
                break;
            }

            if (!hasWrittenLocationOfFingerprint)
            {
                txtLocationOfFingerprint.text = "On the handle of the knife beside the victim";
                hasWrittenLocationOfFingerprint = true;
                break;
            }

            break;
        }
    }
}
