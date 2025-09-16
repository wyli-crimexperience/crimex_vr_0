// WitnessTest.cs (Modified to Toggle Conversation)
using UnityEngine;
using TMPro;

[RequireComponent(typeof(GeminiNPC))]
public class WitnessTest : MonoBehaviour
{
    public float InteractionDistance = 2.0f;
    private Transform playerTransform;
    private GeminiNPC geminiNpc;

    private float gazeTime = 0f;
    private const float GAZE_THRESHOLD = 0.5f;

    [SerializeField] private GameObject talkPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        geminiNpc = GetComponent<GeminiNPC>();
    }

    // CHANGED: This method now acts as a TOGGLE
    public void AttemptConversation()
    {
        // If the conversation is already active, this call will end it.
        if (geminiNpc.IsConversationActive)
        {
            geminiNpc.EndConversation();
        }
        // Otherwise, start a new conversation.
        else
        {
            geminiNpc.StartConversation();
        }
    }

    // REMOVED DoneConversing() as it's no longer needed

    private void Start()
    {
        playerTransform = ManagerGlobal.Instance.VRRigManager.VRTargetHead.transform;
        if (talkPromptUI != null)
        {
            talkPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // --- Gaze and Prompt Logic ---
        bool isCloseEnough = Vector3.Distance(transform.position, playerTransform.position) <= InteractionDistance;
        RaycastHit hit;
        bool isGazingAt = Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, InteractionDistance) && hit.collider.gameObject == this.gameObject;

        // Player is engaged with ANOTHER witness, so do nothing.
        if (ManagerGlobal.Instance.IsPlayerEngaged && !geminiNpc.IsConversationActive)
        {
            if (talkPromptUI.activeSelf) talkPromptUI.SetActive(false);
            if (ManagerGlobal.Instance.CurrentInteractable == gameObject) ManagerGlobal.Instance.ClearCurrentInteractable();
            return;
        }

        // --- Handle UI and Interaction State ---
        if (isCloseEnough && isGazingAt)
        {
            gazeTime += Time.deltaTime;
            if (gazeTime >= GAZE_THRESHOLD)
            {
                ManagerGlobal.Instance.SetCurrentInteractable(this.gameObject);
                talkPromptUI.SetActive(true);
                // ADDED: Change prompt text based on conversation state
                promptText.text = geminiNpc.IsConversationActive ? $"Press X/A to End Interview" : $"Press X/A to Interview {this.gameObject.name}";
            }
        }
        else
        {
            gazeTime = 0f;
            if (ManagerGlobal.Instance.CurrentInteractable == this.gameObject)
            {
                ManagerGlobal.Instance.ClearCurrentInteractable();
                talkPromptUI.SetActive(false);
            }

            // ADDED: If the player walks away, automatically end the conversation.
            if (geminiNpc.IsConversationActive && !isCloseEnough)
            {
                Debug.Log("Player walked away. Ending conversation.");
                geminiNpc.EndConversation();
            }
        }
    }
}