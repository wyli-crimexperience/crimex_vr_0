using UnityEngine;
using TMPro;

public class PageSketcher : Page
{
    [SerializeField] private TextMeshProUGUI txtDateTimeFilledUp;
    [SerializeField] private GameObject goSketch;

    private bool hasWrittenDateTimeFilledUp, hasWrittenSketch;

    private void Awake()
    {
        txtDateTimeFilledUp.text = "";
        goSketch.SetActive(false);
    }

    public override void WriteNext()
    {
        base.WriteNext();

        var timeline = ManagerGlobal.Instance.TimelineManager;

        while (true)
        {
            if (!hasWrittenDateTimeFilledUp)
            {
                // Ensure event exists
                if (!timeline.HasEvent(TimelineEvent.SketchFilledUp))
                    timeline.SetEventNow(TimelineEvent.SketchFilledUp, timeline.GetEventTime(TimelineEvent.Incident).Value);

                txtDateTimeFilledUp.text = timeline.GetEventTime(TimelineEvent.SketchFilledUp)?.ToString("MMM dd, yyyy");
                hasWrittenDateTimeFilledUp = true;
                break;
            }

            if (!hasWrittenSketch)
            {
                goSketch.SetActive(true);
                hasWrittenSketch = true;
                break;
            }

            break;
        }
    }
}
