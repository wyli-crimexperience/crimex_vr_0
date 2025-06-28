using UnityEngine;
using UnityEngine.UI;



public class FingerprintRecordStrip : HandItemBriefcase {

    [SerializeField] private Image[] imgsFingerprints;
    [SerializeField] private int leftOrRight;

    private FingerprintSpoon fingerprintSpoonColliding, fingerprintSpoon;
    private int stripPosition;



    private void Start() {
        for (int i = 0; i < imgsFingerprints.Length; i++) {
            imgsFingerprints[i].sprite = ManagerGlobal.Instance.HolderData.GetSpriteFingerprintRecordStrip(leftOrRight, false, i);
        }
    }
    private void OnCollisionEnter(Collision collision) {
        fingerprintSpoonColliding = collision.gameObject.GetComponent<FingerprintSpoon>();
    }
    private void OnCollisionExit(Collision collision) {
        if (fingerprintSpoonColliding != null && collision.collider.gameObject == fingerprintSpoonColliding.gameObject) {
            fingerprintSpoonColliding = null;
        }
    }

    public void ToggleSpoon() {
        if (fingerprintSpoon == null) {
            if (fingerprintSpoonColliding != null) {
                fingerprintSpoon = fingerprintSpoonColliding;
                fingerprintSpoon.SetStrip(this);
            }
        } else {
            fingerprintSpoon.SetStrip(null);
            fingerprintSpoon = null;
        }
    }

    public void MoveStrip() {
        if (stripPosition < 4) {
            SetStripPosition(stripPosition + 1);
        } else {
            SetStripPosition(0);
        }
    }
    public void SetStripPosition(int _stripPosition) {
        stripPosition = _stripPosition;
        transform.localPosition = new Vector3(-0.0625f + stripPosition * 0.03125f, 0, 0);
    }

    public bool LiftFingerprint() {
        if (fingerprintSpoon == null) { return false; }

        imgsFingerprints[stripPosition].sprite = ManagerGlobal.Instance.HolderData.GetSpriteFingerprintRecordStrip(leftOrRight, true, stripPosition);

        return true;
    }

}