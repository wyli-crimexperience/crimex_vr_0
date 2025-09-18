// GeminiDialogueUIBridge.cs (Final Version)
using UnityEngine;

[RequireComponent(typeof(GeminiNPC))]
[RequireComponent(typeof(SpeechManager))] // Now requires the manager
public class GeminiDialogueUIBridge : MonoBehaviour
{
    private GeminiNPC geminiNpc;
    private SpeechManager speechManager; // Changed from ElevenLabsSynthesizer

    private void Awake()
    {
        geminiNpc = GetComponent<GeminiNPC>();
        speechManager = GetComponent<SpeechManager>(); // Get the manager
    }

    private void OnEnable()
    {
        geminiNpc.OnNPCResponseReceived += HandleStreamingNPCSpeech;
        geminiNpc.OnNPCTurnEnded += HandleFinalNPCSpeech;
        geminiNpc.OnConversationEnded += HandleConversationEnded;
        speechManager.OnSpeechFinished += HandleSpeechFinished; // Subscribe to the manager's event
    }

    private void OnDisable()
    {
        if (geminiNpc != null)
        {
            geminiNpc.OnNPCResponseReceived -= HandleStreamingNPCSpeech;
            geminiNpc.OnNPCTurnEnded -= HandleFinalNPCSpeech;
            geminiNpc.OnConversationEnded -= HandleConversationEnded;
        }
        if (speechManager != null)
        {
            speechManager.OnSpeechFinished -= HandleSpeechFinished;
        }
    }

    private void HandleStreamingNPCSpeech(string streamingResponse)
    {
        ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, streamingResponse);
    }



    private void HandleFinalNPCSpeech(string finalResponse)
    {
        Debug.Log($"<color=lime>UIBridge:</color> Final Response. Requesting speech for: '{finalResponse}'");
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "...");
        // --- The only line that changes: call the manager instead of a specific synthesizer ---
        speechManager.Speak(finalResponse, geminiNpc.Personality);
    }

    private void HandleSpeechFinished()
    {
        geminiNpc.ResumeListening();
    }

    private void HandleConversationEnded()
    {
        Debug.Log("<color=cyan>UIBridge:</color> Conversation ended. Hiding dialogue UI.");
        ManagerGlobal.Instance.DialogueManager.HideDynamicDialogue();
    }
}