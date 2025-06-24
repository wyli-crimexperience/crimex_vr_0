using UnityEngine;



public class FingerprintInkingSlab : HandItemBriefcase {

    [SerializeField] private GameObject[] slabStates;

    private int slabState;



    private void Start() {
        SetSlabState(0);
    }



    public void ApplyInk() {
        if (slabState == 0) {
            SetSlabState(1);
        }
    }

    public void SpreadInk() {
        if (slabState == 1) {
            SetSlabState(2);
        }
    }



    private void SetSlabState(int state) {
        slabState = state;
        for (int i = 0; i < slabStates.Length; i++) {
            slabStates[i].SetActive(i == slabState);
        }
    }

}