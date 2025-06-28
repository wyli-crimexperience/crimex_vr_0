using UnityEngine;



public class EvidenceFingertip : Evidence {

    [SerializeField] private Fingerprint fingerprint;



    private void Awake() {
        fingerprint.SetFingertip();
    }

}