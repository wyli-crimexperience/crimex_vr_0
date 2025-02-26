using System.Collections;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class Briefcase : MonoBehaviour {

    [SerializeField] private Transform briefcaseObject;
    [SerializeField] private XRSimpleInteractable lid;
    [SerializeField] private XRSocketInteractor[] sockets;
    [SerializeField] private HandItem[] items;

    private bool isGrabbingLid;
    private Transform handGrabbingLid;



    private IEnumerator Start() {
        foreach (XRSocketInteractor socket in sockets) {
            socket.socketActive = false;
        }
        yield return new WaitForEndOfFrame();

        foreach (HandItem item in items) {
            item.SocketBriefcase.socketActive = true;
            item.transform.position = item.SocketBriefcase.transform.position;
        }
        yield return new WaitForEndOfFrame();

        foreach (XRSocketInteractor socket in sockets) {
            socket.socketActive = true;
        }
    }
    private void Update() {
        if (isGrabbingLid) {
            lid.transform.localRotation = Quaternion.AngleAxis(
                StaticUtils.ClampAngle(
                    -Vector3.SignedAngle(-briefcaseObject.forward, Vector3.ProjectOnPlane(handGrabbingLid.transform.position - lid.transform.position, lid.transform.right), Vector3.forward),
                    -180, 0),
                Vector3.right);
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