using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;

public class VRSpectatorCameraManager : MonoBehaviour
{
    [Header("Spectator Cameras")]
    [SerializeField] private SpectatorCamera[] spectatorCameras;
    [SerializeField] private UnityEngine.Camera activeSpectatorCamera;

    [Header("VR Player Tracking")]
    [SerializeField] private Transform vrPlayerHead;
    [SerializeField] private Transform vrPlayerLeftHand;
    [SerializeField] private Transform vrPlayerRightHand;

    [Header("Display Settings")]
    [SerializeField] private RenderTexture spectatorRenderTexture;
    [SerializeField] private bool showUIOnSpectatorView = true;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    // Input action references
    private InputAction cycleCamera;
    private InputAction firstPersonToggle;
    private InputAction thirdPersonToggle;
    private InputAction[] numberKeys;

    private int currentCameraIndex = 0;

    [System.Serializable]
    public class SpectatorCamera
    {
        public string cameraName;
        public UnityEngine.Camera camera;
        public SpectatorCameraType cameraType;
        public Vector3 offset = Vector3.zero;
        public bool followPlayer = false;
        public bool lookAtPlayer = false;

        [Header("Third Person Settings")]
        public float followDistance = 3f;
        public float followHeight = 1.5f;
        public float followSpeed = 2f;
    }

    public enum SpectatorCameraType
    {
        Fixed,           // Static camera position
        ThirdPerson,     // Follows behind player
        FirstPerson,     // Matches VR headset view
        FreeCam,         // Manual control
        Cinematic        // Scripted camera movements
    }

    void Start()
    {
        InitializeSpectatorSystem();
        SetupInputActions();
    }

    void SetupInputActions()
    {
        // Setup input actions
        if (inputActions != null)
        {
            var spectatorMap = inputActions.FindActionMap("Spectator");
            if (spectatorMap != null)
            {
                cycleCamera = spectatorMap.FindAction("CycleCamera");
                firstPersonToggle = spectatorMap.FindAction("FirstPerson");
                thirdPersonToggle = spectatorMap.FindAction("ThirdPerson");

                // Enable actions and bind callbacks
                cycleCamera?.Enable();
                firstPersonToggle?.Enable();
                thirdPersonToggle?.Enable();

                cycleCamera.performed += OnCycleCamera;
                firstPersonToggle.performed += OnFirstPersonToggle;
                thirdPersonToggle.performed += OnThirdPersonToggle;
            }
        }
        else
        {
            // Fallback: Create input actions programmatically
            SetupProgrammaticInputActions();
        }
    }

    void SetupProgrammaticInputActions()
    {
        // Create input actions in code if no InputActionAsset is assigned
        cycleCamera = new InputAction("CycleCamera", binding: "<Keyboard>/space");
        firstPersonToggle = new InputAction("FirstPerson", binding: "<Keyboard>/f");
        thirdPersonToggle = new InputAction("ThirdPerson", binding: "<Keyboard>/t");

        // Number key actions for direct camera switching
        numberKeys = new InputAction[9];
        for (int i = 0; i < 9; i++)
        {
            numberKeys[i] = new InputAction($"Camera{i + 1}", binding: $"<Keyboard>/{i + 1}");
            int cameraIndex = i; // Capture for closure
            numberKeys[i].performed += ctx => ActivateSpectatorCamera(cameraIndex);
            numberKeys[i].Enable();
        }

        // Enable actions and bind callbacks
        cycleCamera.Enable();
        firstPersonToggle.Enable();
        thirdPersonToggle.Enable();

        cycleCamera.performed += OnCycleCamera;
        firstPersonToggle.performed += OnFirstPersonToggle;
        thirdPersonToggle.performed += OnThirdPersonToggle;
    }

    // Input callback methods
    void OnCycleCamera(InputAction.CallbackContext context)
    {
        CycleToNextCamera();
    }

    void OnFirstPersonToggle(InputAction.CallbackContext context)
    {
        ToggleFirstPersonView();
    }

    void OnThirdPersonToggle(InputAction.CallbackContext context)
    {
        ToggleThirdPersonView();
    }

    void InitializeSpectatorSystem()
    {
        // Find VR player components if not assigned
        if (vrPlayerHead == null)
        {
            // Try to find XR Origin (updated from XRRig)
            var xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                vrPlayerHead = xrOrigin.Camera.transform;
            }
            else
            {
                // Fallback: try to find by tag or name
                GameObject vrCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (vrCamera != null)
                {
                    vrPlayerHead = vrCamera.transform;
                }
            }
        }

        // Setup render texture for spectator view
        SetupSpectatorRenderTexture();

        // Initialize cameras
        if (spectatorCameras.Length > 0)
        {
            ActivateSpectatorCamera(0);
        }

        // Optimize cameras for performance
        OptimizeSpectatorCameras();
    }

    void SetupSpectatorRenderTexture()
    {
        if (spectatorRenderTexture == null)
        {
            // Create render texture for spectator view
            spectatorRenderTexture = new RenderTexture(1920, 1080, 24);
            spectatorRenderTexture.name = "SpectatorView";
        }

        // Assign render texture to all spectator cameras
        foreach (var specCam in spectatorCameras)
        {
            if (specCam.camera != null)
            {
                specCam.camera.targetTexture = spectatorRenderTexture;
                specCam.camera.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        UpdateActiveCameraTracking();
    }

    void UpdateActiveCameraTracking()
    {
        if (activeSpectatorCamera == null || vrPlayerHead == null) return;

        var currentSpec = spectatorCameras[currentCameraIndex];

        switch (currentSpec.cameraType)
        {
            case SpectatorCameraType.FirstPerson:
                UpdateFirstPersonCamera(currentSpec);
                break;

            case SpectatorCameraType.ThirdPerson:
                UpdateThirdPersonCamera(currentSpec);
                break;

            case SpectatorCameraType.Fixed:
                if (currentSpec.lookAtPlayer)
                {
                    activeSpectatorCamera.transform.LookAt(vrPlayerHead);
                }
                break;

            case SpectatorCameraType.FreeCam:
                // FreeCam can be controlled manually or via script
                break;

            case SpectatorCameraType.Cinematic:
                // Cinematic cameras can have custom scripted movements
                break;
        }
    }

    void UpdateFirstPersonCamera(SpectatorCamera specCam)
    {
        // Match VR headset position and rotation
        specCam.camera.transform.position = vrPlayerHead.position + specCam.offset;
        specCam.camera.transform.rotation = vrPlayerHead.rotation;
    }

    void UpdateThirdPersonCamera(SpectatorCamera specCam)
    {
        // Third person follow camera
        Vector3 targetPosition = vrPlayerHead.position - vrPlayerHead.forward * specCam.followDistance;
        targetPosition.y += specCam.followHeight;

        specCam.camera.transform.position = Vector3.Lerp(
            specCam.camera.transform.position,
            targetPosition,
            specCam.followSpeed * Time.deltaTime
        );

        if (specCam.lookAtPlayer)
        {
            specCam.camera.transform.LookAt(vrPlayerHead);
        }
    }

    public void ActivateSpectatorCamera(int index)
    {
        if (index < 0 || index >= spectatorCameras.Length) return;

        // Deactivate current camera
        if (activeSpectatorCamera != null)
        {
            activeSpectatorCamera.gameObject.SetActive(false);
        }

        // Activate new camera
        currentCameraIndex = index;
        activeSpectatorCamera = spectatorCameras[index].camera;
        activeSpectatorCamera.gameObject.SetActive(true);

        Debug.Log($"Activated spectator camera: {spectatorCameras[index].cameraName}");
    }

    public void CycleToNextCamera()
    {
        int nextIndex = (currentCameraIndex + 1) % spectatorCameras.Length;
        ActivateSpectatorCamera(nextIndex);
    }

    void ToggleFirstPersonView()
    {
        // Find first available first-person camera
        for (int i = 0; i < spectatorCameras.Length; i++)
        {
            if (spectatorCameras[i].cameraType == SpectatorCameraType.FirstPerson)
            {
                ActivateSpectatorCamera(i);
                break;
            }
        }
    }

    void ToggleThirdPersonView()
    {
        // Find first available third-person camera
        for (int i = 0; i < spectatorCameras.Length; i++)
        {
            if (spectatorCameras[i].cameraType == SpectatorCameraType.ThirdPerson)
            {
                ActivateSpectatorCamera(i);
                break;
            }
        }
    }

    // Method to get the spectator render texture for UI display
    public RenderTexture GetSpectatorRenderTexture()
    {
        return spectatorRenderTexture;
    }

    // Performance optimization method
    void OptimizeSpectatorCameras()
    {
        foreach (var specCam in spectatorCameras)
        {
            if (specCam.camera != null)
            {
                // Optimize for spectator viewing
                specCam.camera.renderingPath = RenderingPath.Forward;
                specCam.camera.allowHDR = false;

                // Check if allowMSAA property exists (Unity 2019.1+)
                try
                {
                    specCam.camera.allowMSAA = false;
                }
                catch (System.Exception)
                {
                    // Property doesn't exist in this Unity version
                    Debug.LogWarning("allowMSAA property not available in this Unity version");
                }

                // Additional optimizations
                specCam.camera.allowDynamicResolution = false;

                // Set reasonable depth for spectator cameras
                specCam.camera.depth = -10; // Lower than main camera
            }
        }
    }

    // Public methods for external control
    public void SetSpectatorCameraByName(string cameraName)
    {
        for (int i = 0; i < spectatorCameras.Length; i++)
        {
            if (spectatorCameras[i].cameraName.Equals(cameraName, System.StringComparison.OrdinalIgnoreCase))
            {
                ActivateSpectatorCamera(i);
                break;
            }
        }
    }

    public string GetCurrentCameraName()
    {
        if (currentCameraIndex >= 0 && currentCameraIndex < spectatorCameras.Length)
        {
            return spectatorCameras[currentCameraIndex].cameraName;
        }
        return "None";
    }

    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }

    public int GetTotalCameraCount()
    {
        return spectatorCameras.Length;
    }

    // Cleanup method
    void OnDestroy()
    {
        // Disable and dispose input actions
        cycleCamera?.Disable();
        firstPersonToggle?.Disable();
        thirdPersonToggle?.Disable();

        if (numberKeys != null)
        {
            foreach (var action in numberKeys)
            {
                action?.Disable();
                action?.Dispose();
            }
        }

        cycleCamera?.Dispose();
        firstPersonToggle?.Dispose();
        thirdPersonToggle?.Dispose();

        if (spectatorRenderTexture != null)
        {
            spectatorRenderTexture.Release();
        }
    }

    // Enable/disable spectator system
    public void SetSpectatorSystemEnabled(bool enabled)
    {
        if (activeSpectatorCamera != null)
        {
            activeSpectatorCamera.gameObject.SetActive(enabled);
        }
    }

    // Method to update render texture resolution at runtime
    public void UpdateSpectatorResolution(int width, int height)
    {
        if (spectatorRenderTexture != null)
        {
            spectatorRenderTexture.Release();
        }

        spectatorRenderTexture = new RenderTexture(width, height, 24);
        spectatorRenderTexture.name = "SpectatorView";

        // Reassign to all cameras
        foreach (var specCam in spectatorCameras)
        {
            if (specCam.camera != null)
            {
                specCam.camera.targetTexture = spectatorRenderTexture;
            }
        }
    }
}