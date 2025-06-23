using UnityEngine;

public class AmbientZoneTrigger : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName = "Default";
    public bool useOnEnter = true;
    public bool useOnExit = false;
    public string exitZoneName = "Default";

    [Header("Trigger Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    private AmbientSFXManager ambientManager;

    private void Start()
    {
        ambientManager = FindFirstObjectByType<AmbientSFXManager>();

        if (ambientManager == null)
        {
            Debug.LogError("AmbientSFXManager not found in scene!");
        }

        // Ensure this GameObject has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"AmbientZoneTrigger on {gameObject.name} has no collider. Adding BoxCollider.");
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useOnEnter) return;

        if (requirePlayerTag && !other.CompareTag(playerTag)) return;

        if (ambientManager != null)
        {
            Debug.Log($"Entering ambient zone: {zoneName}");
            ambientManager.SetAmbientZone(zoneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useOnExit) return;

        if (requirePlayerTag && !other.CompareTag(playerTag)) return;

        if (ambientManager != null)
        {
            Debug.Log($"Exiting ambient zone: {zoneName}, switching to: {exitZoneName}");
            ambientManager.SetAmbientZone(exitZoneName);
        }
    }

    // For debugging
    private void OnDrawGizmos()
    {
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
    }
}