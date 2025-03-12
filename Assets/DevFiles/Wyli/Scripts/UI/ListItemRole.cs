using UnityEngine;
using UnityEngine.UI;

using TMPro;



public class ListItemRole : MonoBehaviour {

    [SerializeField] private Image imgButton;
    [SerializeField] private TextMeshProUGUI txtName;

    private TypeRole typeRole;
    public TypeRole TypeRole => typeRole;



    public void Init(TypeRole _typeRole) {
        typeRole = _typeRole;
        txtName.text = ManagerGlobal.Instance.GetRoleName(typeRole);
    }
    public void SetSelected(bool b) {
        imgButton.color = new Color(1, 1, 1, b ? 1 : 0.5f);
    }

}