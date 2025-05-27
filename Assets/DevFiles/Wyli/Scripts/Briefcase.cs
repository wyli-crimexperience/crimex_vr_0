using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Briefcase : MonoBehaviour
{
    [Header("Briefcase References")]
    [SerializeField] private Transform briefcaseObject;
    [SerializeField] private XRSimpleInteractable lid;
    [SerializeField] private XRSocketInteractorBriefcase[] sockets;
    [SerializeField] private HandItemBriefcase[] items;

    [Header("Lid Control")]
    [Tooltip("Minimum angle to consider the briefcase open.")]
    [SerializeField] private float openThresholdAngle = 11.25f;

    private bool isGrabbingLid;
    private Transform handGrabbingLid;
    private float lidAngle;
    private Rigidbody rb;

    public bool IsOpen => Mathf.Abs(lidAngle) > openThresholdAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private IEnumerator Start()
    {
        // Detach to appropriate hierarchy level if necessary
        if (transform.parent != null && transform.parent.root != null)
            transform.parent = transform.parent.root.parent;

        // Initialize sockets
        foreach (var socket in sockets)
        {
            socket.SetBriefcase(this);
            socket.socketActive = false;
        }

        yield return new WaitForEndOfFrame();

        // Snap items to their assigned sockets
        foreach (var item in items)
        {
            if (item.SocketBriefcase != null)
            {
                item.SocketBriefcase.socketActive = true;
                item.transform.position = item.SocketBriefcase.transform.position;
            }
        }

        yield return new WaitForEndOfFrame();

        // Activate all sockets
        foreach (var socket in sockets)
        {
            socket.socketActive = true;
        }

        yield return new WaitForEndOfFrame();

        // Finalize initialization for each item
        foreach (var item in items)
        {
            item.InitBriefcase();
        }
    }

    private void Update()
    {
        if (!isGrabbingLid || handGrabbingLid == null) return;

        Vector3 handOffset = handGrabbingLid.position - lid.transform.position;
        Vector3 projected = Vector3.ProjectOnPlane(handOffset, lid.transform.right);

        float signedAngle = Vector3.SignedAngle(-briefcaseObject.forward, projected,
            briefcaseObject.forward.x < 0 ? Vector3.forward : -Vector3.forward);

        lidAngle = StaticUtils.ClampAngle(signedAngle * (briefcaseObject.up.y > 0 ? -1 : 1), -180, 0);
        lid.transform.localRotation = Quaternion.AngleAxis(lidAngle, Vector3.right);
    }

    public void GrabLid()
    {
        isGrabbingLid = true;

        var interactorLeft = ManagerGlobal.Instance.InteractorLeft;
        var interactorRight = ManagerGlobal.Instance.InteractorRight;

        if (interactorLeft.firstInteractableSelected as XRSimpleInteractable == lid)
        {
            handGrabbingLid = ManagerGlobal.Instance.HandLeftTarget;
        }
        else if (interactorRight.firstInteractableSelected as XRSimpleInteractable == lid)
        {
            handGrabbingLid = ManagerGlobal.Instance.HandRightTarget;
        }
    }

    public void ReleaseLid()
    {
        isGrabbingLid = false;
        handGrabbingLid = null;
    }

    public void SetPaused(bool paused)
    {
        if (rb != null)
            rb.isKinematic = paused;
    }
}
