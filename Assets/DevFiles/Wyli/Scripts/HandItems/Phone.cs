using System.Collections.Generic;

using UnityEngine;



public enum TypePhoneContact {
    None,
    DSWD,
    FireMarshall,
    TacticalOperationsCenter,
    UnitDispatchOffice
}
public class Phone : HandItem {

    [SerializeField] private GameObject prefabPhoneButtonContact;
    [SerializeField] private Transform containerContacts;

    private List<PhoneButton> contacts = new List<PhoneButton>();
    private PhoneButton currentContact;



    private void Start() {
        PhoneButton contact;
        for (int i = 1; i < 5; i++) {
            contact = Instantiate(prefabPhoneButtonContact, containerContacts).GetComponent<PhoneButton>();
            contact.Init(this, (TypePhoneContact)i);
            contact.SetSelected(false);
            contacts.Add(contact);
        }
    }



    public void SelectContact(PhoneButton phoneButton) {
        currentContact = phoneButton;
        foreach (PhoneButton contact in contacts) {
            contact.SetSelected(contact == currentContact);
        }
    }
    public void CallContact() {
        if (currentContact == null) { return; }



        if (currentContact.TypePhoneContact == TypePhoneContact.TacticalOperationsCenter) {
            // todo: initiate dialogue
        } else {
            ManagerGlobal.Instance.ShowThought("They might not be the right people to call right now...");
        }
    }

}