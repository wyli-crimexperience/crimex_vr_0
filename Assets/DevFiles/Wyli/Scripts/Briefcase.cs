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
    [Tooltip("Minimum angle (degrees) before the briefcase counts as open.")]
    [SerializeField] private float openThresholdAngle = 11.25f;

    private bool isGrabbingLid;
    private Transform handGrabbingLid;
    private float lidAngle;
    private Rigidbody rb;

    /// <summary> True if the lid is rotated past the threshold. </summary>
    public bool IsOpen => Mathf.Abs(lidAngle) > openThresholdAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private IEnumerator Start()
    {
        // Ensure the briefcase is placed correctly in hierarchy
        if (transform.parent != null && transform.parent.root != null)
            transform.parent = transform.parent.root.parent;

        // Init sockets (disabled initially)
        foreach (var socket in sockets)
        {
            if (socket == null) continue;
            socket.SetBriefcase(this);
            socket.socketActive = false;
        }

        yield return new WaitForEndOfFrame();

        // Snap items into sockets
        foreach (var item in items)
        {
            if (item == null || item.SocketBriefcase == null) continue;
            item.SocketBriefcase.socketActive = true;
            item.transform.SetPositionAndRotation(
                item.SocketBriefcase.transform.position,
                item.SocketBriefcase.transform.rotation
            );
        }

        yield return new WaitForEndOfFrame();

        // Reactivate all sockets
        foreach (var socket in sockets)
        {
            if (socket != null)
                socket.socketActive = true;
        }

        yield return new WaitForEndOfFrame();

        // Finalize item initialization
        foreach (var item in items)
        {
            if (item != null) item.InitBriefcase();
        }
    }

    private void Update()
    {
        if (!isGrabbingLid || handGrabbingLid == null) return;

        // Vector from lid pivot to hand
        Vector3 handOffset = handGrabbingLid.position - lid.transform.position;

        // Project offset onto lid plane
        Vector3 projected = Vector3.ProjectOnPlane(handOffset, lid.transform.right);

        // Calculate signed angle
        float signedAngle = Vector3.SignedAngle(
            -briefcaseObject.forward,
            projected,
            (briefcaseObject.up.y > 0 ? -Vector3.forward : Vector3.forward)
        );

        // Clamp and apply rotation
        lidAngle = StaticUtils.ClampAngle(signedAngle, -180f, 0f);
        lid.transform.localRotation = Quaternion.AngleAxis(lidAngle, Vector3.right);
    }

    public void GrabLid()
    {
        isGrabbingLid = true;

        var mgr = ManagerGlobal.Instance;
        if (mgr == null || mgr.InteractionManager == null) return;

        var interactorLeft = mgr.InteractionManager.InteractorLeft;
        var interactorRight = mgr.InteractionManager.InteractorRight;

        if (interactorLeft != null && interactorLeft.firstInteractableSelected as XRSimpleInteractable == lid)
        {
            handGrabbingLid = mgr.RoleManager.HandLeftTarget;
        }
        else if (interactorRight != null && interactorRight.firstInteractableSelected as XRSimpleInteractable == lid)
        {
            handGrabbingLid = mgr.RoleManager.HandRightTarget;
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

        foreach (var socket in sockets)
        {
            if (socket != null) socket.socketActive = !paused;
        }

        foreach (var item in items)
        {
            if (item != null) item.SetPaused(paused);
        }
    }
}
