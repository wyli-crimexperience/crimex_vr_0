using UnityEngine;

public class AudioDebugger : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private AudioClip testClip;
    [SerializeField] private KeyCode testKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestAudioSystem();
        }
    }

    [ContextMenu("Test Audio System")]
    public void TestAudioSystem()
    {
        Debug.Log("=== AUDIO SYSTEM DEBUG TEST ===");

        // Check AudioManager
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is NULL!");
            return;
        }

        Debug.Log("AudioManager instance found.");

        // Check test clip
        if (testClip == null)
        {
            Debug.LogError("Test clip is NULL! Assign an audio clip in the inspector.");
            return;
        }

        Debug.Log($"Playing test clip: {testClip.name}");

        // Test play sound
        AudioManager.Instance.PlaySound(testClip, transform.position);
    }
}