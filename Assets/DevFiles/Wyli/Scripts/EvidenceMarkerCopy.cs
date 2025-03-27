using UnityEngine;

using TMPro;



public enum TypeEvidenceMarker {
    Item,
    Body
}
public class EvidenceMarkerCopy : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI txtLabel;

    private TypeEvidenceMarker typeEvidenceMarker;
    public TypeEvidenceMarker TypeEvidenceMarker => typeEvidenceMarker;
    private int index;
    public int Index => index;



    public void Init(TypeEvidenceMarker _typeEvidenceMarker, int _index, Transform evidenceMarker) {
        typeEvidenceMarker = _typeEvidenceMarker;
        index = _index;
        transform.SetPositionAndRotation(evidenceMarker.position, evidenceMarker.rotation);

        txtLabel.text = typeEvidenceMarker == TypeEvidenceMarker.Item ? (index + 1).ToString() : StaticUtils.ConvertToLetter(index);
    }
    public void Remove() {
        ManagerGlobal.Instance.RemoveEvidenceMarker(this);
    }

}