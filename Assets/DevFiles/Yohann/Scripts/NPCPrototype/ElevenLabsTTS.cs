// ElevenLabsSynthesizer.cs (Upgraded for Streaming)
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class ElevenLabsSynthesizer : MonoBehaviour, ITextToSpeech
{
    [Tooltip("Your API Key from the ElevenLabs website.")]
    [SerializeField] private string elevenLabsApiKey;

    private const string TTS_API_URL_TEMPLATE = "https://api.elevenlabs.io/v1/text-to-speech/{0}";
    private UnityWebRequest activeRequest;

    [Serializable] private class TTSRequest { public string text; public string model_id = "eleven_turbo_v2"; public VoiceSettings voice_settings; }
    [Serializable] private class VoiceSettings { public float stability; public float similarity_boost; }

    public bool HasValidAPIKey() => !string.IsNullOrEmpty(elevenLabsApiKey);

    public void Stop()
    {
        if (activeRequest != null && !activeRequest.isDone)
        {
            activeRequest.Abort();
            activeRequest = null;
        }
    }

    public IEnumerator Synthesize(string textToSpeak, NPCPersonality personality, Action<AudioClip> onClipReady)
    {
        if (!HasValidAPIKey())
        {
            Debug.LogError("ElevenLabsSynthesizer: API key is not set.");
            onClipReady?.Invoke(null);
            yield break;
        }

        string url = string.Format(TTS_API_URL_TEMPLATE, personality.ElevenLabsVoiceId);
        var requestBody = new TTSRequest
        {
            text = textToSpeak,
            voice_settings = new VoiceSettings
            {
                stability = personality.VoiceStability,
                similarity_boost = personality.VoiceSimilarityBoost
            }
        };
        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (activeRequest = new UnityWebRequest(url, "POST"))
        {
            activeRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            activeRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            activeRequest.SetRequestHeader("Content-Type", "application/json");
            activeRequest.SetRequestHeader("xi-api-key", elevenLabsApiKey);
            yield return activeRequest.SendWebRequest();

            if (activeRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ElevenLabs API Error: {activeRequest.error} - {activeRequest.downloadHandler.text}");
                onClipReady?.Invoke(null);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(activeRequest);
                if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
                {
                    onClipReady?.Invoke(clip);
                }
                else
                {
                    Debug.LogError("ElevenLabsSynthesizer: Failed to create AudioClip from response.");
                    onClipReady?.Invoke(null);
                }
            }
        }
        activeRequest = null;
    }
}