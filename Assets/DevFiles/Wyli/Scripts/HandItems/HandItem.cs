using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;



public class HandItem : MonoBehaviour {

    // my own interactable
    [SerializeField] protected XRGrabInteractable interactable;
    public XRGrabInteractable Interactable => interactable;
    // my item type
    public TypeItem TypeItem;

    private Rigidbody rb;



    protected virtual void Awake() {
        rb = GetComponent<Rigidbody>();
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

}
