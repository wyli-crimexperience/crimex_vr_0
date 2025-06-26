using UnityEngine;



public class FingerprintSpoon : HandItemBriefcase {

    [SerializeField] private Transform containerStrip;

    private FingerprintRecordStrip fingerprintRecordStrip;
    private int stripPosition;



    private void OnCollisionEnter(Collision collision) {
        fingerprintRecordStrip = collision.gameObject.GetComponent<FingerprintRecordStrip>();
    }
    private void OnCollisionExit(Collision collision) {
        if (fingerprintRecordStrip != null && collision.collider.gameObject == fingerprintRecordStrip.gameObject) {
            fingerprintRecordStrip = null;
        }
    }

    // to be called only from FingerprintRecordStrip.cs
    public void SetStrip(FingerprintRecordStrip _fingerprintRecordStrip) {
        fingerprintRecordStrip = _fingerprintRecordStrip;

        if (fingerprintRecordStrip == null) { return; }

        fingerprintRecordStrip.transform.parent = containerStrip;
        SetStripPosition(0);
    }
    public void MoveStrip() {
        if (fingerprintRecordStrip == null) { return; }

        if (stripPosition < 4) {
            SetStripPosition(stripPosition + 1);
        } else {
            SetStripPosition(0);
        }
    }
    private void SetStripPosition(int _stripPosition) {
        if (fingerprintRecordStrip == null) { return; }

        stripPosition = _stripPosition;
        fingerprintRecordStrip.transform.localPosition = new Vector3(-0.0625f + stripPosition * 0.03125f, 0, 0);
    }

}