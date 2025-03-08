using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    private const float THOUGHT_TIMER_MAX = 3f, DIST_CONVERSE = 1.5f;

    public HolderData HolderData;

    [SerializeField] private InputActionReference primaryButtonLeft, secondaryButtonLeft, primaryButtonRight, secondaryButtonRight, pinchLeft, pinchRight;
    [SerializeField] private NearFarInteractor interactorLeft, interactorRight;
    public NearFarInteractor InteractorLeft => interactorLeft;
    public NearFarInteractor InteractorRight => interactorRight;
    [SerializeField] private IXRFilter_HandToBriefcaseItem ixrFilter_handToBriefcaseItem;

    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private Transform containerPoliceTape;
    public Transform ContainerPoliceTape => containerPoliceTape;
    [SerializeField] private Transform player, handLeftTarget, handRightTarget;
    public Transform HandLeftTarget => handLeftTarget;
    public Transform HandRightTarget => handRightTarget;

    [SerializeField] private GameObject goThought, goDialogue;
    [SerializeField] private TextMeshProUGUI txtThought, txtDialogue;

    // hand items
    private HandItem handItemLeft, handItemRight;
    private Notepad notepad;
    private PoliceTapeRoll policeTapeRoll;
    private FingerprintTapeRoll fingerprintTapeRoll;
    private EvidencePackSealTapeRoll evidencePackSealTapeRoll;
    private EvidencePack evidencePack;

    private float thoughtTimer = 0;
    private Coroutine corThoughtTimer;

    private DateTime timeOfArrival;
    private bool canWriteNotepad, canWriteEvidencePackSeal, hasCheckedTimeOfArrival, hasCheckedPulse, hasWrittenTimeOfArrival, hasWrittenPulse;
    private int pulse;

    private Witness currentWitness;
    private Phone currentPhone;
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

        goThought.SetActive(false);
        goDialogue.SetActive(false);
    }
    private void OnEnable() {
        interactorLeft.selectFilters.Add(ixrFilter_handToBriefcaseItem);
        interactorLeft.hoverFilters.Add(ixrFilter_handToBriefcaseItem);

        interactorRight.selectFilters.Add(ixrFilter_handToBriefcaseItem);
        interactorRight.hoverFilters.Add(ixrFilter_handToBriefcaseItem);
    }
    private void OnDisable() {
        interactorLeft.selectFilters.Remove(ixrFilter_handToBriefcaseItem);
        interactorLeft.hoverFilters.Remove(ixrFilter_handToBriefcaseItem);

        interactorRight.selectFilters.Remove(ixrFilter_handToBriefcaseItem);
        interactorRight.hoverFilters.Remove(ixrFilter_handToBriefcaseItem);
    }
    private void OnDestroy() {
        primaryButtonLeft.action.performed -= PrimaryButtonLeft;
        secondaryButtonLeft.action.performed -= SecondaryButtonLeft;
        primaryButtonRight.action.performed -= PrimaryButtonRight;
        secondaryButtonRight.action.performed -= SecondaryButtonRight;
    }
    private void Update() {
        //ManagerGlobal.Instance.ShowThought($"{TypeItemLeft} / {TypeItemRight}");

        if (currentWitness != null && Vector3.Distance(currentWitness.transform.position, player.position) > DIST_CONVERSE) {
            StopDialogue();
            currentWitness = null;
        }
        if (currentPhone != null && Vector3.Distance(currentPhone.transform.position, player.position) > DIST_CONVERSE) {
            StopDialogue();
            currentPhone = null;
        }
    }

    private void PrimaryButtonLeft(InputAction.CallbackContext context) {
        if (currentDialogue != null) {
            NextDialogue();
        }
    }
    private void SecondaryButtonLeft(InputAction.CallbackContext context) {
        // todo: change role to previous
    }
    private void PrimaryButtonRight(InputAction.CallbackContext context) {
        if (currentDialogue != null) {
            NextDialogue();
        }
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
            policeTapeRoll.TriggerTape();
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
    public void GrabItem(HandItem handItem) {
        if (interactorLeft.firstInteractableSelected is XRGrabInteractable interactableLeft && interactableLeft == handItem.Interactable) {
            handItemLeft = handItem;
            AssignGrabbedItem(handItem);
        }
        if (interactorRight.firstInteractableSelected is XRGrabInteractable interactableRight && interactableRight == handItem.Interactable) {
            handItemRight = handItem;
            AssignGrabbedItem(handItem);
        }
    }
    public void ReleaseItem(HandItem handItem) {
        if (handItemLeft == handItem) {
            handItemLeft = null;
            UnassignGrabbedItem(handItem);
        }
        if (handItemRight == handItem) {
            handItemRight = null;
            UnassignGrabbedItem(handItem);
        }
    }
    private void AssignGrabbedItem(HandItem handItem) {
        if (handItem is Notepad _notepad) { notepad = _notepad; }
        else if (handItem is PoliceTapeRoll _policeTapeRoll) { policeTapeRoll = _policeTapeRoll; }
    }
    private void UnassignGrabbedItem(HandItem handItem) {
        if (handItem is Notepad) { notepad = null; }
        else if (handItem is PoliceTapeRoll) { policeTapeRoll = null; }
    }



    // thought
    private IEnumerator IE_ShowThought() {
        goThought.SetActive(true);
        cgThought.alpha = 1;

        // animate in
        float duration = 0f;
        while (duration < 0.5f) {
            goThought.transform.localScale = Vector3.Lerp(Vector3.forward, new Vector3(0.01f, 0.01f, 1), duration / 0.5f);
            goThought.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0, 0.25f, 0.67f), duration / 0.5f);

            duration += Time.deltaTime;
            yield return null;
        }

        // fade out
        yield return new WaitForSeconds(THOUGHT_TIMER_MAX);

        thoughtTimer = THOUGHT_TIMER_MAX;
        while (thoughtTimer > 0) {
            cgThought.alpha = Mathf.Lerp(0, 1, thoughtTimer / THOUGHT_TIMER_MAX);

            thoughtTimer -= Time.deltaTime;
            yield return null;
        }

        // reset
        goThought.SetActive(false);
        txtThought.text = "Hmmm...";
        corThoughtTimer = null;
    }
    public void ShowThought(string str) {
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

    // dialogue
    private void ClearCurrentConversation() {
        currentWitness = null;
        currentPhone = null;
    }
    public void StartConversation(Witness witness) {
        if (currentWitness != null || Vector3.Distance(witness.transform.position, player.position) > DIST_CONVERSE) { return; }



        ClearCurrentConversation();
        currentWitness = witness;
        currentDialogue = witness.DialogueData;

        StartConservation();
    }
    public void StartConversation(Phone phone) {
        if (currentPhone != null || Vector3.Distance(phone.transform.position, player.position) > DIST_CONVERSE) { return; }



        ClearCurrentConversation();
        currentPhone = phone;
        currentDialogue = phone.DialogueData;

        StartConservation();
    }
    private void StartConservation() {
        goDialogue.SetActive(true);
        dialogueIndex = -1;
        NextDialogue();
    }
    private void NextDialogue() {
        dialogueIndex += 1;
        if (dialogueIndex < currentDialogue.Dialogue.Length) {
            txtDialogue.text = currentDialogue.Dialogue[dialogueIndex].speakerText;
        } else {
            StopDialogue();

            if (currentWitness != null) {
                currentWitness.DoneConversing();
                currentWitness = null;
            }
            if (currentPhone != null) {
                currentPhone.DoneConversing();
                currentPhone = null;
            }
        }
    }
    private void StopDialogue() {
        goDialogue.SetActive(false);
        currentDialogue = null;
    }
}