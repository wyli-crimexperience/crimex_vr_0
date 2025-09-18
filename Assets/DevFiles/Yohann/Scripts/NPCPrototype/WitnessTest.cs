// WitnessTest.cs (Revised to use ThoughtManager for Post-AI Feedback)
using UnityEngine;
using TMPro;

[RequireComponent(typeof(GeminiNPC))]
public class WitnessTest : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float InteractionDistance = 2.0f;

    [Header("Post-AI Feedback")]
    [Tooltip("The thought bubble text to show after the AI conversation has concluded.")]
    [SerializeField] private string postConversationThought = "I've already talked to them.";

    private Transform playerTransform;
    private GeminiNPC geminiNpc;

    // --- STATE MANAGEMENT ---
    private bool hasConcludedAIConversation = false;

    [Header("UI References")]
    [SerializeField] private GameObject talkPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        geminiNpc = GetComponent<GeminiNPC>();
    }

    private void OnEnable()
    {
        geminiNpc.OnConversationConcludedByAI += HandleAIConversationConcluded;
    }

    private void OnDisable()
    {
        geminiNpc.OnConversationConcludedByAI -= HandleAIConversationConcluded;
    }

    // This is called by the GeminiNPC event when the AI sends [END_CONVERSATION]
    private void HandleAIConversationConcluded()
    {
        hasConcludedAIConversation = true;
        Debug.Log($"<color=cyan>{gameObject.name}:</color> AI conversation has been marked as concluded.");
    }

    // This is the primary interaction method called by input
    public void AttemptConversation()
    {
        // --- REVISED LOGIC ---
        // If the AI conversation has finished, show the thought bubble instead.
        if (hasConcludedAIConversation)
        {
            Debug.Log("AI conversation already concluded. Showing thought bubble.");
            // We use the player's GameObject as the "sender" of the thought.
            ManagerGlobal.Instance.ThoughtManager.ShowThought(ManagerGlobal.Instance.CurrentPlayer.gameObject, postConversationThought);
        }
        // Otherwise, handle the normal AI conversation toggle.
        else
        {
            if (geminiNpc.IsConversationActive)
            {
                geminiNpc.EndConversation();
            }
            else
            {
                geminiNpc.StartConversation();
            }
        }
    }

    private void Start()
    {
        // Use VRRigManager.VRTargetHead for accurate head position
        if (ManagerGlobal.Instance.VRRigManager != null && ManagerGlobal.Instance.VRRigManager.VRTargetHead != null)
        {
            playerTransform = ManagerGlobal.Instance.VRRigManager.VRTargetHead.transform;
        }
        else // Fallback for non-VR or if rig isn't set up yet
        {
            Debug.LogWarning("WitnessTest could not find VRRigManager, falling back to main camera.");
        }

        if (talkPromptUI != null)
        {
            talkPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Gaze and Prompt Logic (remains mostly the same)
        bool isCloseEnough = Vector3.Distance(transform.position, playerTransform.position) <= InteractionDistance;
        RaycastHit hit;
        bool isGazingAt = Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, InteractionDistance) && hit.collider.gameObject == this.gameObject;

        if (ManagerGlobal.Instance.IsPlayerEngaged && !geminiNpc.IsConversationActive)
        {
            if (talkPromptUI.activeSelf) talkPromptUI.SetActive(false);
            if (ManagerGlobal.Instance.CurrentInteractable == gameObject) ManagerGlobal.Instance.ClearCurrentInteractable();
            return;
        }

        if (isCloseEnough && isGazingAt)
        {
            ManagerGlobal.Instance.SetCurrentInteractable(this.gameObject);
            talkPromptUI.SetActive(true);

            // Update prompt text based on the current state
            if (hasConcludedAIConversation)
            {
                // We change the prompt to reflect a generic "interact" rather than "interview"
                promptText.text = $"Interact with {this.gameObject.name}";
            }
            else
            {
                promptText.text = geminiNpc.IsConversationActive ? $"Press X/A to End Interview" : $"Press X/A to Interview {this.gameObject.name}";
            }
        }
        else
        {
            if (ManagerGlobal.Instance.CurrentInteractable == this.gameObject)
            {
                ManagerGlobal.Instance.ClearCurrentInteractable();
                talkPromptUI.SetActive(false);
            }

            if (geminiNpc.IsConversationActive && !isCloseEnough)
            {
                geminiNpc.EndConversation();
            }
        }
    }
}