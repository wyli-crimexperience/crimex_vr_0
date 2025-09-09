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

    private DialogueData currentDialogue;
    private Witness currentWitness;
    private Phone currentPhone;
    private int dialogueIndex;

    private const float DIST_CONVERSE = 1.5f;
    private Player player;

    public void Init(Player player)
    {
        this.player = player;
        goDialogue.SetActive(false);
    }

    private void Update()
    {
        if (currentWitness != null && Vector3.Distance(currentWitness.transform.position, player.transform.position) > DIST_CONVERSE)
        {
            StopDialogue();
            currentWitness = null;
        }
        if (currentPhone != null && Vector3.Distance(currentPhone.transform.position, player.transform.position) > DIST_CONVERSE)
        {
            StopDialogue();
            currentPhone = null;
        }
    }

    public void StartConversation(Witness witness)
    {
        if (currentWitness != null || Vector3.Distance(witness.transform.position, player.transform.position) > DIST_CONVERSE) return;

        ClearConversation();
        currentWitness = witness;
        currentDialogue = witness.DialogueData;
        BeginDialogue();
    }

    public void StartConversation(Phone phone)
    {
        if (currentPhone != null || Vector3.Distance(phone.transform.position, player.transform.position) > DIST_CONVERSE) return;

        ClearConversation();
        currentPhone = phone;
        currentDialogue = phone.DialogueData;
        BeginDialogue();
    }

    private void BeginDialogue()
    {
        goDialogue.SetActive(true);
        dialogueIndex = -1;
        NextDialogue();
    }

    public void NextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < currentDialogue.Dialogue.Length)
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
        goDialogue.SetActive(false);
        currentDialogue = null;
    }

    private void ClearConversation()
    {
        currentWitness = null;
        currentPhone = null;
    }

    public bool IsInDialogue => currentDialogue != null;
}
