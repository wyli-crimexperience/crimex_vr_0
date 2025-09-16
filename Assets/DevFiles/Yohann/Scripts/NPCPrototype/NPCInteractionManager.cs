// NPCInteractionManager.cs
using UnityEngine;

public class NPCInteractionManager : MonoBehaviour
{
    private InputManager inputManager;

    void Start()
    {
        // Get the instance of the InputManager
        inputManager = ManagerGlobal.Instance.InputManager;

        // Subscribe to the secondary button press events
        if (inputManager != null)
        {
            inputManager.OnSecondaryButtonLeftPressed += HandleInteractionAttempt;
            inputManager.OnSecondaryButtonRightPressed += HandleInteractionAttempt;
        }
        else
        {
            Debug.LogError("NPCInteractionManager could not find the InputManager!");
        }
    }

    private void HandleInteractionAttempt()
    {
        // Check if the player is currently looking at an interactable object
        GameObject currentInteractable = ManagerGlobal.Instance.CurrentInteractable;
        if (currentInteractable != null)
        {
            // First, try to get the new AI witness component
            if (currentInteractable.TryGetComponent<WitnessTest>(out var witnessTest))
            {
                // If it is, attempt a conversation with the AI
                witnessTest.AttemptConversation();
            }
            // If that fails, check for the original witness component (for non-AI NPCs)
            else if (currentInteractable.TryGetComponent<Witness>(out var witness))
            {
                // If it is, attempt a conversation
                witness.AttemptConversation();
            }
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events to prevent memory leaks
        if (inputManager != null)
        {
            inputManager.OnSecondaryButtonLeftPressed -= HandleInteractionAttempt;
            inputManager.OnSecondaryButtonRightPressed -= HandleInteractionAttempt;
        }
    }
}