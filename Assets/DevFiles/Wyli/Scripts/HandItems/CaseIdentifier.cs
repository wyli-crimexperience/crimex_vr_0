using System;

using UnityEngine;

using TMPro;



public class CaseIdentifier : HandItemBriefcase {

    [SerializeField] private TextMeshProUGUI txtLabel;



    private void Start() {
        DateTime dateTimeNow = StaticUtils.DateTimeNowInEvening(ManagerGlobal.Instance.DateTimeIncident);
        txtLabel.text = $"CXP-{dateTimeNow:MM}-{dateTimeNow:yyyy}";
    }

}