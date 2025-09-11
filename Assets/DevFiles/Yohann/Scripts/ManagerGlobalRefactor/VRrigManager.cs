using UnityEngine;

// VRRigManager
// Manages the VR player rig, including head, hands, and XR input targets.
// This script provides a centralized access point for VR-specific transforms
// and can handle VR rig initialization or calibration if needed.
public class VRRigManager : MonoBehaviour
{
    // Public static instance for easy singleton access
    public static VRRigManager Instance;

    // References to the VR rig's core components
    [SerializeField] private Transform vrTargetLeftHand, vrTargetRightHand, vrTargetHead;

    // Public properties to access the VR rig transforms
    public Transform VRTargetLeftHand => vrTargetLeftHand;
    public Transform VRTargetRightHand => vrTargetRightHand;
    public Transform VRTargetHead => vrTargetHead;

    private void Awake()
    {
        // Simple singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            // Optional: Don't destroy on load if this rig persists across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}