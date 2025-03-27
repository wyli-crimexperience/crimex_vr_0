using UnityEngine;



public class BriefcaseItemCenter : MonoBehaviour {

    private XRSocketInteractorBriefcase currentSocket;
    public XRSocketInteractorBriefcase CurrentSocket => currentSocket;

    private void OnTriggerEnter(Collider other) {
        XRSocketInteractorBriefcase socket = other.GetComponent<XRSocketInteractorBriefcase>();
        if (socket != null) {
            currentSocket = socket;
        }
    }
    private void OnTriggerExit(Collider other) {
        XRSocketInteractorBriefcase socket = other.GetComponent<XRSocketInteractorBriefcase>();
        if (socket != null && currentSocket != null && socket == currentSocket) {
            currentSocket = null;
        }
    }

}