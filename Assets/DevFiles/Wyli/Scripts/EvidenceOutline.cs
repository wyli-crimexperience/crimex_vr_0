using UnityEngine;



public class EvidenceOutline : MonoBehaviour {

    private MeshRenderer meshRenderer;
    private Texture2D texture;
    private Color[] colors;
    private int w, h;
    private Vector2 mousePos;
    private RaycastHit hit;
    private Ray ray;

    public int erSize;
    public Vector2Int lastPos;



    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    private void Start() {
        Texture2D tex = meshRenderer.sharedMaterial.GetTexture("_MainTex") as Texture2D;
        texture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        colors = texture.GetPixels();
        texture.SetPixels(colors);
        texture.Apply();
        meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
    }

    private void OnCollisionStay(Collision collision) {

        if (collision.collider.CompareTag("Chalk")) {

            ray = new Ray(collision.contacts[0].point - collision.contacts[0].normal, collision.contacts[0].normal);
            if (Physics.Raycast(ray, out hit)) {
                ManagerGlobal.Instance.ShowThought(gameObject, hit.textureCoord.ToString());
            }

            // update texture
            w = texture.width;
            h = texture.height;
            //hitPoint = collision.GetContact(0).point;
            //mousePos = hitPoint - (Vector2)collision.collider.bounds.min;

        }

    }

}