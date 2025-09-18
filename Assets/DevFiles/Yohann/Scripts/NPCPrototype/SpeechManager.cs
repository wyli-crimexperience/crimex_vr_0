// SpeechManager.cs
using UnityEngine;
using System;

[RequireComponent(typeof(ElevenLabsSynthesizer))]
[RequireComponent(typeof(GoogleCloudTTS))]
public class SpeechManager : MonoBehaviour
{
    // The single point of contact for other scripts
    public event Action OnSpeechFinished;

    private ITextToSpeech elevenLabs;
    private ITextToSpeech googleTTS;
    private ITextToSpeech activeSynthesizer;
    private NPCPersonality currentPersonality;
    private string currentTextToSpeak;

    private void Awake()
    {
        // Get references to our available TTS "strategies"
        elevenLabs = GetComponent<ElevenLabsSynthesizer>();
        googleTTS = GetComponent<GoogleCloudTTS>();
    }

    public void Speak(string textToSpeak, NPCPersonality personality)
    {
        currentTextToSpeak = textToSpeak;
        currentPersonality = personality;

        // --- Primary Strategy: Try ElevenLabs first ---
        if (elevenLabs.HasValidAPIKey())
        {
            Debug.Log("<color=green>SpeechManager:</color> Attempting to speak with ElevenLabs.");
            activeSynthesizer = elevenLabs;
            SubscribeToEvents(activeSynthesizer);
            activeSynthesizer.Speak(currentTextToSpeak, currentPersonality);
        }
        // --- Fallback Strategy: If ElevenLabs key is missing, try Google ---
        else if (googleTTS.HasValidAPIKey())
        {
            Debug.Log("<color=orange>SpeechManager:</color> ElevenLabs key missing. Falling back to Google TTS.");
            activeSynthesizer = googleTTS;
            SubscribeToEvents(activeSynthesizer);
            activeSynthesizer.Speak(currentTextToSpeak, currentPersonality);
        }
        // --- No valid strategy ---
        else
        {
            Debug.LogError("SpeechManager: No valid API key found for any TTS service. Speech aborted.");
            OnSpeechFinished?.Invoke();
        }
    }

    private void HandleSpeechFinished()
    {
        UnsubscribeFromEvents(activeSynthesizer);
        OnSpeechFinished?.Invoke();
    }

    private void HandleSpeechFailed(string error)
    {
        UnsubscribeFromEvents(activeSynthesizer);
        Debug.LogWarning($"SpeechManager: {activeSynthesizer.GetType().Name} failed. Reason: {error}");

        // If the failed service was ElevenLabs, retry with Google
        if (activeSynthesizer == elevenLabs && googleTTS.HasValidAPIKey())
        {
            Debug.Log("<color=orange>SpeechManager:</color> ElevenLabs failed. Retrying with Google TTS.");
            activeSynthesizer = googleTTS;
            SubscribeToEvents(activeSynthesizer);
            activeSynthesizer.Speak(currentTextToSpeak, currentPersonality);
        }
        else
        {
            Debug.LogError($"SpeechManager: All available TTS services have failed. Speech aborted.");
            OnSpeechFinished?.Invoke();
        }
    }

    private void SubscribeToEvents(ITextToSpeech synthesizer)
    {
        if (synthesizer == null) return;
        synthesizer.OnSpeechFinished += HandleSpeechFinished;
        synthesizer.OnSpeechFailed += HandleSpeechFailed;
    }

    private void UnsubscribeFromEvents(ITextToSpeech synthesizer)
    {
        if (synthesizer == null) return;
        synthesizer.OnSpeechFinished -= HandleSpeechFinished;
        synthesizer.OnSpeechFailed -= HandleSpeechFailed;
    }

}