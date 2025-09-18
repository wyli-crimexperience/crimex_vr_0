// GoogleCloudTTS.cs (Upgraded for Streaming)
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GoogleCloudTTS : MonoBehaviour, ITextToSpeech
{
    [Tooltip("Your API Key from the Google Cloud console.")]
    [SerializeField] private string googleCloudApiKey;
    private const string TTS_API_URL = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=";
    private UnityWebRequest activeRequest;

    #region JSON Data Structures
    [Serializable] private class TTSRequest { public InputData input; public VoiceData voice; public AudioConfig audioConfig; }
    [Serializable] private class InputData { public string text; }
    [Serializable] private class VoiceData { public string languageCode = "en-US"; public string name; }
    [Serializable] private class AudioConfig { public string audioEncoding = "LINEAR16"; public int sampleRateHertz; public string[] effectsProfileId; public float pitch; public float speakingRate; }
    [Serializable] private class TTSResponse { public string audioContent; public ErrorResponse error; }
    [Serializable] private class ErrorResponse { public int code; public string message; public string status; }
    #endregion

    public bool HasValidAPIKey() => !string.IsNullOrEmpty(googleCloudApiKey);

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
            Debug.LogError("GoogleCloudTTS: API key is not set.");
            onClipReady?.Invoke(null);
            yield break;
        }

        int sampleRate = (personality.GoogleTTS_VoiceName.Contains("Studio") || personality.GoogleTTS_VoiceName.Contains("Chirp")) ? 48000 : 24000;

        var requestBody = new TTSRequest
        {
            input = new InputData { text = textToSpeak },
            voice = new VoiceData { name = personality.GoogleTTS_VoiceName },
            audioConfig = new AudioConfig
            {
                audioEncoding = "LINEAR16",
                sampleRateHertz = sampleRate,
                pitch = personality.GoogleTTS_Pitch,
                speakingRate = personality.GoogleTTS_SpeakingRate,
                effectsProfileId = (personality.GoogleTTS_EffectsProfileIds != null && personality.GoogleTTS_EffectsProfileIds.Length > 0) ? personality.GoogleTTS_EffectsProfileIds : null
            }
        };

        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        string jsonBody = JsonConvert.SerializeObject(requestBody, settings);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (activeRequest = new UnityWebRequest(TTS_API_URL + googleCloudApiKey, "POST"))
        {
            activeRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            activeRequest.downloadHandler = new DownloadHandlerBuffer();
            activeRequest.SetRequestHeader("Content-Type", "application/json");
            yield return activeRequest.SendWebRequest();

            if (activeRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Google TTS API Error: {activeRequest.error} - {activeRequest.downloadHandler.text}");
                onClipReady?.Invoke(null);
            }
            else
            {
                var response = JsonConvert.DeserializeObject<TTSResponse>(activeRequest.downloadHandler.text);
                if (response == null || string.IsNullOrEmpty(response.audioContent))
                {
                    Debug.LogError($"Google TTS returned an empty or invalid response. Error: {response?.error?.message}");
                    onClipReady?.Invoke(null);
                    yield break;
                }

                byte[] audioBytes = Convert.FromBase64String(response.audioContent);
                float[] floatSamples = PcmToFloat(audioBytes);
                AudioClip clip = AudioClip.Create("GoogleSpeech", floatSamples.Length, 1, sampleRate, false);
                clip.SetData(floatSamples, 0);

                onClipReady?.Invoke(clip);
            }
        }
        activeRequest = null;
    }

    private float[] PcmToFloat(byte[] pcmBytes)
    {
        int sampleCount = pcmBytes.Length / 2;
        float[] floatSamples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short pcmSample = (short)((pcmBytes[i * 2 + 1] << 8) | pcmBytes[i * 2]);
            floatSamples[i] = pcmSample / 32768f;
        }
        return floatSamples;
    }
}