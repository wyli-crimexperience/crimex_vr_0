using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HandItem : MonoBehaviour
{
    [SerializeField] protected XRGrabInteractable interactable;
    public XRGrabInteractable Interactable => interactable;

    public TypeItem TypeItem;

    private Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Grab()
    {
        if (ManagerGlobal.Instance != null && ManagerGlobal.Instance.InteractionManager != null)
        {
            ManagerGlobal.Instance.InteractionManager.GrabItem(this);
        }
    }

    public virtual void Release()
    {
        if (ManagerGlobal.Instance != null && ManagerGlobal.Instance.InteractionManager != null)
        {
            ManagerGlobal.Instance.InteractionManager.ReleaseItem(this);
        }

        rb.isKinematic = false;
    }

    public void SetPaused(bool b)
    {
        SetKinematic(b);
    }
    public void SetKinematic(bool b) {
        rb.isKinematic = b;
    }
}
