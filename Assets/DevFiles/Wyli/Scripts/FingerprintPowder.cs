using UnityEngine;



public enum TypeFingerprintPowder {
    None,
    Black,
    Fluorescent,
    Gray,
    White,
    Magnetic
}
public class FingerprintPowder : MonoBehaviour {

    public TypeFingerprintPowder TypeFingerprintPowder;

    //[SerializeField] private MeshRenderer meshRenderer;

    private void Start() {
        //for (int i = 0; i < meshRenderer.materials.Length; i++) {
        //    meshRenderer.materials[i] = new Material(meshRenderer.materials[i]);
        //    meshRenderer.materials[i].color = ManagerGlobal.Instance.HolderData.GetColorOfFingerprintPowderType(TypeFingerprintPowder);
        //}
    }

}