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
    FingerprintTapeRoll,
    LiftedFingerprint,

    // soco collector
    Chalk,
    SterileSwab,
    EvidencePack,
    EvidencePackSealTapeRoll,

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
    [SerializeField] private List<HandItem> handItemsLeft = new List<HandItem>(), handItemsRight = new List<HandItem>();
    [SerializeField] private Notepad notepad;
    [SerializeField] private FingerprintTapeRoll fingerprintTapeRoll;
    [SerializeField] private EvidencePackSealTapeRoll evidencePackSealTapeRoll;
    [SerializeField] private EvidencePack evidencePack;
    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private Transform containerPoliceTape;
    [SerializeField] private Transform handLeft;

    [SerializeField] private GameObject prefabPoliceTape, prefabLiftedFingerprint;

    private int handItemIndexLeft, handItemIndexRight;
    private float thoughtTimer = 0;
    private Coroutine corThoughtTimer;

    private DateTime timeOfArrival;
    private bool canWriteNotepad, canWriteEvidencePackSeal, hasCheckedTimeOfArrival, hasCheckedPulse, hasWrittenTimeOfArrival, hasWrittenPulse;
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
            if (GetTypeItemLeft() == TypeItem.FingerprintTapeRoll) {
                if (fingerprintTapeRoll.FingerprintCurrent != null) {
                    SpawnLiftedFingerprint(fingerprintTapeRoll.FingerprintCurrent);
                }
            }
            if (GetTypeItemLeft() == TypeItem.EvidencePack) {
                if (evidencePack.EvidenceCurrent != null) {
                    evidencePack.PackEvidence();
                }
            }
        }
    }
    private void PinchRight(InputAction.CallbackContext context) {
        if (context.performed) {
            if (GetTypeItemRight() == TypeItem.Pen) {
                if (canWriteNotepad && GetTypeItemLeft() == TypeItem.Notepad) {
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
                if (canWriteEvidencePackSeal && GetTypeItemLeft() == TypeItem.EvidencePack) {
                    if (evidencePack.EvidencePackSeal.IsTaped) {
                        evidencePack.EvidencePackSeal.SetMarked(true);
                    }
                }
            }
            if (GetTypeItemLeft() == TypeItem.EvidencePack && GetTypeItemRight() == TypeItem.EvidencePackSealTapeRoll) {
                if (evidencePackSealTapeRoll.EvidencePackSealCurrent != null) {
                    evidencePackSealTapeRoll.EvidencePackSealCurrent.SetTaped(true);
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
    private IEnumerator IE_ShowThought() {
        containerPopupThought.SetActive(true);

        cgThought.alpha = 1;
        yield return new WaitForSeconds(THOUGHT_TIMER_MAX);

        thoughtTimer = THOUGHT_TIMER_MAX;
        while (thoughtTimer > 0) {
            cgThought.alpha = Mathf.Lerp(0, 1, thoughtTimer / THOUGHT_TIMER_MAX);

            thoughtTimer -= Time.deltaTime;
            yield return null;
        }

        containerPopupThought.SetActive(false);
        corThoughtTimer = null;
        txtThought.text = "Hmmm...";
    }
    private void ShowThought(string str) {
        txtThought.text = str;

        corThoughtTimer ??= StartCoroutine(IE_ShowThought());
    }

    // wristwatch
    public void CheckWristwatch() {
        if (!hasCheckedTimeOfArrival) {
            timeOfArrival = DateTime.Now;
            hasCheckedTimeOfArrival = true;
        }
        // todo: datetime.now is not good since each scene has a time setting
        ShowThought($"It's {DateTime.Now:hh:mm tt}");
    }

    // pulse
    public void CheckPulse() {
        hasCheckedPulse = true;

        if (pulse == 0) {
            ShowThought("They have no more pulse...");
        } else {
            ShowThought($"Their pulse is {pulse} BPM");
        }
    }

    // notepad + pen
    public void SetCanWriteNotepad(bool b) {
        canWriteNotepad = b;
    }

    // evidence pack seal + pen
    public void SetCanWriteEvidencePackSeal(bool b) {
        canWriteEvidencePackSeal = b;
    }

    // fingerprint lifting
    private void SpawnLiftedFingerprint(Fingerprint fingerprintSource) {
        LiftedFingerprint liftedFingerprint = Instantiate(prefabLiftedFingerprint, handLeft).GetComponent<LiftedFingerprint>();
        liftedFingerprint.Init(fingerprintSource);
        liftedFingerprint.gameObject.SetActive(false);

        handItemsLeft.Add(liftedFingerprint.HandItem);

        fingerprintSource.Lift();

        ShowThought($"I now have a Lifted Fingerprint Form");
    }

    // witness
    public void ConverseWitness(Witness witness)
    {
        ShowThought($"");
    }
}