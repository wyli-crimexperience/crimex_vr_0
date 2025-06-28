using UnityEngine;



public class FingerprintSpoon : HandItemBriefcase {

    [SerializeField] private Transform containerStrip;

    private FingerprintRecordStrip fingerprintRecordStrip;



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

        fingerprintRecordStrip.MoveStrip();
    }
    private void SetStripPosition(int _stripPosition) {
        if (fingerprintRecordStrip == null) { return; }

        fingerprintRecordStrip.SetStripPosition(_stripPosition);
    }

}