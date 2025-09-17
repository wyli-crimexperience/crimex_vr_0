// ElevenLabsSynthesizer.cs
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ElevenLabsSynthesizer : MonoBehaviour
{
    [Tooltip("Your API Key from the ElevenLabs website.")]
    [SerializeField] private string elevenLabsApiKey;

    private const string TTS_API_URL_TEMPLATE = "https://api.elevenlabs.io/v1/text-to-speech/{0}";
    private AudioSource audioSource;

    public event Action OnSpeechFinished;
    private Coroutine speechCoroutine; // To manage the active speech task

    [Serializable] private class TTSRequest { public string text; public string model_id = "eleven_turbo_v2"; public VoiceSettings voice_settings; }
    [Serializable] private class VoiceSettings { public float stability = 0.7f; public float similarity_boost = 0.7f; }

private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Speak(string textToSpeak, NPCPersonality personality)
    {
        if (string.IsNullOrEmpty(textToSpeak) || string.IsNullOrEmpty(elevenLabsApiKey))
        {
            if (string.IsNullOrEmpty(elevenLabsApiKey)) Debug.LogError("ElevenLabs API Key is not set.");
            return;
        }

        // If a speech coroutine is already running, stop it
        if (speechCoroutine != null)
        {
            StopCoroutine(speechCoroutine);
        }

        speechCoroutine = StartCoroutine(SynthesizeAndPlaySpeechCoroutine(textToSpeak, personality));
    }


    private IEnumerator SynthesizeAndPlaySpeechCoroutine(string text, NPCPersonality personality)
    {
        string url = string.Format(TTS_API_URL_TEMPLATE, personality.ElevenLabsVoiceId);

        var voiceSettings = new VoiceSettings
        {
            stability = personality.VoiceStability,
            similarity_boost = personality.VoiceSimilarityBoost
        };
        var requestBody = new TTSRequest { text = text, voice_settings = voiceSettings };
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
                Debug.LogError($"ElevenLabs API Error: {request.error} - {request.downloadHandler.text}");
                // --- NEW CODE: Fire completion event even on error so we don't get stuck ---
                OnSpeechFinished?.Invoke();
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
            {
                audioSource.clip = clip;
                audioSource.Play();

                // --- NEW CODE: Wait for the audio to finish playing ---
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            else
            {
                Debug.LogError("Failed to get AudioClip from ElevenLabs response.");
            }
        }

        // --- NEW CODE: Fire the completion event ---
        OnSpeechFinished?.Invoke();
        speechCoroutine = null;
    }

}