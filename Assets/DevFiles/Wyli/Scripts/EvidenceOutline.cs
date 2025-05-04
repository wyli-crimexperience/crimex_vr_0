using UnityEngine;



public class EvidenceOutline : MonoBehaviour {

    private RenderTexture splatMap, tempRT;
    private Material currentMaterial, drawMaterial;
    private RaycastHit hit;
    private Vector2 hitPoint;



    private void Start() {
        drawMaterial = new Material(ManagerGlobal.Instance.HolderData.DrawShader);
        drawMaterial.SetVector("_Color", Color.red);

        currentMaterial = GetComponent<MeshRenderer>().material;

        splatMap = new RenderTexture(currentMaterial.GetTexture("_MainTex").width, currentMaterial.GetTexture("_MainTex").height, 0, RenderTextureFormat.ARGBFloat);
        currentMaterial.SetTexture("_SplatMap", splatMap);
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Chalk") && Physics.Raycast(transform.position, (other.transform.position - transform.position).normalized, out hit)) {

            hitPoint = transform.InverseTransformPoint(hit.point);
            hitPoint = (hitPoint + Vector2.one) * 0.5f;

            drawMaterial.SetVector("_Coordinate", new Vector4(1 - hitPoint.x, hitPoint.y, 0, 0));
            drawMaterial.SetFloat("_Strength", 1);
            drawMaterial.SetFloat("_Size", 1f / Mathf.Sqrt(transform.localScale.x * transform.localScale.y));

            tempRT = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(splatMap, tempRT);
            Graphics.Blit(tempRT, splatMap, drawMaterial);
            RenderTexture.ReleaseTemporary(tempRT);

        }
    }

}