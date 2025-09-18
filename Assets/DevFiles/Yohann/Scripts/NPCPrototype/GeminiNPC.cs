// GeminiNPC.cs (Definitive Version with Coroutine-based Graceful Shutdown)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class GeminiNPC : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string googleCloudApiKey;
    [Header("AI Personality")]
    [SerializeField] private NPCPersonality personality;
    public NPCPersonality Personality => personality;

    #region Events
    public event Action<string> OnPlayerTranscriptReceived;
    public event Action<string> OnNPCResponseReceived;
    public event Action<string> OnNPCTurnEnded;
    public event Action OnConversationEnded;
    public event Action OnConversationConcludedByAI;
    #endregion

    public bool IsConversationActive { get; private set; } = false;
    private bool isRecording = false;
    private bool isWaitingForResponse = false;
    private float timeSinceLastSpeech = 0f;
    private bool isQuitting = false;

    private SpeechManager speechManager;
    private bool shutdownRequested = false;

    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:streamGenerateContent?key=";
    private const int RECORDING_LENGTH_SECONDS = 60;
    private const int INPUT_SAMPLE_RATE = 16000;

    private AudioSource audioSource;
    private AudioClip recordingClip;
    private UnityWebRequest activeRequest;
    private StringBuilder jsonBuffer = new StringBuilder();
    private string accumulatedResponse = "";
    private List<Content> conversationHistory = new List<Content>();

    #region JSON Data Structures
    [Serializable] public class Content { public string role; public BasePart[] parts; }
    [Serializable] public abstract class BasePart { }
    [Serializable] public class TextPart : BasePart { public string text; }
    [Serializable] public class AudioPart : BasePart { public InlineData inline_data; }
    [Serializable] public class InlineData { public string mime_type; public string data; }
    [Serializable] public class GeminiLiveResponse { public GeminiCandidate[] candidates; }
    [Serializable] public class GeminiCandidate { public GeminiContentResponse content; }
    [Serializable] public class GeminiContentResponse { public TextPart[] parts; }
    #endregion

    #region Custom Download Handler
    private class StreamingDownloadHandler : DownloadHandlerScript
    {
        public Action<string> OnDataReceived;
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0) return true;
            string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
            OnDataReceived?.Invoke(chunk);
            return true;
        }
    }
    #endregion

    #region Unity Lifecycle & Initialization
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (personality == null)
        {
            Debug.LogError($"GeminiNPC on '{gameObject.name}' is missing an NPCPersonality asset!", this);
            this.enabled = false;
        }
    }

    private void OnApplicationQuit() { isQuitting = true; }

    private void OnDestroy()
    {
        StopAllCoroutines();
        // FinalizeShutdown will be called, but we also need to ensure events are unsubscribed if manager exists.
        if (speechManager != null)
        {
            // The Initialize method is the only place we subscribe, so we don't need to unsubscribe here.
            // The speechManager will be destroyed anyway.
        }
        FinalizeShutdown();
    }

    public void Initialize(SpeechManager manager)
    {
        this.speechManager = manager;
    }

    private void Update()
    {
        if (isRecording)
        {
            float currentVolume = GetMicrophoneVolume();
            if (currentVolume > personality.VadThreshold) timeSinceLastSpeech = 0f;
            else timeSinceLastSpeech += Time.deltaTime;

            if (timeSinceLastSpeech > personality.SilenceTimeout) EndRecordingAndSendRequest();
        }
    }
    #endregion

    #region Conversation Flow
    public void StartConversation()
    {
        if (IsConversationActive || ManagerGlobal.Instance.IsPlayerEngaged) return;
        StopAllCoroutines(); // Stop any lingering shutdown coroutines from a previous, aborted interaction
        IsConversationActive = true;
        isRecording = true;
        isWaitingForResponse = false;
        shutdownRequested = false;
        ManagerGlobal.Instance.IsPlayerEngaged = true;
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "I'm listening...");
        conversationHistory.Clear();
        recordingClip = Microphone.Start(null, false, RECORDING_LENGTH_SECONDS, INPUT_SAMPLE_RATE);
        timeSinceLastSpeech = 0f;
    }

    public void EndConversation()
    {
        if (shutdownRequested || !IsConversationActive) return;
        Debug.Log("<color=orange>GeminiNPC:</color> Shutdown has been requested. Starting graceful shutdown coroutine.");
        shutdownRequested = true;

        if (isRecording)
        {
            if (Microphone.IsRecording(null)) Microphone.End(null);
            isRecording = false;
        }

        StartCoroutine(GracefulShutdownCoroutine());
    }

    private IEnumerator GracefulShutdownCoroutine()
    {
        // Wait one frame. This gives the SpeechManager time to start its own web request
        // and for its IsSpeaking state to become true once audio playback begins.
        yield return null;

        // Now, patiently wait as long as the speech system is busy.
        while (speechManager != null && speechManager.IsSpeaking())
        {
            yield return null;
        }

        Debug.Log("<color=orange>GeminiNPC:</color> Speech has finished. Finalizing shutdown.");
        FinalizeShutdown();
    }

    private void FinalizeShutdown()
    {
        if (!IsConversationActive && !shutdownRequested) return;

        Debug.Log("<color=red>GeminiNPC:</color> Finalizing conversation state.");

        if (Microphone.IsRecording(null)) Microphone.End(null);
        activeRequest?.Abort();

        isRecording = false;
        isWaitingForResponse = false;
        IsConversationActive = false;
        conversationHistory.Clear();
        shutdownRequested = false;

        if (!isQuitting && ManagerGlobal.Instance != null)
        {
            Invoke(nameof(ReleasePlayerEngagement), 0.1f);
            if (ManagerGlobal.Instance.ThoughtManager != null)
                ManagerGlobal.Instance.ThoughtManager.ClearCurrentThought();
        }

        OnConversationEnded?.Invoke();
    }

    public void ResumeListening()
    {
        if (!IsConversationActive || shutdownRequested) return;
        Debug.Log("<color=orange>GeminiNPC:</color> Speech finished. Resuming listening.");
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "I'm listening...");
        isRecording = true;
        recordingClip = Microphone.Start(null, false, RECORDING_LENGTH_SECONDS, INPUT_SAMPLE_RATE);
        timeSinceLastSpeech = 0f;
    }

    private void ReleasePlayerEngagement()
    {
        if (!isQuitting && ManagerGlobal.Instance != null)
        {
            ManagerGlobal.Instance.IsPlayerEngaged = false;
        }
    }
    #endregion

    #region API Communication & Data Handling
    private async void EndRecordingAndSendRequest()
    {
        isRecording = false;
        isWaitingForResponse = true;
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "Let me think...");
        int lastSample = Microphone.GetPosition(null);
        Microphone.End(null);
        if (lastSample == 0)
        {
            Debug.LogWarning("No audio was recorded.");
            isWaitingForResponse = false;
            ResumeListening();
            return;
        }
        float[] samples = new float[lastSample];
        recordingClip.GetData(samples, 0);
        int channels = recordingClip.channels;
        int frequency = recordingClip.frequency;
        Destroy(recordingClip);
        await SendAudioToGeminiAsync(samples, channels, frequency);
    }

    private async Task SendAudioToGeminiAsync(float[] samples, int channels, int frequency)
    {
        accumulatedResponse = "";
        OnNPCResponseReceived?.Invoke("");
        jsonBuffer.Clear();

        byte[] wavData = await Task.Run(() => EncodeToWav(samples, channels, frequency));
        string base64Audio = Convert.ToBase64String(wavData);
        var userTurn = new Content
        {
            role = "user",
            parts = new BasePart[] { new AudioPart { inline_data = new InlineData { mime_type = "audio/wav", data = base64Audio } } }
        };
        List<Content> allContents = new List<Content>(conversationHistory);
        allContents.Add(userTurn);
        string jsonBody = BuildManualJson(personality.SystemContext, allContents);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        activeRequest = new UnityWebRequest(GEMINI_API_URL + googleCloudApiKey, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new StreamingDownloadHandler { OnDataReceived = ProcessStreamedData }
        };
        activeRequest.SetRequestHeader("Content-Type", "application/json");

        await activeRequest.SendWebRequest();

        isWaitingForResponse = false;

        if (activeRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Gemini API Error: {activeRequest.error} - {activeRequest.downloadHandler.text}");
            OnNPCResponseReceived?.Invoke("Sorry, I'm having trouble thinking right now.");
            OnNPCTurnEnded?.Invoke("Sorry, I'm having trouble thinking right now.");
        }
        else
        {
            Debug.Log("Successfully received full stream from Gemini.");
            bool shouldEndConversation = accumulatedResponse.Contains("[END_CONVERSATION]");
            if (shouldEndConversation)
            {
                accumulatedResponse = accumulatedResponse.Replace("[END_CONVERSATION]", "").Trim();
                Debug.Log("<color=magenta>GeminiNPC:</color> AI has signaled the end of the conversation.");
            }

            conversationHistory.Add(userTurn);
            var modelTurn = new Content
            {
                role = "model",
                parts = new BasePart[] { new TextPart { text = accumulatedResponse } }
            };
            conversationHistory.Add(modelTurn);

            OnNPCTurnEnded?.Invoke(accumulatedResponse);

            if (shouldEndConversation)
            {
                OnConversationConcludedByAI?.Invoke();
                EndConversation();
            }
        }

        activeRequest.Dispose();
        activeRequest = null;
    }

    private string EscapeStringForJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    private string BuildManualJson(string systemContext, List<Content> contents)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("\"system_instruction\": { \"parts\": [ { \"text\": \"");
        sb.Append(EscapeStringForJson(systemContext));
        sb.Append("\" } ] },");
        sb.Append("\"contents\": [");
        for (int i = 0; i < contents.Count; i++)
        {
            var content = contents[i];
            sb.Append("{");
            sb.Append($"\"role\": \"{content.role}\",");
            sb.Append("\"parts\": [");
            if (content.parts[0] is TextPart textPart)
            {
                sb.Append("{ \"text\": \"");
                sb.Append(EscapeStringForJson(textPart.text));
                sb.Append("\" }");
            }
            else if (content.parts[0] is AudioPart audioPart)
            {
                sb.Append("{ \"inline_data\": ");
                sb.Append(JsonUtility.ToJson(audioPart.inline_data));
                sb.Append(" }");
            }
            sb.Append("]");
            sb.Append("}");
            if (i < contents.Count - 1) sb.Append(",");
        }
        sb.Append("]");
        sb.Append("}");
        return sb.ToString();
    }

    private void ProcessStreamedData(string chunk)
    {
        jsonBuffer.Append(chunk);
        while (true)
        {
            string bufferString = jsonBuffer.ToString();
            int startIndex = bufferString.IndexOf('{');
            if (startIndex == -1) break;
            int braceCount = 0;
            int endIndex = -1;
            for (int i = startIndex; i < bufferString.Length; i++)
            {
                if (bufferString[i] == '{') braceCount++;
                else if (bufferString[i] == '}') braceCount--;
                if (braceCount == 0) { endIndex = i; break; }
            }
            if (endIndex == -1) break;
            int length = endIndex - startIndex + 1;
            string completeJson = bufferString.Substring(startIndex, length);
            try
            {
                var response = JsonUtility.FromJson<GeminiLiveResponse>(completeJson);
                if (response?.candidates != null && response.candidates.Length > 0 && response.candidates[0].content?.parts != null)
                {
                    string text = response.candidates[0].content.parts[0].text;
                    // Append the raw, unfiltered text to our internal accumulator
                    accumulatedResponse += text;

                    // Create a separate, clean version of the text to send to the UI.
                    // This ensures the UI never sees our special tags, but our script can still use them.
                    string displayResponse = accumulatedResponse;
                    if (displayResponse.Contains("[END_CONVERSATION]"))
                    {
                        displayResponse = displayResponse.Replace("[END_CONVERSATION]", "").Trim();
                    }

                    // Fire the event with the CLEANED text.
                    OnNPCResponseReceived?.Invoke(displayResponse);
                }
            }
            catch (Exception e) { Debug.LogWarning($"JSON Parse Error: {e.Message}. Object: {completeJson}"); }
            jsonBuffer.Remove(0, endIndex + 1);
        }
    }
    #endregion

    #region Audio Utilities
    private float GetMicrophoneVolume()
    {
        if (!Microphone.IsRecording(null)) return 0;
        int sampleWindow = 128;
        float[] data = new float[sampleWindow];
        int currentPosition = Microphone.GetPosition(null) - (sampleWindow + 1);
        if (currentPosition < 0) return 0;
        recordingClip.GetData(data, currentPosition);
        float totalLoudness = 0;
        foreach (var sample in data) totalLoudness += Mathf.Abs(sample);
        return totalLoudness / sampleWindow;
    }

    private byte[] EncodeToWav(float[] samples, int channels, int frequency)
    {
        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(0), 0, 4);
            memoryStream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(16), 0, 4);
            memoryStream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
            memoryStream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
            memoryStream.Write(BitConverter.GetBytes(frequency), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(frequency * channels * 2), 0, 4);
            memoryStream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
            memoryStream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
            memoryStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(0), 0, 4);
            short[] intData = new short[samples.Length];
            byte[] byteData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++) intData[i] = (short)(samples[i] * 32767);
            Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);
            memoryStream.Write(byteData, 0, byteData.Length);
            long fileSize = memoryStream.Length;
            memoryStream.Seek(4, SeekOrigin.Begin);
            memoryStream.Write(BitConverter.GetBytes((int)(fileSize - 8)), 0, 4);
            memoryStream.Seek(40, SeekOrigin.Begin);
            memoryStream.Write(BitConverter.GetBytes((int)(fileSize - 44)), 0, 4);
            return memoryStream.ToArray();
        }
    }
    #endregion
}