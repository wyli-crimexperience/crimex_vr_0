using UnityEngine;
using TMPro;

// Notepad.cs
// This script manages the behavior of a notepad hand item in Unity.
// It allows the notepad to detect when a pen tip collides with it,
// enabling or disabling writing functionality accordingly.
// The script updates the global manager to reflect whether writing is allowed and provides
// methods to update the displayed time and pulse text fields on the notepad UI.
// It inherits from HandItemBriefcase, integrating with the briefcase and hand item system.

public class Notepad : HandItemBriefcase
{
    [SerializeField] private TextMeshProUGUI txtTime, txtPulse;
    private GameObject penTip;

    private void Start()
    {
        SetTextTime("");
        SetTextPulse("");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (penTip == null && collision.collider.CompareTag("PenTip"))
        {
            penTip = collision.collider.gameObject;
            SetCanWriteNotepad(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("PenTip") && collision.collider.gameObject == penTip)
        {
            SetCanWriteNotepad(false);
            penTip = null;
        }
    }

    private void SetCanWriteNotepad(bool canWrite)
    {
        var mgr = ManagerGlobal.Instance;
        if (mgr != null)
        {
            // If you have GameStateManager:
            // mgr.GameStateManager?.SetCanWriteNotepad(canWrite);

            // For now, use the property:
            mgr.CanWriteNotepad = canWrite;
        }
    }

    public void SetTextTime(string str)
    {
        txtTime.text = str;
    }

    public void SetTextPulse(string str)
    {
        txtPulse.text = str;
    }
}