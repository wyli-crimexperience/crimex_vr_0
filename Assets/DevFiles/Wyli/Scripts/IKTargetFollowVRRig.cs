using UnityEngine;

[System.Serializable] public class VRMap {
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    
    public void Map() {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class IKTargetFollowVRRig : MonoBehaviour {

    [Range(0, 1)] public float turnSmoothness = 0.1f;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    private float yaw;



    private void LateUpdate() {
        transform.position = head.ikTarget.position + headBodyPositionOffset - transform.forward * 0.05f;
        yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }

}