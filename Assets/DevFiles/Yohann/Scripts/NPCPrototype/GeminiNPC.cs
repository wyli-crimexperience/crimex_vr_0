// GeminiNPC.cs (Corrected for UnityWebRequest REST API)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class GeminiNPC : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string googleCloudApiKey;

    [Header("AI Configuration")]
    [TextArea(10, 20)]
    [SerializeField] private string npcContext;

    [Header("Voice Activity Detection (VAD)")]
    [Tooltip("The volume threshold to start considering audio as speech.")]
    [SerializeField][Range(0.001f, 0.1f)] private float vadThreshold = 0.01f;
    [Tooltip("How long the player must be silent (in seconds) to end their turn.")]
    [SerializeField] private float silenceTimeout = 2.0f;

    public event Action<string> OnPlayerTranscriptReceived;
    public event Action<string> OnNPCResponseReceived;

    public bool IsConversationActive { get; private set; } = false;
    private bool isRecording = false;
    private bool isWaitingForResponse = false;
    private float timeSinceLastSpeech = 0f;

    // Use a multimodal model that can understand audio. 'flash' is fast and cost-effective.
    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:streamGenerateContent?key=";
    private const int RECORDING_LENGTH_SECONDS = 60;
    private const int INPUT_SAMPLE_RATE = 16000;

    private AudioSource audioSource;
    private AudioClip recordingClip;
    private UnityWebRequest activeRequest;
    private string accumulatedResponse = "";
    public event Action<string> OnNPCTurnEnded; // Our new event!

    #region JSON Data Structures (REWRITTEN FOR ACCURACY)
    // --- For the Request ---
    [Serializable]
    private class GeminiRestRequest
    {
        public SystemInstruction system_instruction;
        public UserContent[] contents;
    }

    [Serializable]
    private class SystemInstruction
    {
        public TextPart[] parts;
    }

    [Serializable]
    private class UserContent
    {
        public string role = "user";
        public AudioPart[] parts;
    }

    [Serializable]
    private class TextPart
    {
        public string text;
    }

    [Serializable]
    private class AudioPart
    {
        public InlineData inline_data;
    }

    [Serializable]
    private class InlineData
    {
        public string mime_type;
        public string data;
    }

    // --- For the Response ---
    [Serializable]
    public class GeminiLiveResponse { public GeminiCandidate[] candidates; }
    [Serializable]
    public class GeminiCandidate { public GeminiContent content; }
    [Serializable]
    public class GeminiContent { public GeminiPart[] parts; }
    [Serializable]
    public class GeminiPart { public string text; }
    #endregion

    #region Custom Download Handler for Streaming
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

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // This is our main Voice Activity Detection (VAD) loop.
        if (isRecording)
        {
            float currentVolume = GetMicrophoneVolume();
            if (currentVolume > vadThreshold)
            {
                // If player is speaking, reset the silence timer.
                timeSinceLastSpeech = 0f;
            }
            else
            {
                // If player is silent, increment the timer.
                timeSinceLastSpeech += Time.deltaTime;
            }

            // If the player has been silent for long enough, their turn is over.
            if (timeSinceLastSpeech > silenceTimeout)
            {
                EndRecordingAndSendRequest();
            }
        }
    }

    /// <summary>
    /// Starts the conversation process by beginning to listen to the player.
    /// </summary>
    public void StartConversation()
    {
        if (IsConversationActive || ManagerGlobal.Instance.IsPlayerEngaged) return;

        IsConversationActive = true;
        isRecording = true;
        isWaitingForResponse = false;
        ManagerGlobal.Instance.IsPlayerEngaged = true;
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "I'm listening...");

        // Start recording from the microphone
        recordingClip = Microphone.Start(null, false, RECORDING_LENGTH_SECONDS, INPUT_SAMPLE_RATE);
        timeSinceLastSpeech = 0f;
    }

    /// <summary>
    /// Ends the conversation entirely, stopping all processes.
    /// </summary>
    public void EndConversation()
    {
        if (!IsConversationActive) return;

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        if (activeRequest != null && !activeRequest.isDone)
        {
            activeRequest.Abort();
            activeRequest = null;
        }

        isRecording = false;
        isWaitingForResponse = false;
        IsConversationActive = false;

        Invoke(nameof(ReleasePlayerEngagement), 0.1f);
        ManagerGlobal.Instance.ThoughtManager.ClearCurrentThought();
    }

    private void ReleasePlayerEngagement()
    {
        ManagerGlobal.Instance.IsPlayerEngaged = false;
    }

    /// <summary>
    /// Called by the VAD in Update() when silence is detected.
    /// </summary>
    private void EndRecordingAndSendRequest()
    {
        isRecording = false;
        isWaitingForResponse = true;
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "Let me think...");

        // Stop the microphone
        int lastSample = Microphone.GetPosition(null);
        Microphone.End(null);

        if (lastSample == 0)
        {
            Debug.LogWarning("No audio was recorded.");
            // Reset to a listening state instead of ending abruptly
            StartConversationAfterDelay();
            return;
        }

        // Create a new AudioClip with the exact length of the recorded audio
        float[] samples = new float[lastSample];
        recordingClip.GetData(samples, 0);
        AudioClip trimmedClip = AudioClip.Create("PlayerSpeech", lastSample, recordingClip.channels, recordingClip.frequency, false);
        trimmedClip.SetData(samples, 0);

        // Start the process of sending the audio to the API
        StartCoroutine(SendAudioToGemini(trimmedClip));
    }

    // Helper to restart listening after a short delay, e.g., if no audio was captured.
    private void StartConversationAfterDelay()
    {
        isWaitingForResponse = false;
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "Sorry, I didn't hear that. Could you repeat?");
        // Small delay before listening again
        Invoke(nameof(StartListeningAgain), 2.0f);
    }

    private void StartListeningAgain()
    {
        isRecording = true;
        recordingClip = Microphone.Start(null, false, RECORDING_LENGTH_SECONDS, INPUT_SAMPLE_RATE);
        timeSinceLastSpeech = 0f;
    }

    /// <summary>
    /// The main coroutine for handling the REST API communication.
    /// </summary>

    private IEnumerator SendAudioToGemini(AudioClip clip)
    {
        // 1. Hard reset the state for this new turn.
        accumulatedResponse = "";
        OnNPCResponseReceived?.Invoke(""); // Immediately clear the dialogue box text.
        jsonBuffer.Clear(); // Also clear the JSON buffer from the previous turn.

        // 2. Prepare the audio data.
        byte[] wavData = EncodeToWav(clip);
        string base64Audio = Convert.ToBase64String(wavData);

        // 3. Construct the JSON request body.
        var requestBody = new GeminiRestRequest
        {
            system_instruction = new SystemInstruction { parts = new[] { new TextPart { text = npcContext } } },
            contents = new[] { new UserContent { parts = new[] { new AudioPart { inline_data = new InlineData { mime_type = "audio/wav", data = base64Audio } } } } }
        };
        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // 4. Create and configure the UnityWebRequest.
        string url = GEMINI_API_URL + googleCloudApiKey;
        activeRequest = new UnityWebRequest(url, "POST");
        activeRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        activeRequest.SetRequestHeader("Content-Type", "application/json");
        var streamingHandler = new StreamingDownloadHandler();
        streamingHandler.OnDataReceived += ProcessStreamedData;
        activeRequest.downloadHandler = streamingHandler;

        // 5. Send the request and wait for the response.
        Debug.Log("Sending audio request to Gemini API...");
        yield return activeRequest.SendWebRequest();

        if (activeRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Gemini API Error: " + activeRequest.error);
            if (activeRequest.downloadHandler?.data != null)
            {
                Debug.LogError("Error Body: " + Encoding.UTF8.GetString(activeRequest.downloadHandler.data));
            }
            OnNPCResponseReceived?.Invoke("Sorry, I'm having trouble thinking right now.");
            ResumeListening(); // Resume listening even on error.
        }
        else
        {
            Debug.Log("Successfully received full stream from Gemini.");
            // The stream is finished. Fire the event to start speech synthesis.
            OnNPCTurnEnded?.Invoke(accumulatedResponse);
        }

        // 6. Clean up the request.
        activeRequest.Dispose();
        activeRequest = null;
        isWaitingForResponse = false;
    }

    public void ResumeListening()
    {
        Debug.Log("<color=orange>GeminiNPC:</color> Speech finished. Resuming listening.");
        ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject, "I'm listening...");
        StartListeningAgain(); // This is the method that starts the microphone
    }
    /// <summary>
    /// Processes the streaming data chunks as they arrive from the API.
    /// </summary>
    private System.Text.StringBuilder jsonBuffer = new System.Text.StringBuilder();
    private void ProcessStreamedData(string chunk)
    {
        // Append the new data chunk to our buffer
        jsonBuffer.Append(chunk);

        // Continuously try to find and process complete JSON objects in the buffer
        while (true)
        {
            string bufferString = jsonBuffer.ToString();
            int startIndex = bufferString.IndexOf('{');
            if (startIndex == -1)
            {
                // No start of a JSON object found, we're done for now.
                break;
            }

            // We found a starting brace. Now we need to find its matching closing brace.
            int braceCount = 0;
            int endIndex = -1;
            for (int i = startIndex; i < bufferString.Length; i++)
            {
                if (bufferString[i] == '{')
                {
                    braceCount++;
                }
                else if (bufferString[i] == '}')
                {
                    braceCount--;
                }

                if (braceCount == 0)
                {
                    // We've found the matching closing brace
                    endIndex = i;
                    break;
                }
            }

            if (endIndex == -1)
            {
                // We have an incomplete JSON object in the buffer.
                // Wait for more data to arrive.
                break;
            }

            // We have a complete JSON object from startIndex to endIndex.
            int length = endIndex - startIndex + 1;
            string completeJson = bufferString.Substring(startIndex, length);

            try
            {
                var response = JsonUtility.FromJson<GeminiLiveResponse>(completeJson);
                if (response?.candidates != null && response.candidates.Length > 0 && response.candidates[0].content?.parts != null)
                {
                    string text = response.candidates[0].content.parts[0].text;
                    accumulatedResponse += text;

                    // Fire the event to update the UI
                    OnNPCResponseReceived?.Invoke(accumulatedResponse);
                }
            }
            catch (Exception e)
            {
                // This might happen if the extracted JSON is still malformed for some reason.
                Debug.LogWarning($"Failed to parse a complete JSON object. Error: {e.Message}. Object: {completeJson}");
            }

            // IMPORTANT: Remove the processed JSON object (and any leading whitespace/characters) from the buffer
            jsonBuffer.Remove(0, endIndex + 1);
        }
    }

    #region Audio Utilities

    private float GetMicrophoneVolume()
    {
        if (!Microphone.IsRecording(null)) return 0;

        int sampleWindow = 128;
        int currentPosition = Microphone.GetPosition(null) - (sampleWindow + 1);
        if (currentPosition < 0) return 0;

        float[] data = new float[sampleWindow];
        recordingClip.GetData(data, currentPosition);

        float totalLoudness = 0;
        foreach (var sample in data)
        {
            totalLoudness += Mathf.Abs(sample);
        }
        return totalLoudness / sampleWindow;
    }

    // --- WAV Encoding ---
    private byte[] EncodeToWav(AudioClip clip)
    {
        using (var memoryStream = new MemoryStream())
        {
            // WAV header
            memoryStream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(0), 0, 4); // Placeholder for file size
            memoryStream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(16), 0, 4); // Sub-chunk size (16 for PCM)
            memoryStream.Write(BitConverter.GetBytes((ushort)1), 0, 2); // Audio format (1 for PCM)
            memoryStream.Write(BitConverter.GetBytes((ushort)clip.channels), 0, 2);
            memoryStream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4); // Byte rate
            memoryStream.Write(BitConverter.GetBytes((ushort)(clip.channels * 2)), 0, 2); // Block align
            memoryStream.Write(BitConverter.GetBytes((ushort)16), 0, 2); // Bits per sample
            memoryStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(0), 0, 4); // Placeholder for data size

            // Audio data
            float[] floatData = new float[clip.samples * clip.channels];
            clip.GetData(floatData, 0);

            short[] intData = new short[floatData.Length];
            byte[] byteData = new byte[floatData.Length * 2];

            for (int i = 0; i < floatData.Length; i++)
            {
                intData[i] = (short)(floatData[i] * 32767);
            }
            Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);
            memoryStream.Write(byteData, 0, byteData.Length);

            // Update placeholders
            long fileSize = memoryStream.Length;
            memoryStream.Seek(4, SeekOrigin.Begin);
            memoryStream.Write(BitConverter.GetBytes((int)(fileSize - 8)), 0, 4);
            memoryStream.Seek(40, SeekOrigin.Begin);
            memoryStream.Write(BitConverter.GetBytes((int)(fileSize - 44)), 0, 4);

            return memoryStream.ToArray();
        }
    }
    #endregion

    private void OnDestroy()
    {
        EndConversation();
    }
}