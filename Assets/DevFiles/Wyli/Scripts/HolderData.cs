using UnityEngine;



[CreateAssetMenu(fileName = "HolderData", menuName = "Scriptable Objects/HolderData")]
public class HolderData : ScriptableObject {

    [SerializeField] private Color colVictoriaPureBlue, colMagnetic, colWhite, colBlack;



    public Color GetColorOfFingerprintPowderType(TypeFingerprintPowder typeFingerprintPowder) {
        switch (typeFingerprintPowder) {
            case TypeFingerprintPowder.VictoriaPureBlue: { return colVictoriaPureBlue; }
            case TypeFingerprintPowder.Magnetic: { return colMagnetic; }
            case TypeFingerprintPowder.White: { return colWhite; }
            case TypeFingerprintPowder.Black: { return colBlack; }

            default: { return Color.white; }
        }
    }

}
