using System;
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
    PoliceTapeRoll,
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

    public HolderData HolderData;

    [SerializeField] private InputActionReference primaryButtonLeft, secondaryButtonLeft, primaryButtonRight, secondaryButtonRight, pinchLeft, pinchRight;
    [SerializeField] private Notepad notepad;
    [SerializeField] private List<HandItem> handItemsLeft = new List<HandItem>(), handItemsRight = new List<HandItem>();
    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private Transform containerPoliceTape;
    [SerializeField] private Transform handLeft;

    [SerializeField] private GameObject prefabPoliceTape;

    private int handItemIndexLeft, handItemIndexRight;
    private float thoughtTimer = 0;
    private Coroutine corThoughtTimer;

    private DateTime timeOfArrival;
    private bool canWriteNotepad, hasCheckedTimeOfArrival, hasCheckedPulse, hasWrittenTimeOfArrival, hasWrittenPulse;
    private int pulse;

    private PoliceTape policeTapeCurrent;
    private Vector3 posPoliceTapeStart, tapeBetween, tapeScale, tapeRot;
    private float tapeDist;



    private void Awake() {
        Instance = this;

        primaryButtonLeft.action.performed += PrimaryButtonLeft;
        secondaryButtonLeft.action.performed += SecondaryButtonLeft;
        primaryButtonRight.action.performed += PrimaryButtonRight;
        secondaryButtonRight.action.performed += SecondaryButtonRight;
        pinchLeft.action.performed += PinchLeft;
        pinchRight.action.performed += PinchRight;

        handItemsLeft.Insert(0, null);
        handItemsRight.Insert(0, null);

        UpdateHandItemIndex(0, 0);
        UpdateHandItemIndex(1, 0);
        timeOfArrival = DateTime.MinValue;
        canWriteNotepad = false;
        hasCheckedTimeOfArrival = false;
        hasCheckedPulse = false;
        hasWrittenTimeOfArrival = false;
        hasWrittenPulse = false;
        //pulse = UnityEngine.Random.Range(60, 100);
        pulse = 0;

        containerPopupThought.SetActive(false);
    }
    private void OnDestroy() {
        primaryButtonLeft.action.performed -= PrimaryButtonLeft;
        secondaryButtonLeft.action.performed -= SecondaryButtonLeft;
        primaryButtonRight.action.performed -= PrimaryButtonRight;
        secondaryButtonRight.action.performed -= SecondaryButtonRight;
    }
    private void Update() {
        if (policeTapeCurrent != null) {
            tapeBetween = handLeft.position - posPoliceTapeStart;
            policeTapeCurrent.transform.position = posPoliceTapeStart + (tapeBetween * 0.5f);

            tapeDist = tapeBetween.magnitude;
            tapeScale = policeTapeCurrent.transform.localScale;
            tapeScale.z = tapeDist * 0.1f;
            policeTapeCurrent.transform.localScale = tapeScale;

            tapeRot = Quaternion.LookRotation(tapeBetween.normalized).eulerAngles;
            tapeRot.z = 90;
            policeTapeCurrent.transform.eulerAngles = tapeRot;
        }
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
    private void PinchLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            if (GetTypeItemLeft() == TypeItem.PoliceTapeRoll) {
                if (policeTapeCurrent == null) {
                    policeTapeCurrent = Instantiate(prefabPoliceTape, containerPoliceTape).GetComponent<PoliceTape>();
                    posPoliceTapeStart = handLeft.transform.position;
                } else {
                    policeTapeCurrent = null;
                }
            }
        }
    }
    private void PinchRight(InputAction.CallbackContext context) {
        if (context.performed) {
            if (canWriteNotepad && GetTypeItemLeft() == TypeItem.Notepad && GetTypeItemRight() == TypeItem.Pen) {
                while (true) {
                    if (!hasWrittenTimeOfArrival && hasCheckedTimeOfArrival) {
                        notepad.SetTextTime(timeOfArrival.ToString("HH: mm"));
                        hasWrittenTimeOfArrival = true;
                        break;
                    }
                    if (!hasWrittenPulse && hasCheckedPulse) {
                        notepad.SetTextPulse($"{pulse} BPM");
                        hasWrittenPulse = true;
                        break;
                    }
                    break;
                }
            }
        }
    }
    private TypeItem GetTypeItemLeft() {
        if (handItemIndexLeft == 0) return TypeItem.None;
        return handItemsLeft[handItemIndexLeft].TypeItem;
    }
    private TypeItem GetTypeItemRight() {
        if (handItemIndexRight == 0) return TypeItem.None;
        return handItemsRight[handItemIndexRight].TypeItem;
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
    public void CheckWristwatch() {
        if (!hasCheckedTimeOfArrival) {
            timeOfArrival = DateTime.Now;
            hasCheckedTimeOfArrival = true;
        }
        txtThought.text = $"It's {DateTime.Now:hh:mm tt}";

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

    // pulse
    public void CheckPulse() {
        hasCheckedPulse = true;

        txtThought.text = $"Their pulse is {pulse} BPM";

        thoughtTimer = THOUGHT_TIMER_MAX;
        corThoughtTimer ??= StartCoroutine(IE_ShowThought());
    }

    // notepad + pen
    public void SetCanWriteNotepad(bool b) {
        canWriteNotepad = b;
    }
}