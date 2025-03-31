using UnityEngine;



public class TapeMeasure : HandItemBriefcase {

    [SerializeField] private Transform clippingPlane, tape;
    [SerializeField] private MeshRenderer mrTapeMeasureTape;

    private int phase; // 0 = none, 1 = extending, 2 = locked



    private void Update() {
        mrTapeMeasureTape.material.SetVector("_SectionPoint", clippingPlane.position + transform.forward);
        mrTapeMeasureTape.material.SetVector("_SectionPlane", -clippingPlane.up);

        if (phase == 1) {
            tape.localPosition = new Vector3(0, 0, Mathf.Min(tape.localPosition.z + Time.deltaTime, 5f));
        }
    }



    public void Activate() {
        if (phase == 0) {
            phase = 1;
        } else if (phase == 2) {
            tape.localPosition = Vector3.zero;
            phase = 0;
        }
    }
    public void Deactivate() {
        if (phase == 1) {
            phase = 2;
        }
    }

}