using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;



public class HandItem : MonoBehaviour {
    // my own interactable
    [SerializeField] private XRGrabInteractable interactable;
    public XRGrabInteractable Interactable => interactable;
    // the briefcase socket i should start at
    [SerializeField] private XRSocketInteractorBriefcase socketBriefcase;
    public XRSocketInteractorBriefcase SocketBriefcase => socketBriefcase;
    // my briefcase center detector
    [SerializeField] private BriefcaseItemCenter briefcaseItemCenter;
    public BriefcaseItemCenter BriefcaseItemCenter => briefcaseItemCenter;
    // my item type
    public TypeItem TypeItem;

    private Rigidbody rb;



    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }



    public void InitBriefcase() {
        // assumes that socketBriefcase != null
        socketBriefcase.startingSelectedInteractable = interactable;
    }

    public void Grab() {
        ManagerGlobal.Instance.GrabItem(this);
    }
    public void Release() {
        ManagerGlobal.Instance.ReleaseItem(this);
        rb.isKinematic = false;
    }
    public void SetPaused(bool b) {
        rb.isKinematic = b;
    }



    public void SetSocketBriefcase(XRSocketInteractorBriefcase _socketBriefcase) {
        socketBriefcase = _socketBriefcase;
    }
}
