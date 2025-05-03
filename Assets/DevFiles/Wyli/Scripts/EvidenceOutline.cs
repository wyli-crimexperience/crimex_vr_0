using UnityEngine;



public class EvidenceOutline : MonoBehaviour {

    private Texture2D texture;
    private Color[] colors;
    private RaycastHit hit;
    private SpriteRenderer spriteRenderer;
    private float pixelsPerUnit;
    private int w, h;
    private Vector3 drawPos;
    private Vector2Int p, start, end;
    private Vector2 dir, pixel, linePos;
    private float d;

    public int erSize;
    public Vector2Int lastPos;



    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start() {
        Texture2D tex = spriteRenderer.sprite.texture;
        texture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        colors = texture.GetPixels();
        //colors = tex.GetPixels();
        texture.SetPixels(colors);
        texture.Apply();

        pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        spriteRenderer.sprite = Sprite.Create(texture, spriteRenderer.sprite.rect, Vector2.one * 0.5f, pixelsPerUnit);
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Chalk") && Physics.Raycast(transform.position, (other.transform.position - transform.position).normalized, out hit)) {
            ManagerGlobal.Instance.ShowThought(gameObject, hit.point.ToString());

            w = texture.width;
            h = texture.height;

            drawPos = hit.point - hit.collider.bounds.min;
            drawPos.x *= w / hit.collider.bounds.size.x;
            drawPos.y *= h / hit.collider.bounds.size.y;

            p = new Vector2Int((int)drawPos.x, (int)drawPos.y);
            start = new Vector2Int(Mathf.Clamp(Mathf.Min(p.x, lastPos.x) - erSize, 0, w), Mathf.Clamp(Mathf.Min(p.y, lastPos.y) - erSize, 0, h));
            end = new Vector2Int(Mathf.Clamp(Mathf.Max(p.x, lastPos.x) + erSize, 0, w), Mathf.Clamp(Mathf.Max(p.y, lastPos.y) + erSize, 0, h));
            dir = p - lastPos;

            for (int x = start.x; x < end.x; x++) {
                for (int y = start.y; y < end.y; y++) {

                    pixel = new Vector2(x, y);
                    linePos = p;

                    d = Vector2.Dot(pixel - lastPos, dir) / dir.sqrMagnitude;
                    d = Mathf.Clamp01(d);
                    linePos = Vector2.Lerp(lastPos, p, d);

                    if ((pixel - linePos).sqrMagnitude <= erSize * erSize) {
                        colors[x + y * w] = Color.clear;
                    }

                }
            }

            lastPos = p;
            texture.SetPixels(colors);
            texture.Apply();
            spriteRenderer.sprite = Sprite.Create(texture, spriteRenderer.sprite.rect, Vector2.one * 0.5f, pixelsPerUnit);
        }
    }

}