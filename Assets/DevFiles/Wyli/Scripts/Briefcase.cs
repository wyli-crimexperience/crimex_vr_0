using System.Collections;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;



public class Briefcase : MonoBehaviour {

    [SerializeField] private Transform briefcaseObject;
    [SerializeField] private XRSimpleInteractable lid;
    [SerializeField] private XRSocketInteractorBriefcase[] sockets;
    [SerializeField] private HandItem[] items;

    private bool isGrabbingLid;
    private Transform handGrabbingLid;
    private float lidAngle;
    public bool IsOpen => Mathf.Abs(lidAngle) > 11.25f;



    private IEnumerator Start() {
        transform.parent = transform.parent.root.parent;

        foreach (XRSocketInteractorBriefcase socket in sockets) {
            socket.SetBriefcase(this);
            socket.socketActive = false;
        }
        yield return new WaitForEndOfFrame();

        foreach (HandItem item in items) {
            item.SocketBriefcase.socketActive = true;
            item.transform.position = item.SocketBriefcase.transform.position;
        }
        yield return new WaitForEndOfFrame();

        foreach (XRSocketInteractorBriefcase socket in sockets) {
            socket.socketActive = true;
        }
        yield return new WaitForEndOfFrame();

        foreach (HandItem item in items) {
            item.InitBriefcase();
        }
    }
    private void Update() {
        if (isGrabbingLid) {
            lidAngle = StaticUtils.ClampAngle(
                    Vector3.SignedAngle(-briefcaseObject.forward, Vector3.ProjectOnPlane(handGrabbingLid.transform.position - lid.transform.position, lid.transform.right),
                    briefcaseObject.forward.x < 0 ? Vector3.forward : -Vector3.forward) * (briefcaseObject.up.y > 0 ? -1 : 1),
                    -180, 0);
            lid.transform.localRotation = Quaternion.AngleAxis(lidAngle, Vector3.right);
        }
    }



    public void GrabLid() {
        isGrabbingLid = true;
        if (ManagerGlobal.Instance.InteractorLeft.firstInteractableSelected as XRSimpleInteractable == lid) {
            handGrabbingLid = ManagerGlobal.Instance.HandLeftTarget;
        }
        if (ManagerGlobal.Instance.InteractorRight.firstInteractableSelected as XRSimpleInteractable == lid) {
            handGrabbingLid = ManagerGlobal.Instance.HandRightTarget;
        }
    }
    public void ReleaseLid() {
        isGrabbingLid = false;
        handGrabbingLid = null;
    }

}