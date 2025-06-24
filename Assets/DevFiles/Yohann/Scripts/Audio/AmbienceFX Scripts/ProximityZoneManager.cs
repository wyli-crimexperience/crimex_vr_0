using UnityEngine;
using System.Collections.Generic;

public class ProximityZoneManager : MonoBehaviour
{
    [System.Serializable]
    public class ProximityZone
    {
        public string zoneName;
        public Transform zoneCenter;
        public float radius = 10f;
        public int priority = 0; // Higher priority zones override lower ones
    }

    [Header("Proximity Zones")]
    public ProximityZone[] proximityZones;

    [Header("Settings")]
    public float checkInterval = 0.5f;
    public string defaultZone = "Default";

    private AmbientSFXManager ambientManager;
    private Transform playerTransform;
    private string currentProximityZone;

    private void Start()
    {
        ambientManager = FindFirstObjectByType<AmbientSFXManager>();

        // Get player transform from VR camera
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            GameObject mainCamera = GameObject.FindWithTag("MainCamera");
            if (mainCamera != null)
                playerTransform = mainCamera.transform;
        }
        else
        {
            playerTransform = playerObject.transform;
        }

        InvokeRepeating(nameof(CheckProximityZones), 0f, checkInterval);
    }

    private void CheckProximityZones()
    {
        if (playerTransform == null || ambientManager == null) return;

        string nearestZone = defaultZone;
        int highestPriority = -1;

        foreach (var zone in proximityZones)
        {
            if (zone.zoneCenter == null) continue;

            float distance = Vector3.Distance(playerTransform.position, zone.zoneCenter.position);

            if (distance <= zone.radius && zone.priority > highestPriority)
            {
                nearestZone = zone.zoneName;
                highestPriority = zone.priority;
            }
        }

        if (nearestZone != currentProximityZone)
        {
            currentProximityZone = nearestZone;
            ambientManager.SetAmbientZone(nearestZone);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var zone in proximityZones)
        {
            if (zone.zoneCenter == null) continue;

            Gizmos.color = new Color(1f, 0f, 1f, 0.2f);
            Gizmos.DrawSphere(zone.zoneCenter.position, zone.radius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(zone.zoneCenter.position, zone.radius);
        }
    }
}