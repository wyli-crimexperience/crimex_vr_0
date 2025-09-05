using UnityEngine;
using System.Collections;

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
    public float interiorVolumeDamping = 0.3f;
    public string exteriorMixerParameter = "CityVolume";

    [Header("Trigger Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    private AudioReverbZone reverbZone;
    private bool playerInside = false;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[BuildingEntranceZone] AudioManager not found at Start. Will check when triggered.");
        }

        SetupBuildingCollider();

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
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[BuildingEntranceZone] AudioManager instance not found!");
            return;
        }

        Debug.Log($"Player entered building - switching to {interiorZone}");

        // Switch to interior ambience
        AudioManager.Instance.SetAmbientZone(interiorZone);

        // Dampen exterior city sounds
        if (!string.IsNullOrEmpty(exteriorMixerParameter))
        {
            StartCoroutine(AudioManager.Instance.TransitionMixerParameter(exteriorMixerParameter, interiorVolumeDamping, 1f));
        }

        // Set interior reverb
        if (enableReverbTransition && reverbZone != null)
        {
            reverbZone.reverbPreset = interiorReverb;
        }
    }

    private void ExitBuilding()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[BuildingEntranceZone] AudioManager instance not found!");
            return;
        }

        Debug.Log($"Player exited building - switching to {exteriorZone}");

        // Switch back to exterior ambience
        AudioManager.Instance.SetAmbientZone(exteriorZone);

        // Restore exterior city sounds
        if (!string.IsNullOrEmpty(exteriorMixerParameter))
        {
            StartCoroutine(AudioManager.Instance.TransitionMixerParameter(exteriorMixerParameter, 1f, 1f));
        }

        // Set exterior reverb
        if (enableReverbTransition && reverbZone != null)
        {
            reverbZone.reverbPreset = exteriorReverb;
        }
    }

    private void OnDrawGizmos()
    {
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

        if (enableReverbTransition && reverbZone != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, reverbZone.maxDistance);
        }
    }
}