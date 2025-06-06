using UnityEngine;



public class PoliceTapeRoll : HandItemBriefcase {

    [SerializeField] private GameObject prefabPoliceTape;

    private PoliceTape policeTapeCurrent;
    private bool canTape, isTaping;
    private Vector3 posPoliceTapeStart, tapeBetween, tapeScale, tapeRot;
    private float tapeDist;



    private void OnCollisionEnter(Collision collision) {
        canTape = true;
    }
    private void OnCollisionExit(Collision collision) {
        canTape = false;
    }
    private void Update() {
        if (policeTapeCurrent != null && isTaping) {
            tapeBetween = transform.position - posPoliceTapeStart;
            policeTapeCurrent.transform.position = posPoliceTapeStart + (tapeBetween * 0.5f);

            tapeDist = tapeBetween.magnitude;
            tapeScale = policeTapeCurrent.transform.localScale;
            tapeScale.z = tapeDist;
            policeTapeCurrent.transform.localScale = tapeScale;

            tapeRot = Quaternion.LookRotation(tapeBetween.normalized).eulerAngles;
            tapeRot.z = -90;
            policeTapeCurrent.transform.eulerAngles = tapeRot;
        }
    }



    public void TriggerTape() {
        if (!canTape) { return; }



        if (policeTapeCurrent == null) {
            policeTapeCurrent = Instantiate(prefabPoliceTape, ManagerGlobal.Instance.ContainerPoliceTape).GetComponent<PoliceTape>();
            posPoliceTapeStart = transform.position;
            isTaping = true;
        } else {
            policeTapeCurrent = null;
            isTaping = false;
        }
    }

}