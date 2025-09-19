// GeminiDialogueUIBridge.cs (Definitive Version with Synchronized Typewriter Effect)
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

[RequireComponent(typeof(GeminiNPC))]
[RequireComponent(typeof(SpeechManager))]
public class GeminiDialogueUIBridge : MonoBehaviour
{
    private GeminiNPC geminiNpc;
    private SpeechManager speechManager;

    [Header("Typewriter Effect Settings")]
    [Tooltip("Adjusts the overall speed...")]
    [SerializeField] private float speedMultiplier = 1.0f;

    [Tooltip("The FASTEST a character can appear (in seconds). Prevents unreadable speeds.")]
    [SerializeField] private float minCharDelay = 0.02f;

    [Tooltip("The SLOWEST a character can appear (in seconds). Prevents sluggish pacing.")]
    [SerializeField] private float maxCharDelay = 0.1f;

    private Queue<(string sentence, AudioClip clip)> sentenceAudioQueue = new Queue<(string, AudioClip)>();
    private bool isPlayingFromQueue = false;
    private StringBuilder displayedTextBuilder = new StringBuilder();
    private bool isNewResponse = true;

    private void Awake()
    {
        geminiNpc = GetComponent<GeminiNPC>();
        speechManager = GetComponent<SpeechManager>();
        geminiNpc.Initialize(speechManager);
    }

    private void OnEnable()
    {
        geminiNpc.OnSentenceReceived += HandleSentenceReceived;
        geminiNpc.OnConversationEnded += HandleConversationEnded;

        // We will use a new event to control the loading indicator
        geminiNpc.OnWaitingForResponseChanged += HandleWaitingForResponse;
    }

    private void OnDisable()
    {
        if (geminiNpc != null)
        {
            geminiNpc.OnSentenceReceived -= HandleSentenceReceived;
            geminiNpc.OnConversationEnded -= HandleConversationEnded;
            geminiNpc.OnWaitingForResponseChanged -= HandleWaitingForResponse;
        }
    }

    private void OnNewAITurn(string finalFullText)
    {
        isNewResponse = true;
    }
    private void HandleWaitingForResponse(bool isWaiting)
    {
        if (isWaiting)
        {
            // Show the loading indicator and clear previous text
            isNewResponse = true;
            displayedTextBuilder.Clear();
            ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, "", true);
        }
        else
        {
            // Hide the loading indicator
            ManagerGlobal.Instance.DialogueManager.SetLoadingIndicator(false);
        }
    }


    private void HandleSentenceReceived(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence)) return;

        string trimmedSentence = sentence.Trim();

        if (isNewResponse)
        {
            // On the first sentence, we know the full response is starting to arrive,
            // so we can hide the loading indicator.
            ManagerGlobal.Instance.DialogueManager.SetLoadingIndicator(false);

            StopAllCoroutines();
            sentenceAudioQueue.Clear();
            isPlayingFromQueue = false;
            speechManager.Stop();
            isNewResponse = false;
        }

        StartCoroutine(speechManager.Synthesize(trimmedSentence, geminiNpc.Personality,
            (clip) => OnAudioClipReady(trimmedSentence, clip)));
    }

    private void OnAudioClipReady(string sentence, AudioClip clip)
    {
        if (clip != null)
        {
            sentenceAudioQueue.Enqueue((sentence, clip));
            if (!isPlayingFromQueue)
            {
                StartCoroutine(PlayAudioAndTextQueue());
            }
        }
        else
        {
            // If audio fails, just display the text instantly as a fallback.
            UpdateDialoguePanel(sentence, true);
        }
    }

    private IEnumerator PlayAudioAndTextQueue()
    {
        isPlayingFromQueue = true;
        while (sentenceAudioQueue.Count > 0)
        {
            var (sentence, clipToPlay) = sentenceAudioQueue.Dequeue();

            // --- THE CORE SYNCHRONIZATION LOGIC ---

            // 1. Start playing the audio immediately.
            speechManager.PlayClip(clipToPlay);

            // 2. Start the typewriter coroutine to display the text over the audio's duration.
            // We use 'yield return StartCoroutine' to make this main queue wait until the
            // typewriter effect is finished for the current sentence.
            yield return StartCoroutine(TypewriterCoroutine(sentence, clipToPlay.length));

            // 3. The WaitWhile is a safety net in case the typewriter is faster than the audio.
            yield return new WaitWhile(() => speechManager.IsSpeaking());

            // Optional: A tiny buffer to feel more natural between sentences.
            yield return new WaitForSeconds(0.2f);
        }
        isPlayingFromQueue = false;
        HandleSpeechFinished();
    }

    /// <summary>
    /// A new coroutine that reveals text one character at a time.
    /// </summary>
    private IEnumerator TypewriterCoroutine(string text, float audioDuration)
    {

        float baseCharDelay = audioDuration / text.Length;
        float finalCharDelay = baseCharDelay / speedMultiplier;

        finalCharDelay = Mathf.Clamp(finalCharDelay, minCharDelay, maxCharDelay);
        // Add a space if we are appending to existing text.
        // Add a space if we are appending to existing text.
        if (displayedTextBuilder.Length > 0)
        {
            displayedTextBuilder.Append(" ");
        }

        // --- FULL PACING LOGIC ---
        // 1. Calculate the ideal delay to perfectly match the audio duration.
        float idealCharDelay = (audioDuration > 0 && text.Length > 0) ? audioDuration / text.Length : minCharDelay;

        // 2. Apply the artistic speed multiplier.

        // 3. Clamp the result to ensure it's within a comfortable reading speed range.
        finalCharDelay = Mathf.Clamp(finalCharDelay, minCharDelay, maxCharDelay);

        // This is the starting position of the new sentence we're adding.
        int sentenceStartIndex = displayedTextBuilder.Length;

        // Append the new, fully-formed sentence to our master text builder.
        displayedTextBuilder.Append(text);

        // Create a temporary StringBuilder for the rich text display effect.
        StringBuilder displayBuilder = new StringBuilder(displayedTextBuilder.ToString());

        // Loop through each character of the NEW sentence.
        for (int i = 0; i < text.Length; i++)
        {
            int charIndex = sentenceStartIndex + i;

            // Insert the alpha tag right after the character we want to reveal.
            displayBuilder.Insert(charIndex + 1, "<alpha=#00>");

            // Update the UI with the partially visible text.
            ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, displayBuilder.ToString());

            // Wait for our final calculated delay.
            yield return new WaitForSeconds(finalCharDelay);

            // Before the next loop, remove the tag so we can place it again.
            displayBuilder.Remove(charIndex + 1, "<alpha=#00>".Length);
        }

        // Final cleanup: Update the panel one last time to ensure the text is solid and clean.
        ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, displayedTextBuilder.ToString());
    }

    // A simplified update method, as the typewriter now handles complex updates.
    private void UpdateDialoguePanel(string newText, bool append)
    {
        if (append)
        {
            if (displayedTextBuilder.Length > 0) displayedTextBuilder.Append(" ");
            displayedTextBuilder.Append(newText);
        }
        else
        {
            displayedTextBuilder.Clear();
            displayedTextBuilder.Append(newText);
        }
        ManagerGlobal.Instance.DialogueManager.DisplayDynamicLine(gameObject.name, displayedTextBuilder.ToString());
    }

    private void HandleSpeechFinished()
    {
        if (!isPlayingFromQueue)
        {
            speechManager.NotifyQueueEmpty();
            geminiNpc.ResumeListening();
        }
    }

    private void HandleConversationEnded()
    {
        StopAllCoroutines();
        isPlayingFromQueue = false;
        sentenceAudioQueue.Clear();
        displayedTextBuilder.Clear();
        speechManager.Stop();
        isNewResponse = true;
        ManagerGlobal.Instance.DialogueManager.HideDynamicDialogue();
    }
}