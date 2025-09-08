using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;



public class FingerprintSpoon : HandItemBriefcase {

    [SerializeField] private Transform containerStrip;

    private FingerprintRecordStrip fingerprintRecordStrip;
    public bool HasStrip => fingerprintRecordStrip != null;



    // to be called only from FingerprintRecordStrip.cs
    public void SetStrip(FingerprintRecordStrip _fingerprintRecordStrip) {
        fingerprintRecordStrip = _fingerprintRecordStrip;

        if (fingerprintRecordStrip == null) { return; }

        StartCoroutine(IE_SetStrip());
    }
    private IEnumerator IE_SetStrip() {
        yield return new WaitForEndOfFrame();

        fingerprintRecordStrip.SetKinematic(true);
        fingerprintRecordStrip.transform.parent = containerStrip;
        fingerprintRecordStrip.transform.localRotation = Quaternion.Euler(Vector3.zero);
        SetStripPosition(0);
    }
    public void MoveStrip() {
        if (fingerprintRecordStrip == null) { return; }

        fingerprintRecordStrip.MoveStrip();
    }
    private void SetStripPosition(int _stripPosition) {
        if (fingerprintRecordStrip == null) { return; }

        fingerprintRecordStrip.SetStripPosition(_stripPosition);
    }

}