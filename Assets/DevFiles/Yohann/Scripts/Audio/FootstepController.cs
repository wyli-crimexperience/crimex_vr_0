using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float baseVolume = 2.0f;
    [SerializeField] private float volumeRange = 0.2f;
    [SerializeField] private float pitchRange = 0.1f;

    [Header("Ground Detection Settings")]
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private float raycastOffset = 0.1f;
    [SerializeField] private bool useMultipleRaycasts = true;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showDebugRaycast = true;
    [SerializeField] private bool forceGrounded = false;

    private CharacterController characterController;
    private float stepTimer;
    private Vector3 lastPosition;
    private bool wasGroundedLastFrame;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        lastPosition = transform.position;

        if (footstepClips.Length == 0)
        {
            Debug.LogError($"[FootstepController] No footstep clips assigned to {gameObject.name}!");
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"[FootstepController] Initialized with {footstepClips.Length} footstep clips.");
            Debug.Log($"[FootstepController] Ground LayerMask: {groundLayerMask.value}");
        }

        // Wait for AudioManager to initialize
        if (AudioManager.Instance == null && enableDebugLogs)
        {
            Debug.LogWarning("[FootstepController] AudioManager instance not found! Will retry each frame.");
        }

        if (characterController == null && enableDebugLogs)
        {
            Debug.LogWarning("[FootstepController] No CharacterController found, using raycast for ground detection.");
        }
    }

    void Update()
    {
        // Check if AudioManager is available
        if (AudioManager.Instance == null) return;

        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float speedThreshold = minSpeed * Time.deltaTime;
        bool isMoving = distanceMoved > speedThreshold;
        bool isGrounded = IsGroundedImproved();

        if (enableDebugLogs && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[FootstepController] Status - Moving: {isMoving} ({distanceMoved:F4} > {speedThreshold:F4}), Grounded: {isGrounded}, Timer: {stepTimer:F2}");

            if (!isGrounded)
            {
                Debug.Log($"[FootstepController] Ground Detection Debug:");
                Debug.Log($"  - Position: {transform.position}");
                Debug.Log($"  - Raycast Distance: {raycastDistance}");
                Debug.Log($"  - LayerMask: {groundLayerMask.value}");
                if (characterController != null)
                    Debug.Log($"  - CC.isGrounded: {characterController.isGrounded}");
            }
        }

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
        if (forceGrounded) return true;

        if (characterController != null && characterController.isGrounded)
            return true;

        return useMultipleRaycasts ? IsGroundedMultiRaycast() : IsGroundedSingleRaycast();
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
        float radius = 0.3f;

        bool centerHit = Physics.Raycast(center, Vector3.down, raycastDistance, groundLayerMask);
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

        float randomVolume = baseVolume + Random.Range(-volumeRange, volumeRange);
        float randomPitch = 1f + Random.Range(-pitchRange, pitchRange);

        Vector3 footstepPosition = transform.position;
        footstepPosition.y -= 0.5f;

        if (enableDebugLogs)
        {
            Debug.Log($"[FootstepController] Playing footstep: {clipToPlay.name}, Final Volume: {randomVolume:F2}");
        }

        AudioManager.Instance.PlayFootstep(clipToPlay, footstepPosition, randomVolume, randomPitch);
    }

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