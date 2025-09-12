using UnityEngine;
using TMPro;

public class Witness : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    public DialogueData DialogueData => dialogueData;
    public float InteractionDistance = 2.0f;
    private Transform playerTransform;

    private bool isDoneConversing;
    private bool isPlayerLookingAt; // New flag to track gaze state
    private float gazeTime = 0f; // New variable to track how long player has been looking
    private const float GAZE_THRESHOLD = 0.5f; // Time in seconds to confirm gaze

    // New UI Element for the prompt
    [SerializeField] private GameObject talkPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    // Public method to be called by ManagerGlobal
    public void AttemptConversation()
    {
        if (isDoneConversing)
        {
            ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject,
                "I've already talked to them...");
        }
        else
        {
            ManagerGlobal.Instance.DialogueManager.StartConversation(this);
        }
    }

    public void DoneConversing()
    {
        isDoneConversing = true;
    }

    private void Start()
    {
        // Find the player's head transform for both proximity and gaze checks
        playerTransform = ManagerGlobal.Instance.VRRigManager.VRTargetHead.transform;

        // Ensure the UI is initially inactive
        if (talkPromptUI != null)
        {
            talkPromptUI.SetActive(false);
            if (promptText != null)
            {
                promptText.text = $"Press X/A to Talk to {this.gameObject.name}";
            }
        }
    }

    private void Update()
    {
        // Don't do any checks if we're already in a conversation.
        if (ManagerGlobal.Instance.DialogueManager.IsInDialogue)
        {
            // If this object was the previous interactable, clear it now.
            if (ManagerGlobal.Instance.CurrentInteractable == this.gameObject)
            {
                ManagerGlobal.Instance.ClearCurrentInteractable();
                talkPromptUI?.SetActive(false);
            }
            return;
        }

        // Check if the player is within interaction distance
        bool isCloseEnough = Vector3.Distance(transform.position, playerTransform.position) <= InteractionDistance;

        // Perform a raycast from the player's head to check for gaze
        RaycastHit hit;
        bool isGazingAt = Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, InteractionDistance) && hit.collider.gameObject == this.gameObject;

        if (isCloseEnough && isGazingAt)
        {
            // If both conditions are met, track gaze time
            gazeTime += Time.deltaTime;
            if (gazeTime >= GAZE_THRESHOLD)
            {
                // If gaze is confirmed, set this as the interactable and show the prompt
                if (ManagerGlobal.Instance.CurrentInteractable != this.gameObject)
                {
                    ManagerGlobal.Instance.SetCurrentInteractable(this.gameObject);
                    talkPromptUI.SetActive(true);
                }
            }
        }
        else
        {
            // If conditions are not met, reset gaze time and clear interactable if needed
            gazeTime = 0f;
            if (ManagerGlobal.Instance.CurrentInteractable == this.gameObject)
            {
                ManagerGlobal.Instance.ClearCurrentInteractable();
                talkPromptUI.SetActive(false);
            }
        }
    }
}