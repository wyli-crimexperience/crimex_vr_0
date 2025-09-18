// SpeechManager.cs (Upgraded with Unified Busy State)
using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(ElevenLabsSynthesizer))]
[RequireComponent(typeof(GoogleCloudTTS))]
[RequireComponent(typeof(AudioSource))]
public class SpeechManager : MonoBehaviour
{
    private ITextToSpeech elevenLabs;
    private ITextToSpeech googleTTS;
    private AudioSource playbackSource;

    // --- NEW: Unified state property ---
    public bool IsBusy { get; private set; } = false;

    private void Awake()
    {
        elevenLabs = GetComponent<ElevenLabsSynthesizer>();
        googleTTS = GetComponent<GoogleCloudTTS>();
        playbackSource = GetComponent<AudioSource>();
    }

    public IEnumerator Synthesize(string textToSpeak, NPCPersonality personality, Action<AudioClip> onClipReady)
    {
        // --- Mark as busy as soon as we start synthesis ---
        IsBusy = true;

        ITextToSpeech activeSynthesizer;
        if (elevenLabs.HasValidAPIKey()) activeSynthesizer = elevenLabs;
        else if (googleTTS.HasValidAPIKey()) activeSynthesizer = googleTTS;
        else
        {
            Debug.LogError("SpeechManager: No valid TTS service available for synthesis.");
            onClipReady?.Invoke(null);
            IsBusy = false; // Un-mark if we fail early
            yield break;
        }

        yield return StartCoroutine(activeSynthesizer.Synthesize(textToSpeak, personality, onClipReady));
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null && playbackSource != null)
        {
            playbackSource.clip = clip;
            playbackSource.Play();
            // We are already busy, so no need to set the flag here.
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
        if (playbackSource != null && playbackSource.isPlaying)
        {
            playbackSource.Stop();
        }
        elevenLabs.Stop();
        googleTTS.Stop();
        IsBusy = false; // Force stop means we are no longer busy.
    }

    /// <summary>
    /// This is the primary state check for external scripts.
    /// It returns true if we are synthesizing OR playing audio.
    /// </summary>
    public bool IsSpeaking()
    {
        return playbackSource != null && playbackSource.isPlaying;
    }

    // --- NEW: Method to be called by the bridge when the queue is finished ---
    public void NotifyQueueEmpty()
    {
        // Only set IsBusy to false if we are truly done.
        // A check against IsSpeaking might be needed if synthesis can happen while playing.
        // For our sequential queue, this is safe.
        IsBusy = false;
    }
}