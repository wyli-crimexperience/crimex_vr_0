using UnityEngine;
using TMPro;

public class EvidenceMarkerCopy : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private TextMeshProUGUI txtLabel;

    [Header("Debug Info")]
    [SerializeField] private TypeEvidenceMarker typeEvidenceMarker;
    [SerializeField] private int index;
    [SerializeField] private int markerNumber;
    [SerializeField] private string caseId;

    // Public properties
    public TypeEvidenceMarker TypeEvidenceMarker => typeEvidenceMarker;
    public int Index => index;
    public int MarkerNumber => markerNumber;
    public string CaseId => caseId;

    /// <summary>
    /// Legacy initialization method - kept for backward compatibility
    /// </summary>
    /// <param name="_typeEvidenceMarker">Type of evidence marker</param>
    /// <param name="_index">Array index in the evidence list</param>
    /// <param name="evidenceMarker">Transform to copy position and rotation from</param>
    public void Init(TypeEvidenceMarker _typeEvidenceMarker, int _index, Transform evidenceMarker)
    {
        typeEvidenceMarker = _typeEvidenceMarker;
        index = _index;
        markerNumber = _index + 1; // Default marker number is index + 1

        transform.SetPositionAndRotation(evidenceMarker.position, evidenceMarker.rotation);

        UpdateLabel();
    }

    /// <summary>
    /// New initialization method for the refactored EvidenceManager
    /// </summary>
    /// <param name="type">Type of evidence marker</param>
    /// <param name="arrayIndex">Index in the evidence array</param>
    /// <param name="displayNumber">Number to display on the marker</param>
    /// <param name="evidenceCaseId">Optional case ID</param>
    public void Initialize(TypeEvidenceMarker type, int arrayIndex, int displayNumber, string evidenceCaseId = "")
    {
        typeEvidenceMarker = type;
        index = arrayIndex;
        markerNumber = displayNumber;
        caseId = evidenceCaseId;

        UpdateLabel();
    }

    /// <summary>
    /// Initialize with position and rotation
    /// </summary>
    /// <param name="type">Type of evidence marker</param>
    /// <param name="arrayIndex">Index in the evidence array</param>
    /// <param name="displayNumber">Number to display on the marker</param>
    /// <param name="position">World position</param>
    /// <param name="rotation">World rotation</param>
    /// <param name="evidenceCaseId">Optional case ID</param>
    public void Initialize(TypeEvidenceMarker type, int arrayIndex, int displayNumber,
        Vector3 position, Quaternion rotation, string evidenceCaseId = "")
    {
        Initialize(type, arrayIndex, displayNumber, evidenceCaseId);
        transform.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// Initialize from existing transform (backward compatibility)
    /// </summary>
    /// <param name="type">Type of evidence marker</param>
    /// <param name="arrayIndex">Index in the evidence array</param>
    /// <param name="displayNumber">Number to display on the marker</param>
    /// <param name="evidenceMarker">Transform to copy position and rotation from</param>
    /// <param name="evidenceCaseId">Optional case ID</param>
    public void Initialize(TypeEvidenceMarker type, int arrayIndex, int displayNumber,
        Transform evidenceMarker, string evidenceCaseId = "")
    {
        Initialize(type, arrayIndex, displayNumber, evidenceCaseId);
        transform.SetPositionAndRotation(evidenceMarker.position, evidenceMarker.rotation);
    }

    /// <summary>
    /// Updates the text label based on marker type and number
    /// </summary>
    private void UpdateLabel()
    {
        if (txtLabel == null) return;

        // Use your existing logic: numbers for items, letters for bodies
        if (typeEvidenceMarker == TypeEvidenceMarker.Item)
        {
            txtLabel.text = markerNumber.ToString();
        }
        else
        {
            txtLabel.text = StaticUtils.ConvertToLetter(index);
        }

        // Optional: Add case ID if available
        if (!string.IsNullOrEmpty(caseId))
        {
            txtLabel.text += $"\n({caseId})";
        }
    }

    /// <summary>
    /// Removes this evidence marker from the scene
    /// </summary>
    public void Remove()
    {
        // Try new EvidenceManager first, fallback to ManagerGlobal
        if (EvidenceManager.Instance != null)
        {
            EvidenceManager.Instance.RemoveEvidenceMarker(this);
        }
        else if (ManagerGlobal.Instance != null)
        {
            ManagerGlobal.Instance.RemoveEvidenceMarker(this);
        }
        else
        {
            Debug.LogWarning("[EvidenceMarkerCopy] No manager available to remove evidence marker");
            Destroy(gameObject); // Fallback: just destroy the object
        }
    }

    /// <summary>
    /// Updates the marker's case ID and refreshes the display
    /// </summary>
    /// <param name="newCaseId">New case ID to assign</param>
    public void SetCaseId(string newCaseId)
    {
        caseId = newCaseId;
        UpdateLabel();
    }

    /// <summary>
    /// Updates the marker number and refreshes the display
    /// </summary>
    /// <param name="newMarkerNumber">New marker number</param>
    public void SetMarkerNumber(int newMarkerNumber)
    {
        markerNumber = newMarkerNumber;
        UpdateLabel();
    }

    /// <summary>
    /// Gets formatted display text for this marker
    /// </summary>
    /// <returns>Formatted string for display</returns>
    public string GetDisplayText()
    {
        string baseText = typeEvidenceMarker == TypeEvidenceMarker.Item
            ? markerNumber.ToString()
            : StaticUtils.ConvertToLetter(index);

        if (!string.IsNullOrEmpty(caseId))
        {
            return $"{baseText} ({caseId})";
        }

        return baseText;
    }

    /// <summary>
    /// Validates that the marker has all required components
    /// </summary>
    /// <returns>True if marker is properly configured</returns>
    public bool IsValid()
    {
        return txtLabel != null;
    }

    // Unity Editor validation
    private void OnValidate()
    {
        UpdateLabel();
    }

    // Optional: Add interaction feedback
    private void OnMouseEnter()
    {
        // Could add hover effects here
    }

    private void OnMouseExit()
    {
        // Could remove hover effects here
    }

    // Debug information
    public override string ToString()
    {
        return $"EvidenceMarker({typeEvidenceMarker}, Index:{index}, Number:{markerNumber}, CaseID:{caseId})";
    }
}