// In GeminiDialogueUIBridge.cs
using UnityEngine;

[RequireComponent(typeof(GeminiNPC))]
[RequireComponent(typeof(ElevenLabsSynthesizer))]
public class GeminiDialogueUIBridge : MonoBehaviour
{
    private GeminiNPC geminiNpc;
    private ElevenLabsSynthesizer speechSynthesizer;

    private void Awake()
    {
        geminiNpc = GetComponent<GeminiNPC>();
        speechSynthesizer = GetComponent<ElevenLabsSynthesizer>();
    }

    private void OnEnable()
    {
        // Unsubscribe first to be safe
        geminiNpc.OnPlayerTranscriptReceived -= HandlePlayerSpeech;
        geminiNpc.OnNPCResponseReceived -= HandleStreamingNPCSpeech;
        geminiNpc.OnNPCTurnEnded -= HandleFinalNPCSpeech;
        speechSynthesizer.OnSpeechFinished -= HandleSpeechFinished;
        geminiNpc.OnConversationEnded -= HandleConversationEnded; // --- NEW ---

        // Subscribe to all events
        geminiNpc.OnPlayerTranscriptReceived += HandlePlayerSpeech;
        geminiNpc.OnNPCResponseReceived += HandleStreamingNPCSpeech;
        geminiNpc.OnNPCTurnEnded += HandleFinalNPCSpeech;
        speechSynthesizer.OnSpeechFinished += HandleSpeechFinished;
        geminiNpc.OnConversationEnded += HandleConversationEnded; // --- NEW ---
    }

    private void OnDisable()
    {
        if (geminiNpc != null)
        {
            geminiNpc.OnPlayerTranscriptReceived -= HandlePlayerSpeech;
            geminiNpc.OnNPCResponseReceived -= HandleStreamingNPCSpeech;
            geminiNpc.OnNPCTurnEnded -= HandleFinalNPCSpeech;
            geminiNpc.OnConversationEnded -= HandleConversationEnded; // --- NEW ---
        }
        if (speechSynthesizer != null)
        {
            speechSynthesizer.OnSpeechFinished -= HandleSpeechFinished;
        }
    }

    private void HandlePlayerSpeech(string transcript)
    {
        ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine("Player", transcript);
    }

    private void HandleStreamingNPCSpeech(string streamingResponse)
    {
        Debug.Log($"<color=yellow>UIBridge ({GetInstanceID()}):</color> Streaming Update: '{streamingResponse}'");
        ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, streamingResponse);
    }

    private void HandleFinalNPCSpeech(string finalResponse)
    {
        Debug.Log($"<color=lime>UIBridge ({GetInstanceID()}):</color> Final Response. Speaking: '{finalResponse}'");
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "...");
        speechSynthesizer.Speak(finalResponse, geminiNpc.Personality); // Assuming you're using the NPCPersonality version
    }

    private void HandleSpeechFinished()
    {
        geminiNpc.ResumeListening();
    }

    // --- ADD THIS ENTIRE METHOD ---
    /// <summary>
    /// Called by the GeminiNPC.OnConversationEnded event.
    /// This method is responsible for cleaning up the UI.
    /// </summary>
    private void HandleConversationEnded()
    {
        Debug.Log("<color=cyan>UIBridge:</color> Conversation ended. Hiding dialogue UI.");
        // Use the specific method in DialogueManager for hiding the AI-driven dialogue
        ManagerGlobal.Instance.DialogueManager.HideDynamicDialogue();
    }
}