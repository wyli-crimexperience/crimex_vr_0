// GoogleCloudTTS.cs (Definitive Version for v1beta1 with Studio Voices)
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
// --- Required for robust JSON serialization ---
using Newtonsoft.Json;

[RequireComponent(typeof(AudioSource))]
public class GoogleCloudTTS : MonoBehaviour, ITextToSpeech
{
    [Tooltip("Your API Key from the Google Cloud console.")]
    [SerializeField] private string googleCloudApiKey;
    private const string TTS_API_URL = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=";

    private AudioSource audioSource;
    private Coroutine speechCoroutine;
    public event Action OnSpeechFinished;
    public event Action<string> OnSpeechFailed;

    #region JSON Data Structures
    // These C# classes exactly match the target JSON structure
    [Serializable] private class TTSRequest { public InputData input; public VoiceData voice; public AudioConfig audioConfig; }
    [Serializable] private class InputData { public string text; }
    [Serializable] private class VoiceData { public string languageCode = "en-US"; public string name; }
    [Serializable] private class AudioConfig { public string audioEncoding = "LINEAR16"; public int sampleRateHertz; public string[] effectsProfileId; public float pitch; public float speakingRate; }
    [Serializable] private class TTSResponse { public string audioContent; public ErrorResponse error; }
    [Serializable] private class ErrorResponse { public int code; public string message; public string status; }
    #endregion

    private void Awake() { audioSource = GetComponent<AudioSource>(); }

    public bool HasValidAPIKey() => !string.IsNullOrEmpty(googleCloudApiKey);
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
            OnSpeechFailed?.Invoke("Google Cloud TTS API Key is not set.");
            return;
        }
        if (IsSpeaking()) Stop();
        speechCoroutine = StartCoroutine(SynthesizeAndPlaySpeechCoroutine(textToSpeak, personality));
    }

    private IEnumerator SynthesizeAndPlaySpeechCoroutine(string text, NPCPersonality personality)
    {
        // --- Step 1: Dynamically determine the correct sample rate ---
        int sampleRate;
        // Chirp and Studio voices require 48kHz, others use 24kHz.
        if (personality.GoogleTTS_VoiceName.Contains("Studio") || personality.GoogleTTS_VoiceName.Contains("Chirp"))
        {
            sampleRate = 48000;
        }
        else
        {
            sampleRate = 24000;
        }

        // --- Step 2: Construct the request object ---
        var requestBody = new TTSRequest
        {
            input = new InputData { text = text },
            voice = new VoiceData { name = personality.GoogleTTS_VoiceName },
            audioConfig = new AudioConfig
            {
                audioEncoding = "LINEAR16",
                sampleRateHertz = sampleRate,
                pitch = personality.GoogleTTS_Pitch,
                speakingRate = personality.GoogleTTS_SpeakingRate,
                // Set to null if the array is empty. This is key for the serializer.
                effectsProfileId = (personality.GoogleTTS_EffectsProfileIds != null && personality.GoogleTTS_EffectsProfileIds.Length > 0) ? personality.GoogleTTS_EffectsProfileIds : null
            }
        };

        // --- Step 3: Serialize using Newtonsoft.Json for proper optional field handling ---
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore // This will OMIT null fields (like effectsProfileId) from the JSON
        };
        string jsonBody = JsonConvert.SerializeObject(requestBody, settings);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(TTS_API_URL + googleCloudApiKey, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"<color=lime>--- Sending v1beta1 JSON ---</color>\n{jsonBody}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string error = $"Google TTS API Error (Network): {request.error} - {request.downloadHandler.text}";
                Debug.LogError(error);
                OnSpeechFailed?.Invoke(error);
                yield break;
            }
            var response = JsonConvert.DeserializeObject<TTSResponse>(request.downloadHandler.text);
            if (response.error != null && !string.IsNullOrEmpty(response.error.message))
            {
                string error = $"Google TTS API Error (Logic): {response.error.message} (Code: {response.error.code})";
                Debug.LogError(error);
                OnSpeechFailed?.Invoke(error);
                yield break;
            }
            if (string.IsNullOrEmpty(response.audioContent))
            {
                string error = "Google TTS API returned an empty audioContent field. Verify API key and that the Text-to-Speech API is enabled.";
                Debug.LogError(error);
                OnSpeechFailed?.Invoke(error);
                yield break;
            }

            byte[] audioBytes = Convert.FromBase64String(response.audioContent);
            float[] floatSamples = PcmToFloat(audioBytes);
            AudioClip clip = AudioClip.Create("GoogleSpeech", floatSamples.Length, 1, sampleRate, false);
            clip.SetData(floatSamples, 0);

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            else
            {
                string error = "Failed to create AudioClip from Google TTS LINEAR16 data.";
                Debug.LogError(error);
                OnSpeechFailed?.Invoke(error);
                yield break;
            }
        }
        OnSpeechFinished?.Invoke();
        speechCoroutine = null;
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