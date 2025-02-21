using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;



public class HandItem : MonoBehaviour {

    [SerializeField] private XRGrabInteractable interactable;
    public XRGrabInteractable Interactable => interactable;

    public TypeItem TypeItem;



    public void Grab() {
        ManagerGlobal.Instance.GrabInteractable(this);
    }
    public void Release() {
        ManagerGlobal.Instance.ReleaseInteractable(this);
    }

}
