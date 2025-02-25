using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class HandItem : MonoBehaviour {

    [SerializeField] private XRGrabInteractable interactable;
    public XRGrabInteractable Interactable => interactable;
    [SerializeField] private XRSocketInteractor socketBriefcase;
    public XRSocketInteractor SocketBriefcase => socketBriefcase;

    public TypeItem TypeItem;



    private void Awake() {
        if (socketBriefcase != null) {
            socketBriefcase.startingSelectedInteractable = interactable;
        }
    }

    public void Grab() {
        ManagerGlobal.Instance.GrabInteractable(this);
    }
    public void Release() {
        ManagerGlobal.Instance.ReleaseInteractable(this);
    }

}
