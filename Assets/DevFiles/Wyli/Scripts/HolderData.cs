using UnityEngine;
using UnityEngine.InputSystem;



[CreateAssetMenu(fileName = "HolderData", menuName = "Scriptable Objects/HolderData")]
public class HolderData : ScriptableObject {

    [SerializeField] private InputActionReference primaryButtonLeft, secondaryButtonLeft, pinchLeft, thumbstickLeft, primaryButtonRight, secondaryButtonRight, pinchRight, thumbstickRight;
    public InputActionReference PrimaryButtonLeft => primaryButtonLeft;
    public InputActionReference SecondaryButtonLeft => secondaryButtonLeft;
    public InputActionReference PinchLeft => pinchLeft;
    public InputActionReference ThumbstickLeft => thumbstickLeft;
    public InputActionReference PrimaryButtonRight => primaryButtonRight;
    public InputActionReference SecondaryButtonRight => secondaryButtonRight;
    public InputActionReference PinchRight => pinchRight;
    public InputActionReference ThumbstickRight => thumbstickRight;

    [SerializeField] private GameObject prefabPlayerFirstResponder, prefabPlayerInvestigatorOnCase, prefabPlayerSOCOTeamLead, prefabPlayerPhotographer, prefabPlayerSketcher, prefabPlayerSearcher,
        prefabPlayerMeasurer;
    public GameObject GetPrefabPlayer(TypeRole typeRole) {
        return typeRole switch {
            TypeRole.FirstResponder => prefabPlayerFirstResponder,
            TypeRole.InvestigatorOnCase => prefabPlayerInvestigatorOnCase,
            TypeRole.SOCOTeamLead => prefabPlayerSOCOTeamLead,
            TypeRole.Photographer => prefabPlayerPhotographer,
            TypeRole.Sketcher => prefabPlayerSketcher,
            TypeRole.Searcher => prefabPlayerSearcher,
            TypeRole.Measurer => prefabPlayerMeasurer,

            _ => null,
        };
    }

    [SerializeField] private GameObject prefabListItemRole, prefabCommandPostCopy, prefabFormFirstResponder, prefabFormInvestigatorOnCase, prefabEvidenceMarkerCopy;
    public GameObject PrefabListItemRole => prefabListItemRole;
    public GameObject PrefabCommandPostCopy => prefabCommandPostCopy;
    public GameObject PrefabFormFirstResponder => prefabFormFirstResponder;
    public GameObject PrefabFormInvestigatorOnCase => prefabFormInvestigatorOnCase;
    public GameObject PrefabEvidenceMarkerCopy => prefabEvidenceMarkerCopy;

    [SerializeField] private Color colBlack, colFluorescent, colGray, colWhite, colMagnetic;
    public Color GetColorOfFingerprintPowderType(TypeFingerprintPowder typeFingerprintPowder) {
        switch (typeFingerprintPowder) {
            case TypeFingerprintPowder.Black: { return colBlack; }
            case TypeFingerprintPowder.Fluorescent: { return colFluorescent; }
            case TypeFingerprintPowder.Gray: { return colGray; }
            case TypeFingerprintPowder.White: { return colWhite; }
            case TypeFingerprintPowder.Magnetic: { return colMagnetic; }

            default: { return Color.white; }
        }
    }

}
