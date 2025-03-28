using UnityEngine;



public class TapeMeasure : HandItemBriefcase {

    [SerializeField] private Transform clippingPlane;
    [SerializeField] private MeshRenderer mrTapeMeasureTape;



    private void Update() {
        mrTapeMeasureTape.material.SetVector("_SectionPoint", clippingPlane.position + transform.forward);
        mrTapeMeasureTape.material.SetVector("_SectionPlane", -clippingPlane.up);
    }

}