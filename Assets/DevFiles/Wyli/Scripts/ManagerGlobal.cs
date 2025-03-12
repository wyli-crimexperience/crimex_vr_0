using System;
using System.Collections;
using System.Collections.Generic;

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

    // ioc part 3,
    ReleaseOfCrimeSceneForm

}
public enum TypeRole {
    None,

    // scene 1
    FirstResponder,
    InvestigatorOnCase,
    SOCOTeamLead,
    Photographer,
    Searcher,
    Measurer,
    Sketcher,
    FingerprintSpecialist,
    Collector,
    EvidenceCustodian,

    // scene 2
}



public class ManagerGlobal : MonoBehaviour {
    public static ManagerGlobal Instance;

    private const float THOUGHT_TIMER_MAX = 3f, DIST_CONVERSE = 1.5f;

    public HolderData HolderData;

    [SerializeField] private NearFarInteractor interactorLeft, interactorRight;
    public NearFarInteractor InteractorLeft => interactorLeft;
    public NearFarInteractor InteractorRight => interactorRight;
    [SerializeField] private IXRFilter_HandToBriefcaseItem ixrFilter_handToBriefcaseItem;

    [SerializeField] private Player player;

    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private Transform containerPoliceTape;
    public Transform ContainerPoliceTape => containerPoliceTape;
    [SerializeField] private Transform handLeftTarget, handRightTarget;
    public Transform HandLeftTarget => handLeftTarget;
    public Transform HandRightTarget => handRightTarget;

    [SerializeField] private GameObject goChangeRole, goThought, goDialogue;
    [SerializeField] private TextMeshProUGUI txtThought, txtDialogue;

    [SerializeField] private GameObject prefabRole;
    [SerializeField] private Transform containerRoles;

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

    private List<ListItemRole> listItemRoles = new List<ListItemRole>();
    private int indexRole;



    private void Awake() {
        Instance = this;

        HolderData.PrimaryButtonLeft.action.performed += PrimaryButtonLeft;
        HolderData.SecondaryButtonLeft.action.performed += SecondaryButtonLeft;
        HolderData.PinchLeft.action.performed += PinchLeft;
        HolderData.ThumbstickLeft.action.performed += ThumbstickLeft;
        HolderData.PrimaryButtonRight.action.performed += PrimaryButtonRight;
        HolderData.SecondaryButtonRight.action.performed += SecondaryButtonRight;
        HolderData.PinchRight.action.performed += PinchRight;
        HolderData.ThumbstickRight.action.performed += ThumbstickRight;

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

        // roles ui
        ListItemRole listItemRole;
        foreach (TypeRole typeRole in Enum.GetValues(typeof(TypeRole))) {
            if (typeRole == TypeRole.None) { continue; }

            listItemRole = Instantiate(prefabRole, containerRoles).GetComponent<ListItemRole>();
            listItemRole.Init(typeRole);
            listItemRole.SetSelected(typeRole == player.TypeRole);
            listItemRoles.Add(listItemRole);
        }
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
        HolderData.PrimaryButtonLeft.action.performed -= PrimaryButtonLeft;
        HolderData.SecondaryButtonLeft.action.performed -= SecondaryButtonLeft;
        HolderData.PinchLeft.action.performed -= PinchLeft;
        HolderData.ThumbstickLeft.action.performed -= ThumbstickLeft;
        HolderData.PrimaryButtonRight.action.performed -= PrimaryButtonRight;
        HolderData.SecondaryButtonRight.action.performed -= SecondaryButtonRight;
        HolderData.PinchRight.action.performed -= PinchRight;
        HolderData.ThumbstickRight.action.performed -= ThumbstickRight;
    }
    private void Update() {
        //ManagerGlobal.Instance.ShowThought($"{TypeItemLeft} / {TypeItemRight}");

        if (currentWitness != null && Vector3.Distance(currentWitness.transform.position, player.transform.position) > DIST_CONVERSE) {
            StopDialogue();
            currentWitness = null;
        }
        if (currentPhone != null && Vector3.Distance(currentPhone.transform.position, player.transform.position) > DIST_CONVERSE) {
            StopDialogue();
            currentPhone = null;
        }
    }

    private void PrimaryButtonLeft(InputAction.CallbackContext context) {
        PrimaryButton();
    }
    private void PrimaryButtonRight(InputAction.CallbackContext context) {
        PrimaryButton();
    }
    private void PrimaryButton() {
        if (currentDialogue == null) {
            goThought.SetActive(false);
            ToggleMenuChangeRole(!goChangeRole.activeSelf);
        } else {
            NextDialogue();
        }
    }
    private void ToggleMenuChangeRole(bool b) {
        goChangeRole.SetActive(b);
        if (b) {
            // todo: pause whole game
            Time.timeScale = b ? 0 : 1;
        } else {
            // todo: change roles
            print($"changing role to {listItemRoles[indexRole].TypeRole}");
        }
    }
    private void SecondaryButtonLeft(InputAction.CallbackContext context) {
        // todo: add a function here
    }
    private void SecondaryButtonRight(InputAction.CallbackContext context) {
        // todo: add a function here
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
    private void ThumbstickLeft(InputAction.CallbackContext context) {
        Thumbstick(context.ReadValue<Vector2>());
    }
    private void ThumbstickRight(InputAction.CallbackContext context) {
        Thumbstick(context.ReadValue<Vector2>());
    }
    private void Thumbstick(Vector2 vector2) {
        if (Mathf.Abs(vector2.x) > Mathf.Abs(vector2.y)) {
            // horizontal input
        } else if (Mathf.Abs(vector2.y) > Mathf.Abs(vector2.x)) {
            // vertical input
            if (goChangeRole.activeInHierarchy) {
                // scroll through roles
                if (vector2.y > 0) {
                    indexRole += 1;
                } else if (vector2.y < 0) {
                    indexRole -= 1;
                }
                for (int i = 0; i < listItemRoles.Count; i++) {
                    listItemRoles[i].SetSelected(i == indexRole);
                }
            }
        }
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
        if (currentWitness != null || Vector3.Distance(witness.transform.position, player.transform.position) > DIST_CONVERSE) { return; }



        ClearCurrentConversation();
        currentWitness = witness;
        currentDialogue = witness.DialogueData;

        StartConservation();
    }
    public void StartConversation(Phone phone) {
        if (currentPhone != null || Vector3.Distance(phone.transform.position, player.transform.position) > DIST_CONVERSE) { return; }



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

    // string formatting
    public string GetRoleName(TypeRole typeRole) {
        switch (typeRole) {
            case TypeRole.FirstResponder: return "First Responder";
            case TypeRole.InvestigatorOnCase: return "Investigator-On-Case";
            case TypeRole.SOCOTeamLead: return "SOCO Team Lead";
            case TypeRole.Photographer: return "Photographer";
            case TypeRole.Searcher: return "Searcher";
            case TypeRole.Measurer: return "Measurer";
            case TypeRole.Sketcher: return "Sketcher";
            case TypeRole.FingerprintSpecialist: return "Fingerprint Specialist";
            case TypeRole.Collector: return "Collector";
            case TypeRole.EvidenceCustodian: return "Evidence Custodian";
            default: return "";
        }
    }
}