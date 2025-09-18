// ElevenLabsSynthesizer.cs (Refactored)
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ElevenLabsSynthesizer : MonoBehaviour, ITextToSpeech // Implement the interface
{
    [Tooltip("Your API Key from the ElevenLabs website.")]
    [SerializeField] private string elevenLabsApiKey;

    private const string TTS_API_URL_TEMPLATE = "https://api.elevenlabs.io/v1/text-to-speech/{0}";
    private AudioSource audioSource;
    private Coroutine speechCoroutine;

    // --- Interface Events ---
    public event Action OnSpeechFinished;
    public event Action<string> OnSpeechFailed;

    [Serializable] private class TTSRequest { public string text; public string model_id = "eleven_turbo_v2"; public VoiceSettings voice_settings; }
    [Serializable] private class VoiceSettings { public float stability; public float similarity_boost; }

    private void Awake() { audioSource = GetComponent<AudioSource>(); }

    // --- Interface Methods ---
    public bool HasValidAPIKey() => !string.IsNullOrEmpty(elevenLabsApiKey);

    public bool IsSpeaking() => (speechCoroutine != null || (audioSource != null && audioSource.isPlaying));

    public void Stop()
    {
        if (speechCoroutine != null) StopCoroutine(speechCoroutine);
        if (audioSource.isPlaying) audioSource.Stop();
        speechCoroutine = null;
    }

    public void Speak(string textToSpeak, NPCPersonality personality)
    {
        if (!HasValidAPIKey())
        {
            OnSpeechFailed?.Invoke("ElevenLabs API Key is not set.");
            return;
        }
        if (IsSpeaking()) Stop();
        speechCoroutine = StartCoroutine(SynthesizeAndPlaySpeechCoroutine(textToSpeak, personality));
    }

    private IEnumerator SynthesizeAndPlaySpeechCoroutine(string text, NPCPersonality personality)
    {
        string url = string.Format(TTS_API_URL_TEMPLATE, personality.ElevenLabsVoiceId);
        var requestBody = new TTSRequest
        {
            text = text,
            voice_settings = new VoiceSettings
            {
                stability = personality.VoiceStability,
                similarity_boost = personality.VoiceSimilarityBoost
            }
        };
        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", elevenLabsApiKey);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string error = $"ElevenLabs API Error: {request.error} - {request.downloadHandler.text}";
                Debug.LogError(error);
                OnSpeechFailed?.Invoke(error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
            {
                audioSource.clip = clip;
                audioSource.Play();
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            else
            {
                string error = "Failed to get AudioClip from ElevenLabs response.";
                Debug.LogError(error);
                OnSpeechFailed?.Invoke(error);
                yield break;
            }
        }
        OnSpeechFinished?.Invoke();
        speechCoroutine = null;
    }
}