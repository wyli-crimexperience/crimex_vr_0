using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float volumeRange = 0.2f;
    [SerializeField] private float pitchRange = 0.1f;

    [Header("Ground Detection Settings")]
    [SerializeField] private LayerMask groundLayerMask = -1; // Changed to detect all layers by default
    [SerializeField] private float raycastDistance = 2f; // Increased distance
    [SerializeField] private float raycastOffset = 0.1f; // Start raycast slightly above ground
    [SerializeField] private bool useMultipleRaycasts = true; // Cast multiple rays for better detection

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showDebugRaycast = true;
    [SerializeField] private bool forceGrounded = false; // Emergency override for testing

    private CharacterController characterController;
    private float stepTimer;
    private Vector3 lastPosition;
    private bool wasGroundedLastFrame;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        lastPosition = transform.position;

        // Validation checks
        if (footstepClips.Length == 0)
        {
            Debug.LogError($"[FootstepController] No footstep clips assigned to {gameObject.name}!");
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"[FootstepController] Initialized with {footstepClips.Length} footstep clips.");
            Debug.Log($"[FootstepController] Ground LayerMask: {groundLayerMask.value} (Binary: {System.Convert.ToString(groundLayerMask.value, 2)})");
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogError("[FootstepController] AudioManager instance not found!");
        }

        if (characterController == null && enableDebugLogs)
        {
            Debug.LogWarning("[FootstepController] No CharacterController found, using raycast for ground detection.");
        }
    }

    void Update()
    {
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        // Calculate movement
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float speedThreshold = minSpeed * Time.deltaTime;
        bool isMoving = distanceMoved > speedThreshold;

        // Check if grounded with improved detection
        bool isGrounded = IsGroundedImproved();

        // Debug logging (every 30 frames to reduce spam)
        if (enableDebugLogs && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[FootstepController] Status - Moving: {isMoving} ({distanceMoved:F4} > {speedThreshold:F4}), Grounded: {isGrounded}, Timer: {stepTimer:F2}");

            if (!isGrounded)
            {
                Debug.Log($"[FootstepController] Ground Detection Debug:");
                Debug.Log($"  - Position: {transform.position}");
                Debug.Log($"  - Raycast Distance: {raycastDistance}");
                Debug.Log($"  - LayerMask: {groundLayerMask.value}");
                Debug.Log($"  - CharacterController: {(characterController != null ? "Found" : "Missing")}");
                if (characterController != null)
                {
                    Debug.Log($"  - CC.isGrounded: {characterController.isGrounded}");
                }
            }
        }

        // Alert when grounded state changes
        if (isGrounded != wasGroundedLastFrame && enableDebugLogs)
        {
            Debug.Log($"[FootstepController] Ground state changed: {wasGroundedLastFrame} → {isGrounded}");
        }
        wasGroundedLastFrame = isGrounded;

        if (isMoving && isGrounded)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                if (enableDebugLogs)
                    Debug.Log($"[FootstepController] Step triggered! Timer: {stepTimer:F2}, Interval: {stepInterval}");

                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }

        lastPosition = transform.position;
    }

    bool IsGroundedImproved()
    {
        // Emergency override for testing
        if (forceGrounded) return true;

        // Try CharacterController first if available
        if (characterController != null && characterController.isGrounded)
        {
            return true;
        }

        // Multiple raycast approach for better detection
        if (useMultipleRaycasts)
        {
            return IsGroundedMultiRaycast();
        }
        else
        {
            return IsGroundedSingleRaycast();
        }
    }

    bool IsGroundedSingleRaycast()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * raycastOffset;
        bool hit = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayerMask);

        if (showDebugRaycast)
        {
            Color rayColor = hit ? Color.green : Color.red;
            Debug.DrawRay(rayOrigin, Vector3.down * raycastDistance, rayColor, 0.1f);
        }

        return hit;
    }

    bool IsGroundedMultiRaycast()
    {
        Vector3 center = transform.position + Vector3.up * raycastOffset;
        float radius = 0.3f; // Adjust based on your character size

        // Center raycast
        bool centerHit = Physics.Raycast(center, Vector3.down, raycastDistance, groundLayerMask);

        // Four corner raycasts
        Vector3[] offsets = {
            new Vector3(radius, 0, 0),
            new Vector3(-radius, 0, 0),
            new Vector3(0, 0, radius),
            new Vector3(0, 0, -radius)
        };

        int hitCount = centerHit ? 1 : 0;

        foreach (Vector3 offset in offsets)
        {
            Vector3 rayOrigin = center + offset;
            bool hit = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayerMask);

            if (showDebugRaycast)
            {
                Color rayColor = hit ? Color.green : Color.red;
                Debug.DrawRay(rayOrigin, Vector3.down * raycastDistance, rayColor, 0.1f);
            }

            if (hit) hitCount++;
        }

        // Consider grounded if at least 2 out of 5 rays hit
        return hitCount >= 2;
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0 || AudioManager.Instance == null) return;

        int clipIndex = Random.Range(0, footstepClips.Length);
        AudioClip clipToPlay = footstepClips[clipIndex];

        if (clipToPlay == null)
        {
            if (enableDebugLogs)
                Debug.LogError($"[FootstepController] Footstep clip at index {clipIndex} is null!");
            return;
        }

        float randomVolume = 1f + Random.Range(-volumeRange, volumeRange);
        float randomPitch = 1f + Random.Range(-pitchRange, pitchRange);

        Vector3 footstepPosition = transform.position;
        footstepPosition.y -= 0.5f;

        if (enableDebugLogs)
        {
            Debug.Log($"[FootstepController] Playing footstep: {clipToPlay.name}");
        }

        AudioManager.Instance.PlayFootstep(clipToPlay, footstepPosition, randomVolume, randomPitch);
    }

    // Debug methods
    [ContextMenu("Test Footstep")]
    public void TestFootstep()
    {
        Debug.Log("[FootstepController] Manual footstep test triggered!");
        PlayFootstep();
    }

    [ContextMenu("Debug Ground Detection")]
    public void DebugGroundDetection()
    {
        Debug.Log("=== GROUND DETECTION DEBUG ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"CharacterController: {(characterController != null ? "Present" : "Missing")}");
        if (characterController != null)
            Debug.Log($"CC.isGrounded: {characterController.isGrounded}");

        Debug.Log($"Single Raycast Result: {IsGroundedSingleRaycast()}");
        Debug.Log($"Multi Raycast Result: {IsGroundedMultiRaycast()}");
        Debug.Log($"LayerMask: {groundLayerMask.value}");
        Debug.Log($"Raycast Distance: {raycastDistance}");
    }
}