using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using TMPro;
using System.Linq;



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
    [SerializeField] private NearFarInteractor interactorLeft, interactorRight;
    [SerializeField] private Notepad notepad;
    [SerializeField] private Transform policeTapeRoll;
    [SerializeField] private FingerprintTapeRoll fingerprintTapeRoll;
    [SerializeField] private EvidencePackSealTapeRoll evidencePackSealTapeRoll;
    [SerializeField] private EvidencePack evidencePack;
    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private Transform containerPoliceTape;
    [SerializeField] private Transform handLeftTarget, handRightTarget;
    [SerializeField] private GameObject prefabPoliceTape;
    [SerializeField] private GameObject goDialogue;
    [SerializeField] private TextMeshProUGUI txtDialogue;

    private HandItem handItemLeft, handItemRight;

    private float thoughtTimer = 0;
    private Coroutine corThoughtTimer;

    private DateTime timeOfArrival;
    private bool canWriteNotepad, canWriteEvidencePackSeal, hasCheckedTimeOfArrival, hasCheckedPulse, hasWrittenTimeOfArrival, hasWrittenPulse;
    private int pulse;

    private PoliceTape policeTapeCurrent;
    private Vector3 posPoliceTapeStart, tapeBetween, tapeScale, tapeRot;
    private float tapeDist;

    private DialogueData currentDialogue;
    private int dialogueIndex;



    private void Awake() {
        Instance = this;

        primaryButtonLeft.action.performed += PrimaryButtonLeft;
        secondaryButtonLeft.action.performed += SecondaryButtonLeft;
        primaryButtonRight.action.performed += PrimaryButtonRight;
        secondaryButtonRight.action.performed += SecondaryButtonRight;
        pinchLeft.action.performed += PinchLeft;
        pinchRight.action.performed += PinchRight;

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
            tapeBetween = policeTapeRoll.position - posPoliceTapeStart;
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
        // todo: change role to next
    }
    private void SecondaryButtonLeft(InputAction.CallbackContext context) {
        // todo: change role to previous
    }
    private void PrimaryButtonRight(InputAction.CallbackContext context) {
        // todo: change role to next
    }
    private void SecondaryButtonRight(InputAction.CallbackContext context) {
        // todo: change role to previous
    }
    private void PinchLeft(InputAction.CallbackContext context) {
        if (context.performed) { Pinch(TypeItemLeft, TypeItemRight); }
    }
    private void PinchRight(InputAction.CallbackContext context) {
        if (context.performed) { Pinch(TypeItemRight, TypeItemLeft); }
    }
    private void Pinch(TypeItem typeItem1, TypeItem typeItem2) {
        // police tape
        if (typeItem1 == TypeItem.PoliceTapeRoll) {
            if (policeTapeCurrent == null) {
                policeTapeCurrent = Instantiate(prefabPoliceTape, containerPoliceTape).GetComponent<PoliceTape>();
                posPoliceTapeStart = policeTapeRoll.transform.position;
            } else {
                policeTapeCurrent = null;
            }
        }

        // fingerprint tape
        if (typeItem1 == TypeItem.FingerprintTapeRoll) {
            if (fingerprintTapeRoll.FingerprintCurrent != null) {
                LiftFingerprint(fingerprintTapeRoll.FingerprintCurrent);
            }
        }

        // evidence pack
        if (typeItem1 == TypeItem.EvidencePack) {
            if (evidencePack.EvidenceCurrent != null) {
                evidencePack.PackEvidence();
            }
        }

        // pen
        if (typeItem1 == TypeItem.Pen) {
            // pen on notepad
            if (canWriteNotepad && typeItem2 == TypeItem.Notepad) {
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
            // pen on evidence pack
            if (canWriteEvidencePackSeal && typeItem2 == TypeItem.EvidencePack) {
                if (evidencePack.EvidencePackSeal.IsTaped) {
                    evidencePack.EvidencePackSeal.SetMarked(true);
                }
            }
        }

        // evidence pack seal tape on evidence pack
        if (typeItem1 == TypeItem.EvidencePackSealTapeRoll && typeItem2 == TypeItem.EvidencePack) {
            if (evidencePackSealTapeRoll.EvidencePackSealCurrent != null) {
                evidencePackSealTapeRoll.EvidencePackSealCurrent.SetTaped(true);
            }
        }
    }
    private TypeItem TypeItemLeft => handItemLeft == null ? TypeItem.None : handItemLeft.TypeItem;
    private TypeItem TypeItemRight => handItemRight == null ? TypeItem.None : handItemRight.TypeItem;
    public void GrabInteractable(HandItem handItem) {
        if ((XRGrabInteractable)interactorLeft.firstInteractableSelected == handItem.Interactable) {
            handItemLeft = handItem;
        }
        if ((XRGrabInteractable)interactorRight.firstInteractableSelected == handItem.Interactable) {
            handItemRight = handItem;
        }
    }
    public void ReleaseInteractable(HandItem handItem) {
        if ((XRGrabInteractable)interactorLeft.firstInteractableSelected == handItem.Interactable) {
            handItemLeft = null;
        }
        if ((XRGrabInteractable)interactorRight.firstInteractableSelected == handItem.Interactable) {
            handItemRight = null;
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
    private void LiftFingerprint(Fingerprint fingerprintSource) {
        fingerprintTapeRoll.LiftFingerprint(fingerprintSource);
        fingerprintSource.Lift();
    }

    // witness
    public void ConverseWitness(Witness witness) {
        currentDialogue = witness.DialogueData;
        dialogueIndex = -1;

        NextDialogue();
    }
    private void NextDialogue() {
        dialogueIndex += 1;
        if (dialogueIndex < currentDialogue.Dialogue.Length) {
            txtDialogue.text = currentDialogue.Dialogue[dialogueIndex].speakerText;
        } else {
            currentDialogue = null;
        }
    }
}