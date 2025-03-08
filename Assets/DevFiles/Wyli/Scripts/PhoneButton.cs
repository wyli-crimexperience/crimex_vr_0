using UnityEngine;
using UnityEngine.UI;

using TMPro;



public enum TypePhoneButton {
    None,
    Contact,
    Call
}
public class PhoneButton : MonoBehaviour {

    [SerializeField] private Phone phone;
    [SerializeField] private Image imgButton; 
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TypePhoneButton typePhoneButton;

    private TypePhoneContact typePhoneContact;
    public TypePhoneContact TypePhoneContact => typePhoneContact;



    private void OnTriggerEnter(Collider collider) {
        if (collider.CompareTag("Fingertip")) {
            switch (typePhoneButton) {
                case TypePhoneButton.Contact: {
                    phone.SelectContact(this);
                } break;

                case TypePhoneButton.Call: {
                    phone.CallContact();
                }
                break;

                default: break;
            }
        }
    }



    public void Init(Phone _phone, TypePhoneContact _typePhoneContact) {
        phone = _phone;

        typePhoneButton = TypePhoneButton.Contact;
        typePhoneContact = _typePhoneContact;

        switch (typePhoneContact) {
            case TypePhoneContact.DSWD: { txtName.text = "DSWD"; } break;
            case TypePhoneContact.FireMarshall: { txtName.text = "Fire Marshall"; } break;
            case TypePhoneContact.TacticalOperationsCenter: { txtName.text = "Tactical Operations Center"; } break;
            case TypePhoneContact.UnitDispatchOffice: { txtName.text = "Unit Dispatch Office"; } break;
            default: { txtName.text = ""; } break;
        }
    }
    public void SetSelected(bool b) {
        imgButton.color = b ? Color.black : Color.white;
    }
}