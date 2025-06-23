using UnityEngine;

public class BuildingEntranceZone : MonoBehaviour
{
    [Header("Ambience Zones")]
    public string exteriorZone = "City";
    public string interiorZone = "IndoorRoom";

    [Header("Audio Settings")]
    public bool enableReverbTransition = true;
    public AudioReverbPreset interiorReverb = AudioReverbPreset.Room;
    public AudioReverbPreset exteriorReverb = AudioReverbPreset.City;

    [Header("Volume Transitions")]
    [Range(0f, 1f)]
    public float interiorVolumeDamping = 0.3f; // Dampen exterior sounds when inside
    public string exteriorMixerParameter = "CityVolume"; // Mixer parameter name

    [Header("Trigger Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    private AmbientSFXManager ambientManager;
    private AudioReverbZone reverbZone;
    private bool playerInside = false;

    private void Start()
    {
        ambientManager = FindFirstObjectByType<AmbientSFXManager>();

        if (ambientManager == null)
        {
            Debug.LogError("AmbientSFXManager not found in scene!");
        }

        // Setup building collider
        SetupBuildingCollider();

        // Setup reverb zone for interior acoustics
        if (enableReverbTransition)
        {
            SetupReverbZone();
        }
    }

    private void SetupBuildingCollider()
    {
        Collider buildingCollider = GetComponent<Collider>();
        if (buildingCollider == null)
        {
            Debug.LogWarning($"Building entrance on {gameObject.name} needs a collider. Adding BoxCollider.");
            buildingCollider = gameObject.AddComponent<BoxCollider>();
        }
        buildingCollider.isTrigger = true;
    }

    private void SetupReverbZone()
    {
        reverbZone = GetComponent<AudioReverbZone>();
        if (reverbZone == null)
        {
            reverbZone = gameObject.AddComponent<AudioReverbZone>();
        }

        // Configure reverb zone to match building interior
        reverbZone.reverbPreset = interiorReverb;
        reverbZone.minDistance = 1f;
        reverbZone.maxDistance = 15f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag)) return;

        if (!playerInside)
        {
            playerInside = true;
            EnterBuilding();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag)) return;

        if (playerInside)
        {
            playerInside = false;
            ExitBuilding();
        }
    }

    private void EnterBuilding()
    {
        Debug.Log($"Player entered building - switching to {interiorZone}");

        // Switch to interior ambience
        ambientManager?.SetAmbientZone(interiorZone);

        // Dampen exterior city sounds
        if (!string.IsNullOrEmpty(exteriorMixerParameter))
        {
            StartCoroutine(TransitionMixerParameter(exteriorMixerParameter, interiorVolumeDamping, 1f));
        }

        // Set interior reverb
        if (enableReverbTransition && reverbZone != null)
        {
            reverbZone.reverbPreset = interiorReverb;
        }
    }

    private void ExitBuilding()
    {
        Debug.Log($"Player exited building - switching to {exteriorZone}");

        // Switch back to exterior ambience
        ambientManager?.SetAmbientZone(exteriorZone);

        // Restore exterior city sounds
        if (!string.IsNullOrEmpty(exteriorMixerParameter))
        {
            StartCoroutine(TransitionMixerParameter(exteriorMixerParameter, 1f, 1f));
        }

        // Set exterior reverb
        if (enableReverbTransition && reverbZone != null)
        {
            reverbZone.reverbPreset = exteriorReverb;
        }
    }

    private System.Collections.IEnumerator TransitionMixerParameter(string parameterName, float targetValue, float duration)
    {
        if (ambientManager.masterMixerGroup?.audioMixer == null) yield break;

        var mixer = ambientManager.masterMixerGroup.audioMixer;
        mixer.GetFloat(parameterName, out float currentValue);

        float startValue = Mathf.Pow(10, currentValue / 20); // Convert from dB to linear
        float targetLinear = targetValue;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            float currentLinear = Mathf.Lerp(startValue, targetLinear, normalizedTime);
            float dBValue = currentLinear > 0 ? Mathf.Log10(currentLinear) * 20 : -80f;

            mixer.SetFloat(parameterName, dBValue);
            yield return null;
        }

        float finalDB = targetLinear > 0 ? Mathf.Log10(targetLinear) * 20 : -80f;
        mixer.SetFloat(parameterName, finalDB);
    }

    private void OnDrawGizmos()
    {
        // Draw building entrance zone
        Gizmos.color = playerInside ? new Color(1f, 0.5f, 0f, 0.3f) : new Color(0f, 0.5f, 1f, 0.3f);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }

        // Draw reverb zone if enabled
        if (enableReverbTransition && reverbZone != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, reverbZone.maxDistance);
        }
    }
}