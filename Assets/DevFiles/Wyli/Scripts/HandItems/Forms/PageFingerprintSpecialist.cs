using System;
using TMPro;
using UnityEngine;
using static Unity.Tutorials.Core.Editor.RichTextParser;



public class PageFingerprintSpecialist : Page {

    [SerializeField] private TextMeshProUGUI txtCaseNumber, txtNatureOfCase, txtDate, txtWeatherCondition, txtNameOfVictim, txtTimeOfArrival, txtTimeDatePlaceOfOccurrence, txtLocationOfFingerprint;
    [SerializeField] private GameObject goSketch;

    private bool hasWrittenCaseNumber, hasWrittenNatureOfCase, hasWrittenDate, hasWrittenWeatherCondition, hasWrittenNameOfVictim, hasWrittenTimeOfArrival, hasWrittenTimeDatePlaceOfOccurrence, hasWrittenLocationOfFingerprint, hasWrittenSketch;



    private void Awake() {
        txtCaseNumber.text = "";
        txtNatureOfCase.text = "";
        txtDate.text = "";
        txtWeatherCondition.text = "";
        txtNameOfVictim.text = "";
        txtTimeOfArrival.text = "";
        txtTimeDatePlaceOfOccurrence.text = "";
        goSketch.SetActive(false);
        txtLocationOfFingerprint.text = "";
    }



    public override void WriteNext() {
        base.WriteNext();


        while (true) {
            if (!hasWrittenCaseNumber) {
                DateTime dateTimeNow = StaticUtils.DateTimeNowInEvening(ManagerGlobal.Instance.DateTimeIncident);
                txtCaseNumber.text = $"CXP-{dateTimeNow:MM}-{dateTimeNow:yyyy}";
                hasWrittenCaseNumber = true;
                break;
            }
            if (!hasWrittenNatureOfCase) {
                txtNatureOfCase.text = "Alleged Homicide Stabbing Incident";
                hasWrittenNatureOfCase = true;
                break;
            }
            if (!hasWrittenDate) {
                txtDate.text = $"{StaticUtils.DateTimeNowInEvening(ManagerGlobal.Instance.DateTimeIncident):MMM dd, yyyy}";
                hasWrittenDate = true;
                break;
            }
            if (!hasWrittenWeatherCondition) {
                txtWeatherCondition.text = "Fair";
                hasWrittenWeatherCondition = true;
                break;
            }
            if (!hasWrittenNameOfVictim) {
                txtNameOfVictim.text = "Jose Martinez";
                hasWrittenNameOfVictim = true;
                break;
            }
            if (!hasWrittenTimeOfArrival) {
                txtTimeOfArrival.text = $"{StaticUtils.DateTimeNowInEvening(ManagerGlobal.Instance.DateTimeInvestigatorArrived):MMM dd, yyyy}";
                hasWrittenTimeOfArrival = true;
                break;
            }
            if (!hasWrittenTimeDatePlaceOfOccurrence) {
                txtTimeDatePlaceOfOccurrence.text = "#132 Legarda Rd., Baguio City";
                hasWrittenTimeDatePlaceOfOccurrence = true;
                break;
            }
            if (!hasWrittenSketch) {
                goSketch.SetActive(true);
                hasWrittenSketch = true;
                break;
            }
            if (!hasWrittenLocationOfFingerprint) {
                txtLocationOfFingerprint.text = "On the handle of the knife beside the victim";
                hasWrittenLocationOfFingerprint = true;
                break;
            }
            break;
        }
    }

}