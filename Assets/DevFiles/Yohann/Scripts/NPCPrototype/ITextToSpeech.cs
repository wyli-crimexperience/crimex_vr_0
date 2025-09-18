// ITextToSpeech.cs
using System;

// This interface defines the essential capabilities of any Text-to-Speech service.
public interface ITextToSpeech
{
    // Event fired when the audio clip has finished playing.
    event Action OnSpeechFinished;
    // Event fired if the API call or audio playback fails.
    event Action<string> OnSpeechFailed;

    // Starts the process of synthesizing and speaking the provided text.
    void Speak(string textToSpeak, NPCPersonality personality);

    // Immediately stops any currently playing speech.
    void Stop();

    // Returns true if the synthesizer is currently generating or playing audio.
    bool IsSpeaking();

    // A check to see if the service is configured with a valid API key.
    bool HasValidAPIKey();
}