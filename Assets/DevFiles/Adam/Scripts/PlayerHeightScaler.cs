using UnityEngine;

public class PlayerHeightScaler : MonoBehaviour
{
    [Header("References")]
    public Transform playerModel;
    public Transform vrCamera;

    [Header("Target Height")]
    public float referenceHeight = 1.69f;

    void Start()
    {
        AdjustPlayerHeight();
    }

    void Update()
    {
        
    }

    void AdjustPlayerHeight()
    {
        if (playerModel == null || vrCamera == null)
        {
            Debug.LogWarning("PlayerHeightScaler: Missing references.");
            return;
        }

        float realHeight = vrCamera.position.y;

        if (realHeight <= 0.1f)
        {
            Debug.LogWarning("PlayerHeightScaler: Invalid player height.");
            return;
        }

        float scaleFactor = realHeight / referenceHeight;

        playerModel.localScale = Vector3.one * scaleFactor;
    }
}
