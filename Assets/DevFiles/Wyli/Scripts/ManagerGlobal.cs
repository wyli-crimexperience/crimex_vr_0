using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;



public enum TypeItem {
    None,

    // first responder
    Notepad,
    Pen,
    PoliceTape,
    Phone,
    
    // investigator-on-case part 1
    FirstResponderForm,
    
    // soco photographer
    Camera,
    Sketchpad,

    // soco searcher
    EvidenceMarker,

    // soco measurer
    TapeMeasure,
    EvidenceRuler,
    CaseID,

    // soco specialist
    FingerprintBrush,
    FingerprintTape,
    LiftedFingerprint,

    // soco collector
    Chalk,
    SterileSwab,
    EvidencePack,

    // ioc or soco team leader part 2
    ItemOfIdentification,

    // evidence custodian
    EvidenceChecklist,

    // ioc part 3
    ReleaseOfCrimeSceneForm

}
public class ManagerGlobal : MonoBehaviour {
    public static ManagerGlobal Instance;

    private const float THOUGHT_TIMER_MAX = 3f;

    [SerializeField] private InputActionReference primaryButtonLeft, secondaryButtonLeft, primaryButtonRight, secondaryButtonRight;
    [SerializeField] private HandItem handItemNotepad, handItemPen, handItemPoliceTape, handItemPhone;
    [SerializeField] private List<HandItem> handItemsLeft = new List<HandItem>(), handItemsRight = new List<HandItem>();
    [SerializeField] private CanvasGroup cgThought;

    private int handItemIndexLeft, handItemIndexRight;
    private float thoughtTimer = 0;
    private Coroutine corThoughtTimer;



    private void Awake() {
        Instance = this;

        primaryButtonLeft.action.performed += PrimaryButtonLeft;
        secondaryButtonLeft.action.performed += SecondaryButtonLeft;
        primaryButtonRight.action.performed += PrimaryButtonRight;
        secondaryButtonRight.action.performed += SecondaryButtonRight;

        handItemsLeft.Insert(0, null);
        handItemsRight.Insert(0, null);

        UpdateHandItemIndex(0, 0);
        UpdateHandItemIndex(1, 0);

        containerPopupThought.SetActive(false);
    }
    private void OnDestroy() {
        primaryButtonLeft.action.performed -= PrimaryButtonLeft;
        secondaryButtonLeft.action.performed -= SecondaryButtonLeft;
        primaryButtonRight.action.performed -= PrimaryButtonRight;
        secondaryButtonRight.action.performed -= SecondaryButtonRight;
    }
    private void PrimaryButtonLeft(InputAction.CallbackContext context) {
        UpdateHandItemIndex(0, handItemIndexLeft + 1);
    }
    private void SecondaryButtonLeft(InputAction.CallbackContext context) {
        UpdateHandItemIndex(0, handItemIndexLeft - 1);
    }
    private void PrimaryButtonRight(InputAction.CallbackContext context) {
        UpdateHandItemIndex(1, handItemIndexRight + 1);
    }
    private void SecondaryButtonRight(InputAction.CallbackContext context) {
        UpdateHandItemIndex(1, handItemIndexRight - 1);
    }
    private void UpdateHandItemIndex(int typeHand, int index) {

        switch (typeHand) {
            case 0: {
                if (index == -1) { index = handItemsLeft.Count - 1; }
                if (index == handItemsLeft.Count) { index = 0; }
                handItemIndexLeft = index;

                for (int i = 0; i < handItemsLeft.Count; i++) { 
                    if (handItemsLeft[i] != null) {
                        handItemsLeft[i].gameObject.SetActive(i == index);
                    }
                }
            } break;

            case 1: {
                if (index == -1) { index = handItemsRight.Count - 1; }
                if (index == handItemsRight.Count) { index = 0; }
                handItemIndexRight = index;
                    
                for (int i = 0; i < handItemsRight.Count; i++) { 
                    if (handItemsRight[i] != null) {
                        handItemsRight[i].gameObject.SetActive(i == index);
                    }
                }
            } break;

            default: break;
        }
    }



    // thoughts
    [SerializeField] private GameObject containerPopupThought;
    [SerializeField] private TextMeshProUGUI txtThought;

    // wristwatch
    public void GazeWristwatch() {
        txtThought.text = $"It's {System.DateTime.Now:hh:mm tt}";

        thoughtTimer = THOUGHT_TIMER_MAX;
        corThoughtTimer ??= StartCoroutine(IE_ShowThought());
    }
    private IEnumerator IE_ShowThought() {
        containerPopupThought.SetActive(true);

        while (thoughtTimer > 0) {
            cgThought.alpha = Mathf.Lerp(0, 1, thoughtTimer / THOUGHT_TIMER_MAX);

            thoughtTimer -= Time.deltaTime;
            yield return null;
        }

        containerPopupThought.SetActive(false);
        corThoughtTimer = null;
    }
}