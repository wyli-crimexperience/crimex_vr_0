// ITextToSpeech.cs (Upgraded for Streaming)
using System;
using System.Collections;
using UnityEngine;

public interface ITextToSpeech
{
    // Synthesizes audio and returns the clip via a callback.
    // This is the primary method for the streaming system.
    IEnumerator Synthesize(string textToSpeak, NPCPersonality personality, Action<AudioClip> onClipReady);

    // Immediately stops any active synthesis or playback.
    void Stop();

    // A check to see if the service is configured with a valid API key.
    bool HasValidAPIKey();
}