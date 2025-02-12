using UnityEngine;



[CreateAssetMenu(fileName = "HolderData", menuName = "Scriptable Objects/HolderData")]
public class HolderData : ScriptableObject {

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
