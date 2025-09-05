using UnityEngine;

public class AmbientZoneTrigger : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName = "Default";
    public bool useOnEnter = true;
    public bool useOnExit = false;
    public string exitZoneName = "Default";

    [Header("Transition Type")]
    public TransitionType transitionType = TransitionType.Normal;
    public float crossfadeTime = 0.2f;

    [Header("Reverb Settings")]
    public bool enableReverbControl = false;
    public AudioReverbPreset enterReverb = AudioReverbPreset.Room;
    public AudioReverbPreset exitReverb = AudioReverbPreset.Off;

    [Header("Advanced Reverb (Optional)")]
    public bool useCustomReverbSettings = false;
    [SerializeField] private ReverbSettings customEnterSettings;
    [SerializeField] private ReverbSettings customExitSettings;

    [Header("Trigger Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    [Header("Debug")]
    public bool showReverbDebug = false;

    public enum TransitionType
    {
        Normal,      // Standard fade in/out
        Fast,        // Quick crossfade
        Instant      // Immediate change
    }

    [System.Serializable]
    public class ReverbSettings
    {
        [Range(-10000, 0)] public int room = -1000;
        [Range(-10000, 0)] public int roomHF = -100;
        [Range(0.1f, 20f)] public float decayTime = 1.49f;
        [Range(0.1f, 2f)] public float decayHFRatio = 0.83f;
        [Range(-10000, 1000)] public int reflections = -2602;
        [Range(0f, 0.3f)] public float reflectionsDelay = 0.007f;
        [Range(-10000, 2000)] public int reverb = 200;
        [Range(0f, 0.1f)] public float reverbDelay = 0.011f;
        [Range(0f, 100f)] public float diffusion = 100f;
        [Range(0f, 100f)] public float density = 100f;
    }

    private AudioReverbZone reverbZone;
    private bool hasCustomReverbZone = false;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[AmbientZoneTrigger] AudioManager not found at Start. Will check when triggered.");
        }

        SetupCollider();

        if (enableReverbControl)
        {
            SetupReverbZone();
        }
    }

    private void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"AmbientZoneTrigger on {gameObject.name} has no collider. Adding BoxCollider.");
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;
    }

    private void SetupReverbZone()
    {
        reverbZone = GetComponent<AudioReverbZone>();
        if (reverbZone == null)
        {
            reverbZone = gameObject.AddComponent<AudioReverbZone>();
            hasCustomReverbZone = true;

            if (showReverbDebug)
                Debug.Log($"[AmbientZoneTrigger] Created AudioReverbZone on {gameObject.name}");
        }

        // Set initial reverb settings
        if (useCustomReverbSettings && customEnterSettings != null)
        {
            ApplyCustomReverbSettings(customEnterSettings);
        }
        else
        {
            reverbZone.reverbPreset = enterReverb;
        }

        // Configure reverb zone bounds
        ConfigureReverbZoneBounds();
    }

    private void ConfigureReverbZoneBounds()
    {
        if (reverbZone == null) return;

        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            reverbZone.minDistance = Mathf.Min(box.size.x, box.size.y, box.size.z) * 0.1f;
            reverbZone.maxDistance = Mathf.Max(box.size.x, box.size.y, box.size.z) * 0.8f;
        }
        else if (col is SphereCollider sphere)
        {
            reverbZone.minDistance = sphere.radius * 0.1f;
            reverbZone.maxDistance = sphere.radius * 0.8f;
        }
        else
        {
            // Default values
            reverbZone.minDistance = 1f;
            reverbZone.maxDistance = 10f;
        }

        if (showReverbDebug)
        {
            Debug.Log($"[AmbientZoneTrigger] Reverb zone configured - Min: {reverbZone.minDistance}, Max: {reverbZone.maxDistance}");
        }
    }

    private void ApplyCustomReverbSettings(ReverbSettings settings)
    {
        if (reverbZone == null) return;

        reverbZone.reverbPreset = AudioReverbPreset.User;

        // Only set properties that actually exist on AudioReverbZone
        reverbZone.room = settings.room;
        reverbZone.roomHF = settings.roomHF;
        reverbZone.decayTime = settings.decayTime;
        reverbZone.decayHFRatio = settings.decayHFRatio;
        reverbZone.reflections = settings.reflections;
        reverbZone.reflectionsDelay = settings.reflectionsDelay;
        reverbZone.reverb = settings.reverb;
        reverbZone.reverbDelay = settings.reverbDelay;
        reverbZone.diffusion = settings.diffusion;
        reverbZone.density = settings.density;

        // Note: hfReference and lfReference don't exist on AudioReverbZone in Unity
        // They're part of the Audio Mixer's reverb effect, not AudioReverbZone
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useOnEnter) return;
        if (requirePlayerTag && !other.CompareTag(playerTag)) return;
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[AmbientZoneTrigger] AudioManager instance not found!");
            return;
        }

        Debug.Log($"Entering ambient zone: {zoneName}");

        // Change ambient zone
        switch (transitionType)
        {
            case TransitionType.Normal:
                AudioManager.Instance.SetAmbientZone(zoneName);
                break;
            case TransitionType.Fast:
                AudioManager.Instance.SetAmbientZoneFast(zoneName, crossfadeTime);
                break;
            case TransitionType.Instant:
                AudioManager.Instance.SetAmbientZoneInstant(zoneName);
                break;
        }

        // Apply reverb settings
        if (enableReverbControl && reverbZone != null)
        {
            if (useCustomReverbSettings && customEnterSettings != null)
            {
                ApplyCustomReverbSettings(customEnterSettings);
                if (showReverbDebug)
                    Debug.Log($"[AmbientZoneTrigger] Applied custom enter reverb settings");
            }
            else
            {
                reverbZone.reverbPreset = enterReverb;
                if (showReverbDebug)
                    Debug.Log($"[AmbientZoneTrigger] Applied enter reverb preset: {enterReverb}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useOnExit) return;
        if (requirePlayerTag && !other.CompareTag(playerTag)) return;
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[AmbientZoneTrigger] AudioManager instance not found!");
            return;
        }

        Debug.Log($"Exiting ambient zone: {zoneName}, switching to: {exitZoneName}");

        // Change ambient zone
        switch (transitionType)
        {
            case TransitionType.Normal:
                AudioManager.Instance.SetAmbientZone(exitZoneName);
                break;
            case TransitionType.Fast:
                AudioManager.Instance.SetAmbientZoneFast(exitZoneName, crossfadeTime);
                break;
            case TransitionType.Instant:
                AudioManager.Instance.SetAmbientZoneInstant(exitZoneName);
                break;
        }

        // Apply exit reverb settings
        if (enableReverbControl && reverbZone != null)
        {
            if (useCustomReverbSettings && customExitSettings != null)
            {
                ApplyCustomReverbSettings(customExitSettings);
                if (showReverbDebug)
                    Debug.Log($"[AmbientZoneTrigger] Applied custom exit reverb settings");
            }
            else
            {
                reverbZone.reverbPreset = exitReverb;
                if (showReverbDebug)
                    Debug.Log($"[AmbientZoneTrigger] Applied exit reverb preset: {exitReverb}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw trigger zone
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;

        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }

        // Draw reverb zone bounds if enabled
        if (enableReverbControl && reverbZone != null)
        {
            Gizmos.matrix = Matrix4x4.identity;

            // Inner reverb boundary (min distance)
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, reverbZone.minDistance);

            // Outer reverb boundary (max distance)
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, reverbZone.maxDistance);
        }

        // Draw zone label
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(zoneName))
        {
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            UnityEditor.Handles.Label(labelPos, $"Zone: {zoneName}");

            if (enableReverbControl)
            {
                string reverbLabel = useCustomReverbSettings ? "Custom Reverb" : enterReverb.ToString();
                UnityEditor.Handles.Label(labelPos + Vector3.up * 0.5f, $"Reverb: {reverbLabel}");
            }
        }
#endif
    }

    // Context menu methods for testing
    [ContextMenu("Test Enter Zone")]
    public void TestEnterZone()
    {
        Debug.Log("[AmbientZoneTrigger] Manual enter zone test");
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInChildren<Collider>();
            OnTriggerEnter(playerCollider);
        }
    }

    [ContextMenu("Test Exit Zone")]
    public void TestExitZone()
    {
        Debug.Log("[AmbientZoneTrigger] Manual exit zone test");
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInChildren<Collider>();
            OnTriggerExit(playerCollider);
        }
    }

    // Public methods for runtime control
    public void SetReverbPreset(AudioReverbPreset preset)
    {
        if (reverbZone != null)
        {
            reverbZone.reverbPreset = preset;
            if (showReverbDebug)
                Debug.Log($"[AmbientZoneTrigger] Reverb preset set to: {preset}");
        }
    }

    public void EnableReverb(bool enable)
    {
        enableReverbControl = enable;
        if (reverbZone != null)
        {
            reverbZone.enabled = enable;
        }
    }
}