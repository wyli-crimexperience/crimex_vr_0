// DialogueManager.cs (Refactored)
using UnityEngine;
using TMPro;

// DIALOGUE MANAGER
// Handles the display and flow of dialogues between the player and interactable objects (Witnesses and Phones).
// It manages starting, progressing, and stopping conversations based on player proximity and interaction.
// The script activates a dialogue UI, updates dialogue text, and ensures only one conversation occurs at a time.
// When the player moves out of range, or the dialogue ends, it cleans up and notifies the relevant object. (To be Improved)

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject goDialogue;
    [SerializeField] private TextMeshProUGUI txtDialogue;
    [SerializeField] private TextMeshProUGUI txtSpeakerName; // OPTIONAL: Add a UI element for the speaker's name
    private DialogueData currentDialogue;
    private Witness currentWitness; // This can now be used for non-AI witnesses if you have any
    private Phone currentPhone;
    private int dialogueIndex;

    // We will keep this property for internal logic, but the primary check should be the global one.
    public bool IsInDialogue { get; private set; }

    private const float DIST_CONVERSE = 1.5f;
    private Player player;

    public void Init(Player player)
    {
        this.player = player;
        goDialogue.SetActive(false);
    }

    private void Update()
    {
        if (!IsInDialogue) return; // Exit early if not in a dialogue

        if (currentWitness != null && Vector3.Distance(currentWitness.transform.position, player.transform.position) > DIST_CONVERSE)
        {
            StopDialogue();
            currentWitness.DoneConversing();
            currentWitness = null;
        }
        if (currentPhone != null && Vector3.Distance(currentPhone.transform.position, player.transform.position) > DIST_CONVERSE)
        {
            StopDialogue();
            currentPhone.DoneConversing();
            currentPhone = null;
        }
    }

    // This method can still be called by non-AI witnesses or other interactables.
    public void StartConversation(Witness witness)
    {
        if (ManagerGlobal.Instance.IsPlayerEngaged || Vector3.Distance(witness.transform.position, player.transform.position) > DIST_CONVERSE) return;

        ClearConversation();
        currentWitness = witness;
        currentDialogue = witness.DialogueData;
        BeginDialogue();
    }

    public void StartConversation(Phone phone)
    {
        // The primary check is now against the global "engaged" state.
        if (ManagerGlobal.Instance.IsPlayerEngaged || Vector3.Distance(phone.transform.position, player.transform.position) > DIST_CONVERSE) return;

        ClearConversation();
        currentPhone = phone;
        currentDialogue = phone.DialogueData;
        BeginDialogue();
    }

    private void BeginDialogue()
    {
        // Set the global state to true.
        ManagerGlobal.Instance.IsPlayerEngaged = true;
        IsInDialogue = true;

        goDialogue.SetActive(true);
        dialogueIndex = -1;
        NextDialogue();
    }

    public void NextDialogue()
    {
        dialogueIndex++;
        if (currentDialogue != null && dialogueIndex < currentDialogue.Dialogue.Length)
        {
            txtDialogue.text = currentDialogue.Dialogue[dialogueIndex].speakerText;
        }
        else
        {
            StopDialogue();

            if (currentWitness != null)
            {
                currentWitness.DoneConversing();
                currentWitness = null;
            }
            if (currentPhone != null)
            {
                currentPhone.DoneConversing();
                currentPhone = null;
            }
        }
    }

    public void StopDialogue()
    {
        // Set the global state back to false.
        ManagerGlobal.Instance.IsPlayerEngaged = false;
        IsInDialogue = false;

        goDialogue.SetActive(false);
        currentDialogue = null;
    }

    private void ClearConversation()
    {
        currentWitness = null;
        currentPhone = null;
    }


    public void DisplayDynamicLine(string speaker, string text)
    {
        // CHECKPOINT 3: Is the DialogueManager receiving the call and is the UI reference valid?
        if (txtDialogue == null)
        {
            Debug.LogError("<color=red>DialogueManager:</color> txtDialogue is NULL! Assign it in the Inspector.");
            return;
        }
        Debug.Log($"<color=lime>DialogueManager:</color> Displaying line from '{speaker}': '{text}'");

        if (!goDialogue.activeSelf)
        {
            goDialogue.SetActive(true);
        }

        if (txtSpeakerName != null)
        {
            txtSpeakerName.text = speaker;
        }

        txtDialogue.text = text;
    }
    public void HideDynamicDialogue()
    {
        // Only hide it if it's not being used by the pre-scripted system
        if (!IsInDialogue)
        {
            goDialogue.SetActive(false);
        }
    }
}