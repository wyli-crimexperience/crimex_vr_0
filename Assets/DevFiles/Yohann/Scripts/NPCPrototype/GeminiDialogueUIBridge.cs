// GeminiDialogueUIBridge.cs (Final Final Version)
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
        // Unsubscribe from all events first to be safe
        geminiNpc.OnPlayerTranscriptReceived -= HandlePlayerSpeech;
        geminiNpc.OnNPCResponseReceived -= HandleStreamingNPCSpeech;
        geminiNpc.OnNPCTurnEnded -= HandleFinalNPCSpeech;
        speechSynthesizer.OnSpeechFinished -= HandleSpeechFinished; // Unsubscribe from our new event

        // Subscribe to all events
        geminiNpc.OnPlayerTranscriptReceived += HandlePlayerSpeech;
        geminiNpc.OnNPCResponseReceived += HandleStreamingNPCSpeech;
        geminiNpc.OnNPCTurnEnded += HandleFinalNPCSpeech;
        speechSynthesizer.OnSpeechFinished += HandleSpeechFinished; // Subscribe to our new event
    }

    private void OnDisable()
    {
        if (geminiNpc != null)
        {
            geminiNpc.OnPlayerTranscriptReceived -= HandlePlayerSpeech;
            geminiNpc.OnNPCResponseReceived -= HandleStreamingNPCSpeech;
            geminiNpc.OnNPCTurnEnded -= HandleFinalNPCSpeech;
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
        // Add GetInstanceID() to the log. This gives a unique ID for the component.
        Debug.Log($"<color=yellow>UIBridge ({GetInstanceID()}):</color> Streaming Update: '{streamingResponse}'");
        ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, streamingResponse);
    }

    private void HandleFinalNPCSpeech(string finalResponse)
    {
        Debug.Log($"<color=lime>UIBridge ({GetInstanceID()}):</color> Final Response. Speaking: '{finalResponse}'");
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "..."); // Thinking/Speaking indicator
        speechSynthesizer.Speak(finalResponse);
    }

    // --- NEW CODE: This is called by the synthesizer when it's done ---
    private void HandleSpeechFinished()
    {
        // Tell the NPC it's now safe to start listening for the player again.
        geminiNpc.ResumeListening();
    }
}