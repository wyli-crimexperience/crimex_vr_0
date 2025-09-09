using System;
using System.Collections.Generic;
using UnityEngine;

//EVIDENCE MANAGER
// Responsible for managing the placement, removal, and tracking of evidence markers (items and bodies) in the scene.
// It handles marker instantiation, ensures placement rules (such as minimum distance and maximum count), maintains data for persistence,
// and provides events for other systems to react to evidence changes. It also supports clearing all evidence and finding the closest marker.

[System.Serializable]
public class EvidenceMarkerData
{
    public int index;
    public TypeEvidenceMarker type;
    public Vector3 position;
    public Quaternion rotation;
    public string caseId;
    public DateTime placedTime;
    public TypeRole placedByRole;
}

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance { get; private set; }

    [Header("Prefab References")]
    [SerializeField] private GameObject evidenceMarkerItemPrefab;
    [SerializeField] private GameObject evidenceMarkerBodyPrefab;

    [Header("Evidence Configuration")]
    [SerializeField] private int maxEvidenceMarkers = 50;
    [SerializeField] private float minDistanceBetweenMarkers = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // Events
    public event System.Action<EvidenceMarkerCopy> OnEvidenceMarkerPlaced;
    public event System.Action<EvidenceMarkerCopy> OnEvidenceMarkerRemoved;
    public event System.Action<int> OnEvidenceCountChanged;

    // Evidence tracking
    private List<EvidenceMarkerCopy> evidenceMarkerItemCopies = new List<EvidenceMarkerCopy>();
    private List<EvidenceMarkerCopy> evidenceMarkerBodyCopies = new List<EvidenceMarkerCopy>();
    private int nextEvidenceMarkerIndex = 1;

    // Evidence data for persistence
    private List<EvidenceMarkerData> evidenceMarkerDataList = new List<EvidenceMarkerData>();

    public int TotalEvidenceCount => evidenceMarkerItemCopies.Count + evidenceMarkerBodyCopies.Count;
    public int ItemEvidenceCount => evidenceMarkerItemCopies.Count;
    public int BodyEvidenceCount => evidenceMarkerBodyCopies.Count;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            InitializeEvidenceManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEvidenceManager()
    {
        // Initialize lists with null values up to max capacity
        for (int i = 0; i < maxEvidenceMarkers; i++)
        {
            evidenceMarkerItemCopies.Add(null);
            evidenceMarkerBodyCopies.Add(null);
        }

        if (enableDebugLogs)
            Debug.Log($"[EvidenceManager] Initialized with max capacity: {maxEvidenceMarkers}");
    }

    /// <summary>
    /// Places an evidence marker at the specified position
    /// </summary>
    /// <param name="markerType">Type of evidence marker (Item or Body)</param>
    /// <param name="position">World position to place the marker</param>
    /// <param name="rotation">World rotation for the marker</param>
    /// <param name="placedByRole">Role of the player placing the marker</param>
    /// <param name="caseId">Optional case ID for the evidence</param>
    /// <returns>The created EvidenceMarkerCopy or null if failed</returns>
    public EvidenceMarkerCopy PlaceEvidenceMarker(TypeEvidenceMarker markerType, Vector3 position, 
        Quaternion rotation, TypeRole placedByRole, string caseId = "")
    {
        // Validate placement
        if (!CanPlaceEvidenceMarker(markerType, position))
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[EvidenceManager] Cannot place evidence marker at position {position}");
            return null;
        }

        // Get the appropriate prefab
        GameObject prefab = GetEvidenceMarkerPrefab(markerType);
        if (prefab == null)
        {
            Debug.LogError($"[EvidenceManager] No prefab found for marker type: {markerType}");
            return null;
        }

        // Find available index
        int availableIndex = GetNextAvailableIndex(markerType);
        if (availableIndex == -1)
        {
            Debug.LogWarning($"[EvidenceManager] No available slots for {markerType} evidence markers");
            return null;
        }

        // Instantiate the evidence marker
        GameObject markerObject = Instantiate(prefab, position, rotation);
        EvidenceMarkerCopy markerCopy = markerObject.GetComponent<EvidenceMarkerCopy>();
        
        if (markerCopy == null)
        {
            Debug.LogError($"[EvidenceManager] Evidence marker prefab missing EvidenceMarkerCopy component");
            Destroy(markerObject);
            return null;
        }

        // Initialize the marker
        markerCopy.Initialize(markerType, availableIndex, nextEvidenceMarkerIndex, caseId);
        nextEvidenceMarkerIndex++;

        // Add to appropriate list
        if (markerType == TypeEvidenceMarker.Item)
        {
            evidenceMarkerItemCopies[availableIndex] = markerCopy;
        }
        else
        {
            evidenceMarkerBodyCopies[availableIndex] = markerCopy;
        }

        // Store evidence data
        EvidenceMarkerData evidenceData = new EvidenceMarkerData
        {
            index = availableIndex,
            type = markerType,
            position = position,
            rotation = rotation,
            caseId = caseId,
            placedTime = DateTime.Now,
            placedByRole = placedByRole
        };
        evidenceMarkerDataList.Add(evidenceData);

        // Update timeline if available
        if (ManagerGlobal.Instance?.TimelineManager != null)
        {
            ManagerGlobal.Instance.TimelineManager.SetEventNow(
                TimelineEvent.EvidenceMarked,
                ManagerGlobal.Instance.TimelineManager.GetEventTime(TimelineEvent.Incident).Value
            );
        }

        // Trigger events
        OnEvidenceMarkerPlaced?.Invoke(markerCopy);
        OnEvidenceCountChanged?.Invoke(TotalEvidenceCount);

        if (enableDebugLogs)
            Debug.Log($"[EvidenceManager] Placed {markerType} evidence marker #{markerCopy.MarkerNumber} at index {availableIndex}");

        return markerCopy;
    }

    /// <summary>
    /// Removes an evidence marker from the scene
    /// </summary>
    /// <param name="evidenceMarkerCopy">The evidence marker to remove</param>
    public bool RemoveEvidenceMarker(EvidenceMarkerCopy evidenceMarkerCopy)
    {
        if (evidenceMarkerCopy == null)
        {
            Debug.LogWarning("[EvidenceManager] Attempted to remove null evidence marker");
            return false;
        }

        int index = evidenceMarkerCopy.Index;
        TypeEvidenceMarker markerType = evidenceMarkerCopy.TypeEvidenceMarker;

        // Remove from appropriate list
        bool removed = false;
        if (markerType == TypeEvidenceMarker.Item && 
            index < evidenceMarkerItemCopies.Count && 
            evidenceMarkerItemCopies[index] == evidenceMarkerCopy)
        {
            evidenceMarkerItemCopies[index] = null;
            removed = true;
        }
        else if (markerType == TypeEvidenceMarker.Body && 
                 index < evidenceMarkerBodyCopies.Count && 
                 evidenceMarkerBodyCopies[index] == evidenceMarkerCopy)
        {
            evidenceMarkerBodyCopies[index] = null;
            removed = true;
        }

        if (removed)
        {
            // Remove from data list
            evidenceMarkerDataList.RemoveAll(data => 
                data.index == index && data.type == markerType);

            // Destroy the game object
            Destroy(evidenceMarkerCopy.gameObject);

            // Trigger events
            OnEvidenceMarkerRemoved?.Invoke(evidenceMarkerCopy);
            OnEvidenceCountChanged?.Invoke(TotalEvidenceCount);

            if (enableDebugLogs)
                Debug.Log($"[EvidenceManager] Removed {markerType} evidence marker at index {index}");

            return true;
        }

        Debug.LogWarning($"[EvidenceManager] Could not remove evidence marker - not found in lists");
        return false;
    }

    /// <summary>
    /// Gets all evidence markers of a specific type
    /// </summary>
    /// <param name="markerType">Type of evidence markers to retrieve</param>
    /// <returns>List of active evidence markers of the specified type</returns>
    public List<EvidenceMarkerCopy> GetEvidenceMarkers(TypeEvidenceMarker markerType)
    {
        List<EvidenceMarkerCopy> result = new List<EvidenceMarkerCopy>();
        List<EvidenceMarkerCopy> sourceList = markerType == TypeEvidenceMarker.Item 
            ? evidenceMarkerItemCopies 
            : evidenceMarkerBodyCopies;

        foreach (var marker in sourceList)
        {
            if (marker != null)
            {
                result.Add(marker);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all evidence markers regardless of type
    /// </summary>
    /// <returns>List of all active evidence markers</returns>
    public List<EvidenceMarkerCopy> GetAllEvidenceMarkers()
    {
        List<EvidenceMarkerCopy> result = new List<EvidenceMarkerCopy>();
        result.AddRange(GetEvidenceMarkers(TypeEvidenceMarker.Item));
        result.AddRange(GetEvidenceMarkers(TypeEvidenceMarker.Body));
        return result;
    }

    /// <summary>
    /// Gets evidence marker data for persistence/saving
    /// </summary>
    /// <returns>List of all evidence marker data</returns>
    public List<EvidenceMarkerData> GetEvidenceData()
    {
        return new List<EvidenceMarkerData>(evidenceMarkerDataList);
    }

    /// <summary>
    /// Clears all evidence markers from the scene
    /// </summary>
    public void ClearAllEvidence()
    {
        // Remove all item markers
        for (int i = 0; i < evidenceMarkerItemCopies.Count; i++)
        {
            if (evidenceMarkerItemCopies[i] != null)
            {
                Destroy(evidenceMarkerItemCopies[i].gameObject);
                evidenceMarkerItemCopies[i] = null;
            }
        }

        // Remove all body markers
        for (int i = 0; i < evidenceMarkerBodyCopies.Count; i++)
        {
            if (evidenceMarkerBodyCopies[i] != null)
            {
                Destroy(evidenceMarkerBodyCopies[i].gameObject);
                evidenceMarkerBodyCopies[i] = null;
            }
        }

        // Clear data
        evidenceMarkerDataList.Clear();
        nextEvidenceMarkerIndex = 1;

        OnEvidenceCountChanged?.Invoke(0);

        if (enableDebugLogs)
            Debug.Log("[EvidenceManager] Cleared all evidence markers");
    }

    /// <summary>
    /// Finds the closest evidence marker to a given position
    /// </summary>
    /// <param name="position">Position to search from</param>
    /// <param name="maxDistance">Maximum search distance</param>
    /// <returns>Closest evidence marker or null if none found within range</returns>
    public EvidenceMarkerCopy FindClosestEvidenceMarker(Vector3 position, float maxDistance = float.MaxValue)
    {
        EvidenceMarkerCopy closest = null;
        float closestDistance = maxDistance;

        foreach (var marker in GetAllEvidenceMarkers())
        {
            float distance = Vector3.Distance(position, marker.transform.position);
            if (distance < closestDistance)
            {
                closest = marker;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private bool CanPlaceEvidenceMarker(TypeEvidenceMarker markerType, Vector3 position)
    {
        // Check if we have available slots
        if (GetNextAvailableIndex(markerType) == -1)
            return false;

        // Check minimum distance to other markers
        foreach (var marker in GetAllEvidenceMarkers())
        {
            if (Vector3.Distance(position, marker.transform.position) < minDistanceBetweenMarkers)
            {
                return false;
            }
        }

        return true;
    }

    private int GetNextAvailableIndex(TypeEvidenceMarker markerType)
    {
        List<EvidenceMarkerCopy> list = markerType == TypeEvidenceMarker.Item 
            ? evidenceMarkerItemCopies 
            : evidenceMarkerBodyCopies;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                return i;
        }

        return -1; // No available slots
    }

    private GameObject GetEvidenceMarkerPrefab(TypeEvidenceMarker markerType)
    {
        return markerType switch
        {
            TypeEvidenceMarker.Item => evidenceMarkerItemPrefab,
            TypeEvidenceMarker.Body => evidenceMarkerBodyPrefab,
            _ => null
        };
    }

    // Debug methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawGizmos()
    {
        if (!enableDebugLogs) return;

        // Draw evidence markers in scene view
        Gizmos.color = Color.red;
        foreach (var marker in GetEvidenceMarkers(TypeEvidenceMarker.Item))
        {
            Gizmos.DrawWireSphere(marker.transform.position, 0.1f);
        }

        Gizmos.color = Color.blue;
        foreach (var marker in GetEvidenceMarkers(TypeEvidenceMarker.Body))
        {
            Gizmos.DrawWireSphere(marker.transform.position, 0.1f);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}