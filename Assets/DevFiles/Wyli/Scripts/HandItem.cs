using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class HandItem : MonoBehaviour {
    // my own interactable
    [SerializeField] private XRGrabInteractable interactable;
    public XRGrabInteractable Interactable => interactable;
    // the briefcase socket i should start at
    [SerializeField] private XRSocketInteractor socketBriefcase;
    public XRSocketInteractor SocketBriefcase => socketBriefcase;
    // my briefcase center detector
    [SerializeField] private BriefcaseItemCenter briefcaseItemCenter;
    public BriefcaseItemCenter BriefcaseItemCenter => briefcaseItemCenter;
    // my item type
    public TypeItem TypeItem;



    private void Awake() {
        if (socketBriefcase != null) {
            socketBriefcase.startingSelectedInteractable = interactable;
        }
    }

    public void Grab() {
        ManagerGlobal.Instance.GrabItem(this);
    }
    public void Release() {
        ManagerGlobal.Instance.ReleaseItem(this);
    }

}
