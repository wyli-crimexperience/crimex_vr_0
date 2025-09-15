// Phone.cs
using System.Collections.Generic;
using UnityEngine;


public class Phone : HandItemBriefcase
{
    [SerializeField] private GameObject prefabPhoneButtonContact;
    [SerializeField] private Transform containerContacts;
    [SerializeField] private TypePhoneContact correctContact;
    [SerializeField] private DialogueData dialogueData;
    public DialogueData DialogueData => dialogueData;

    private List<PhoneButton> contacts = new List<PhoneButton>();
    private PhoneButton currentContact;
    private bool isDoneConversing;

    private void Start()
    {
        List<TypePhoneContact> bagContacts = new List<TypePhoneContact>();
        for (int i = 1; i < System.Enum.GetValues(typeof(TypePhoneContact)).Length; i++)
        {
            bagContacts.Add((TypePhoneContact)i);
        }
        bagContacts.Remove(correctContact);

        // add correct contact
        PhoneButton contact;
        contact = Instantiate(prefabPhoneButtonContact, containerContacts).GetComponent<PhoneButton>();
        contact.Init(this, correctContact);
        contact.SetSelected(false);
        contacts.Add(contact);

        // fill in other contacts
        for (int i = 0; i < 3; i++)
        {
            contact = Instantiate(prefabPhoneButtonContact, containerContacts).GetComponent<PhoneButton>();
            contact.Init(this, bagContacts[Random.Range(0, bagContacts.Count - 1)]);
            contact.SetSelected(false);
            contacts.Add(contact);

            bagContacts.Remove(contact.TypePhoneContact);
        }

        // randomize order
        foreach (PhoneButton pb in contacts)
        {
            pb.transform.SetSiblingIndex(Random.Range(0, contacts.Count - 1));
        }
    }
    public void SelectContact(PhoneButton phoneButton)
    {
        currentContact = phoneButton;
        foreach (PhoneButton contact in contacts)
        {
            contact.SetSelected(contact == currentContact);
        }

        // When a contact is selected, signal to ManagerGlobal that this phone is now ready for interaction.
        if (currentContact != null)
        {
            ManagerGlobal.Instance.SetCurrentInteractable(this.gameObject);
        }
        else
        {
            ManagerGlobal.Instance.ClearCurrentInteractable();
        }
    }

    // CallContact() will no longer directly start the conversation.
    // Instead, it can trigger some UI feedback, but the conversation starts via a primary button press.
    public void CallContact()
    {
        if (currentContact == null) return;

        // The logic for attempting conversation is now handled by the ManagerGlobal
        // via the primary button press.
        // This method can be used for UI feedback, like a dialing animation.
    }

    // This method is called by ManagerGlobal.HandlePrimaryButton()
    public void AttemptConversation()
    {
        if (currentContact == null) return;

        if (currentContact.TypePhoneContact == correctContact)
        {
            if (isDoneConversing)
            {
                ManagerGlobal.Instance.ThoughtManager.ShowThought(currentContact.gameObject,
                    "I've already talked to them...");
            }
            else
            {
                ManagerGlobal.Instance.DialogueManager.StartConversation(this);
            }
        }
        else
        {
            ManagerGlobal.Instance.ThoughtManager.ShowThought(currentContact.gameObject,
                "They might not be the right people to call right now...");
            // The dialogue manager will stop the conversation automatically after the thought.
        }
    }

    public void DoneConversing()
    {
        isDoneConversing = true;
        if (correctContact == TypePhoneContact.TacticalOperationsCenter)
        {
            var timeline = ManagerGlobal.Instance.TimelineManager;
            var dateTimeIncident = timeline.GetEventTime(TimelineEvent.CalledTOC).Value;
        }
    }
}