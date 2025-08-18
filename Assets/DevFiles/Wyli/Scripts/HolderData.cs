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

    [SerializeField] private Shader drawShader;
    public Shader DrawShader => drawShader;

    [SerializeField] private GameObject prefabPlayerFirstResponder, prefabPlayerInvestigatorOnCase, prefabPlayerSOCOTeamLead, prefabPlayerPhotographer, prefabPlayerSketcher, prefabPlayerSearcher,
        prefabPlayerMeasurer, prefabPlayerFingerprintSpecialist;
    public GameObject GetPrefabPlayer(TypeRole typeRole) {
        return typeRole switch {
            TypeRole.FirstResponder => prefabPlayerFirstResponder,
            TypeRole.InvestigatorOnCase => prefabPlayerInvestigatorOnCase,
            TypeRole.SOCOTeamLead => prefabPlayerSOCOTeamLead,
            TypeRole.Photographer => prefabPlayerPhotographer,
            TypeRole.Sketcher => prefabPlayerSketcher,
            TypeRole.Searcher => prefabPlayerSearcher,
            TypeRole.Measurer => prefabPlayerMeasurer,
            TypeRole.FingerprintSpecialist => prefabPlayerFingerprintSpecialist,

            _ => null,
        };
    }

    [SerializeField] private GameObject prefabListItemRole, prefabCommandPostCopy, prefabFormFirstResponder, prefabFormInvestigatorOnCase, prefabEvidenceMarkerCopy,
        prefabWipe;
    public GameObject PrefabListItemRole => prefabListItemRole;
    public GameObject PrefabCommandPostCopy => prefabCommandPostCopy;
    public GameObject PrefabFormFirstResponder => prefabFormFirstResponder;
    public GameObject PrefabFormInvestigatorOnCase => prefabFormInvestigatorOnCase;
    public GameObject PrefabEvidenceMarkerCopy => prefabEvidenceMarkerCopy;
    public GameObject PrefabWipe => prefabWipe;

    [SerializeField] private Color colBlack, colFluorescent, colGray, colWhite, colMagnetic;
    public Color GetColorOfFingerprintPowderType(TypeFingerprintPowder typeFingerprintPowder) {
        switch (typeFingerprintPowder) {
            case TypeFingerprintPowder.Black: { return colBlack; }
            case TypeFingerprintPowder.Fluorescent: { return colFluorescent; }
            case TypeFingerprintPowder.Gray: { return colGray; }
            case TypeFingerprintPowder.White: { return colWhite; }
            case TypeFingerprintPowder.Magnetic: { return colMagnetic; }
            case TypeFingerprintPowder.Ink: { return colBlack; }

            default: { return Color.green; }
        }
    }

    [SerializeField] private Sprite[] sprsFingerprintRecordStripL, sprsFingerprintRecordStripFilledL, sprsFingerprintRecordStripR, sprsFingerprintRecordStripFilledR;
    public Sprite GetSpriteFingerprintRecordStrip(int leftOrRight, bool isFilled, int index) {
        if (leftOrRight == 0) {
            return isFilled ? sprsFingerprintRecordStripFilledL[index] : sprsFingerprintRecordStripL[index];
        }
        else if (leftOrRight == 1) {
            return isFilled ? sprsFingerprintRecordStripFilledR[index] : sprsFingerprintRecordStripR[index];
        }

        return null;
    }

}
