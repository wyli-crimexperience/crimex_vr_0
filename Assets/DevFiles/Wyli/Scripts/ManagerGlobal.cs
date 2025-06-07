using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using TMPro;
using UnityEngine.UI;



public enum TypeItem {
    None,
    Briefcase,

    // first responder
    Notepad,
    Pen,
    PoliceTapeRoll,
    Phone,
    
    // investigator-on-case part 1
    FormFirstResponder,

    // soco team leader
    FormInvestigatorOnCase,
    CommandPost,
    
    // soco photographer
    Camera,

    // soco sketcher
    FormSketcher,

    // soco searcher
    EvidenceMarkerItem,
    EvidenceMarkerBody,
    CaseID,

    // soco measurer
    EvidenceRuler,
    TapeMeasure,

    // soco fingerprint specialist
    Chalk,
    Bowl,
    FingerprintPowderBottle,
    FingerprintBrush,
    FingerprintTapeRoll,
    FingerprintTapeLifted,
    FormLatentFingerprint,

    // soco collector
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
public enum TypeFingerprintBrush {
    Feather,
    Fiber,
    FlatHead,
    Round
}
public enum TypeFingerprintPowder {
    None,
    Black,
    Fluorescent,
    Gray,
    White,
    Magnetic
}
public enum TypeRole {
    None,

    // scene 1
    FirstResponder,
    InvestigatorOnCase,
    SOCOTeamLead,
    Photographer,
    Sketcher,
    Searcher,
    Measurer,
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
    public TypeRole TypeRolePlayer => player == null ? TypeRole.None : player.TypeRole;

    [SerializeField] private CanvasGroup cgThought;
    [SerializeField] private Transform containerPoliceTape;
    public Transform ContainerPoliceTape => containerPoliceTape;
    [SerializeField] private Transform handLeftTarget, handRightTarget;
    public Transform HandLeftTarget => handLeftTarget;
    public Transform HandRightTarget => handRightTarget;
    [SerializeField] private Transform xrInteractionSetup;
    [SerializeField] private Transform vrTargetLeftHand, vrTargetRightHand, vrTargetHead;
    public Transform VRTargetLeftHand => vrTargetLeftHand;
    public Transform VRTargetRightHand => vrTargetRightHand;
    public Transform VRTargetHead => vrTargetHead;

    [SerializeField] private ScrollRect srChangeRole;
    [SerializeField] private GameObject goChangeRole, goThought, goDialogue;
    [SerializeField] private TextMeshProUGUI txtThought, txtDialogue;

    [SerializeField] private Transform containerRoles;

    private GameObject commandPostCopy;

    [SerializeField] private Transform containerEvidenceMarker;
    private EvidenceMarkerCopy _evidenceMarkerCopy;
    private int _evidenceMarkerIndex;
    private List<EvidenceMarkerCopy> listEvidenceMarkerItemCopies = new List<EvidenceMarkerCopy>();
    private List<EvidenceMarkerCopy> listEvidenceMarkerBodyCopies = new List<EvidenceMarkerCopy>();

    // hand items
    private HandItem handItemLeft, handItemRight;
    private Notepad notepad;
    private PoliceTapeRoll policeTapeRoll;
    private Form form;
    private HandItem evidenceMarkerItem, evidenceMarkerBody;
    private FingerprintTapeRoll fingerprintTapeRoll;
    private EvidencePackSealTapeRoll evidencePackSealTapeRoll;
    private EvidencePack evidencePack;

    private float thoughtTimer = 0;
    private Coroutine corThoughtTimer;
    private GameObject thoughtSender;

    // timings
    private DateTime dateTimeIncident;
    public DateTime DateTimeIncident => dateTimeIncident;
    // timings for first responder
    private DateTime dateTimeReported, dateTimeFirstResponderArrived, dateTimeCordoned, dateTimeCalledTOC, dateTimeFirstResponderFilledUp, dateTimeInvestigatorArrived, dateTimeInvestigatorReceived;
    public DateTime DateTimeReported => dateTimeReported;
    public DateTime DateTimeFirstResponderArrived => dateTimeFirstResponderArrived;
    public DateTime DateTimeCordoned => dateTimeCordoned;
    public DateTime DateTimeCalledTOC => dateTimeCalledTOC;
    public void SetDateTimeCalledTOC() {
        dateTimeCalledTOC = StaticUtils.DateTimeNowInEvening(DateTimeIncident);
    }
    public DateTime DateTimeFirstResponderFilledUp => dateTimeFirstResponderFilledUp;
    public DateTime DateTimeInvestigatorArrived => dateTimeInvestigatorArrived;
    public DateTime DateTimeInvestigatorReceived => dateTimeInvestigatorReceived;
    public void SetDateTimeInvestigatorReceived() {
        dateTimeInvestigatorReceived = StaticUtils.DateTimeNowInEvening(DateTimeIncident);
    }
    // timings for investigator on case
    private DateTime dateTimeInvestigatorFilledUp;
    public DateTime DateTimeInvestigatorFilledUp => dateTimeInvestigatorFilledUp;

    // item flags
    private bool canWriteNotepad, canWriteEvidencePackSeal, hasCheckedTimeOfArrival, hasCheckedPulse, hasWrittenTimeOfArrival, hasWrittenPulse, canWriteForm;

    private int pulse;
    private Witness currentWitness;
    private Phone currentPhone;
    private DialogueData currentDialogue;
    private int dialogueIndex;

    private List<ListItemRole> listItemRoles = new List<ListItemRole>();
    private int indexRole;

    private Vector3 playerStartPos;
    private Quaternion playerStartRot;
    private Dictionary<TypeRole, Player> dictPlayers = new Dictionary<TypeRole, Player>();



    private void Awake() {
        Instance = this;

        HolderData.PrimaryButtonLeft.action.performed += PrimaryButtonLeft;
        HolderData.SecondaryButtonLeft.action.performed += SecondaryButtonLeft;
        HolderData.PinchLeft.action.performed += PinchLeft;
        HolderData.ThumbstickLeft.action.started += ThumbstickLeftTap;
        HolderData.PrimaryButtonRight.action.performed += PrimaryButtonRight;
        HolderData.SecondaryButtonRight.action.performed += SecondaryButtonRight;
        HolderData.PinchRight.action.performed += PinchRight;
        HolderData.ThumbstickRight.action.started += ThumbstickRightTap;

        canWriteNotepad = false;
        hasCheckedTimeOfArrival = false;
        hasCheckedPulse = false;
        hasWrittenTimeOfArrival = false;
        hasWrittenPulse = false;
        canWriteForm = false;
        pulse = 0;
        // todo: this is only scene 1. make it adapt
        dateTimeIncident = DateTime.Now.AddHours(-0.5f);
        dateTimeReported = dateTimeIncident.AddHours(0.25f);
        dateTimeFirstResponderArrived = dateTimeReported.AddHours(0.25f);

        goThought.SetActive(false);
        goDialogue.SetActive(false);

        // init player
        if (player != null) {
            playerStartPos = player.transform.position;
            playerStartRot = player.transform.rotation;
            player.Init(player.TypeRole);
            dictPlayers.Add(player.TypeRole, player);
        }

        // roles ui
        ListItemRole listItemRole;
        foreach (TypeRole typeRole in Enum.GetValues(typeof(TypeRole))) {
            if (typeRole == TypeRole.None) { continue; }

            listItemRole = Instantiate(HolderData.PrefabListItemRole, containerRoles).GetComponent<ListItemRole>();
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
        HolderData.ThumbstickLeft.action.started -= ThumbstickLeftTap;
        HolderData.PrimaryButtonRight.action.performed -= PrimaryButtonRight;
        HolderData.SecondaryButtonRight.action.performed -= SecondaryButtonRight;
        HolderData.PinchRight.action.performed -= PinchRight;
        HolderData.ThumbstickRight.action.started -= ThumbstickRightTap;
    }
    private void Update() {
        //ManagerGlobal.Instance.ShowThought($"{srChangeRole.verticalNormalizedPosition}");

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
        Time.timeScale = b ? 0 : 1;
        if (b) {
            // todo: pause whole game
        } else {
            StopCoroutine(IE_ChangeRole());
            StartCoroutine(IE_ChangeRole());
        }
    }
    private IEnumerator IE_ChangeRole() {
        // retain held items
        interactorLeft.allowSelect = false;
        interactorRight.allowSelect = false;
        HandItem handItemLeft = null, handItemRight = null;
        if (interactorLeft.firstInteractableSelected is XRGrabInteractable interactableLeft) {
            handItemLeft = interactableLeft.GetComponent<HandItem>();
        }
        if (interactorRight.firstInteractableSelected is XRGrabInteractable interactableRight) {
            handItemRight = interactableRight.GetComponent<HandItem>();
        }
        if (handItemLeft != null || handItemRight != null) {
            yield return new WaitForEndOfFrame();
            if (handItemLeft != null) {
                handItemLeft.SetPaused(true);
            }
            if (handItemRight != null) {
                handItemRight.SetPaused(true);
            }
        }

        // deactivate current player
        player.SetActive(false);

        // activate new player
        // check if player already exists; if so, just switch to that
        TypeRole typeRole = listItemRoles[indexRole].TypeRole;
        if (dictPlayers.ContainsKey(typeRole)) {
            player = dictPlayers[typeRole];
        } else { // else, spawn a new player and store to dictionary
            player = Instantiate(HolderData.GetPrefabPlayer(typeRole)).GetComponent<Player>();
            player.transform.SetPositionAndRotation(playerStartPos, playerStartRot);
            player.Init(typeRole);
            dictPlayers.Add(typeRole, player);

            if (player.TypeRole == TypeRole.InvestigatorOnCase) {
                dateTimeInvestigatorArrived = StaticUtils.DateTimeNowInEvening(DateTimeIncident);
            }
        }
        player.SetActive(true);

        Vector3 pos = player.transform.position;
        pos.y = playerStartPos.y;
        xrInteractionSetup.SetPositionAndRotation(pos, playerStartRot);

        // re-enable interactors
        interactorLeft.allowSelect = true;
        interactorRight.allowSelect = true;
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
        // pen
        if (typeItem1 == TypeItem.Pen) {
            // pen on notepad
            if (canWriteNotepad && typeItem2 == TypeItem.Notepad) {
                while (true) {
                    if (!hasWrittenTimeOfArrival && hasCheckedTimeOfArrival) {
                        notepad.SetTextTime(dateTimeFirstResponderArrived.ToString("HH: mm"));
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
            // pen on form
            if (canWriteForm && IsForm(typeItem2)) {
                form.WriteOnPage();
            }
            // pen on evidence pack
            if (canWriteEvidencePackSeal && typeItem2 == TypeItem.EvidencePack) {
                if (evidencePack.EvidencePackSeal.IsTaped) {
                    evidencePack.EvidencePackSeal.SetMarked(true);
                }
            }
        }

        // police tape
        if (typeItem1 == TypeItem.PoliceTapeRoll) {
            policeTapeRoll.TriggerTape();

            // todo: set time cordoned to after cordoning, not at the start
            dateTimeCordoned = StaticUtils.DateTimeNowInEvening(DateTimeIncident);
        }

        // command post set-up
        if (typeItem1 == TypeItem.CommandPost) {
            if (commandPostCopy == null) {
                commandPostCopy = Instantiate(HolderData.PrefabCommandPostCopy);
            }

            Vector3 pos = player.transform.position;
            pos.y = 0.486f;
            commandPostCopy.transform.SetPositionAndRotation(pos, Quaternion.Euler(new Vector3(0, -player.transform.eulerAngles.y, 0)));
        }

        // any form
        if (IsForm(typeItem1)) {
            form.TogglePage();
        }

        // evidence marker
        if (typeItem1 == TypeItem.EvidenceMarkerItem) {
            _evidenceMarkerIndex = listEvidenceMarkerItemCopies.Count;
            for (int i = 0; i < listEvidenceMarkerItemCopies.Count; i++) {
                if (listEvidenceMarkerItemCopies[i] == null) {
                    _evidenceMarkerIndex = i;
                    break;
                }
            }

            _evidenceMarkerCopy = Instantiate(HolderData.PrefabEvidenceMarkerCopy, containerEvidenceMarker).GetComponent<EvidenceMarkerCopy>();
            _evidenceMarkerCopy.Init(TypeEvidenceMarker.Item, _evidenceMarkerIndex, evidenceMarkerItem.transform);
            if (_evidenceMarkerIndex == listEvidenceMarkerItemCopies.Count) {
                listEvidenceMarkerItemCopies.Add(_evidenceMarkerCopy);
            } else {
                listEvidenceMarkerItemCopies[_evidenceMarkerIndex] = _evidenceMarkerCopy;
            }
        }

        if (typeItem1 == TypeItem.EvidenceMarkerBody) {
            _evidenceMarkerIndex = listEvidenceMarkerBodyCopies.Count;
            for (int i = 0; i < listEvidenceMarkerBodyCopies.Count; i++) {
                if (listEvidenceMarkerBodyCopies[i] == null) {
                    _evidenceMarkerIndex = i;
                    break;
                }
            }

            _evidenceMarkerCopy = Instantiate(HolderData.PrefabEvidenceMarkerCopy, containerEvidenceMarker).GetComponent<EvidenceMarkerCopy>();
            _evidenceMarkerCopy.Init(TypeEvidenceMarker.Body, _evidenceMarkerIndex, evidenceMarkerBody.transform);
            if (_evidenceMarkerIndex == listEvidenceMarkerBodyCopies.Count) {
                listEvidenceMarkerBodyCopies.Add(_evidenceMarkerCopy);
            } else {
                listEvidenceMarkerBodyCopies[_evidenceMarkerIndex] = _evidenceMarkerCopy;
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

        // evidence pack seal tape on evidence pack
        if (typeItem1 == TypeItem.EvidencePackSealTapeRoll && typeItem2 == TypeItem.EvidencePack) {
            if (evidencePackSealTapeRoll.EvidencePackSealCurrent != null) {
                evidencePackSealTapeRoll.EvidencePackSealCurrent.SetTaped(true);
            }
        }
    }
    private bool IsForm(TypeItem typeItem) => typeItem == TypeItem.FormFirstResponder || typeItem == TypeItem.FormInvestigatorOnCase || typeItem == TypeItem.FormSketcher;
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
        else if (handItem is Form _form) {
            form = _form;
            form.Receive();
        }
        else if (handItem.TypeItem == TypeItem.EvidenceMarkerItem) { evidenceMarkerItem = handItem; }
        else if (handItem.TypeItem == TypeItem.EvidenceMarkerBody) { evidenceMarkerBody = handItem; }
        else if (handItem is PoliceTapeRoll _policeTapeRoll) { policeTapeRoll = _policeTapeRoll; }
    }
    private void UnassignGrabbedItem(HandItem handItem) {
        if (handItem is Notepad) { notepad = null; }
        else if (handItem is Form) { form = null; }
        else if (handItem.TypeItem == TypeItem.EvidenceMarkerItem) { evidenceMarkerItem = null; }
        else if (handItem.TypeItem == TypeItem.EvidenceMarkerBody) { evidenceMarkerBody = null; }
        else if (handItem is PoliceTapeRoll) { policeTapeRoll = null; }
    }
    private void ThumbstickLeftTap(InputAction.CallbackContext context) {
        ThumbstickTap(context.ReadValue<Vector2>());
    }
    private void ThumbstickRightTap(InputAction.CallbackContext context) {
        ThumbstickTap(context.ReadValue<Vector2>());
    }
    private void ThumbstickTap(Vector2 vector2) {
        if (Mathf.Abs(vector2.x) > Mathf.Abs(vector2.y)) {
            // horizontal input
        } else if (Mathf.Abs(vector2.y) > Mathf.Abs(vector2.x)) {
            // vertical input
            // change roles
            if (goChangeRole.activeInHierarchy) {
                // scroll through roles
                if (vector2.y < 0) {
                    indexRole += 1;
                    if (indexRole > listItemRoles.Count - 1) {
                        indexRole = 0;
                    }
                } else if (vector2.y > 0) {
                    indexRole -= 1;
                    if (indexRole < 0) {
                        indexRole = listItemRoles.Count - 1;
                    }
                }

                for (int i = 0; i < listItemRoles.Count; i++) {
                    listItemRoles[i].SetSelected(i == indexRole);
                }
                srChangeRole.verticalNormalizedPosition = Mathf.Clamp01(1 - (2f * indexRole / listItemRoles.Count));
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
        ClearCurrentThought();
    }
    public void ShowThought(GameObject sender, string str) {
        //if (sender == thoughtSender) { return; }



        thoughtSender = sender;

        txtThought.text = str;
        if (corThoughtTimer != null) { StopCoroutine(corThoughtTimer); }
        corThoughtTimer = StartCoroutine(IE_ShowThought());
    }
    private void ClearCurrentThought() {
        if (corThoughtTimer != null) {
            StopCoroutine(corThoughtTimer);
            corThoughtTimer = null;
        }

        goThought.SetActive(false);
        txtThought.text = "Hmmm...";

        thoughtSender = null;
    }

    // wristwatch
    public void CheckWristwatch(GameObject sender) {
        if (!hasCheckedTimeOfArrival) {
            // todo: this is only scene 1. make it adapt
            dateTimeFirstResponderArrived = StaticUtils.DateTimeNowInEvening(DateTimeIncident);
            hasCheckedTimeOfArrival = true;
        }
        ShowThought(sender, $"It's {dateTimeFirstResponderArrived:hh:mm tt}");
    }

    // pulse
    public void CheckPulse(GameObject sender) {
        hasCheckedPulse = true;

        // todo: pulse should be set in EvidenceBody, and assigned to their TouchAreaPulse/s instead of here
        if (pulse == 0) {
            ShowThought(sender, "They have no more pulse...");
        } else {
            ShowThought(sender, $"Their pulse is {pulse} BPM");
        }
    }

    // notepad + pen
    public void SetCanWriteNotepad(bool b) {
        canWriteNotepad = b;
    }
    // page + pen
    public void SetCanWriteForm(bool b) {
        canWriteForm = b;
    }

    // first responder form
    public bool SpawnFormFirstResponder(Player firstResponder) {
        if (Vector3.Distance(firstResponder.transform.position, player.transform.position) > DIST_CONVERSE) { return false; }



        dateTimeFirstResponderFilledUp = StaticUtils.DateTimeNowInEvening(DateTimeIncident);

        HandItem form = Instantiate(HolderData.PrefabFormFirstResponder).GetComponent<HandItem>();
        form.SetPaused(true);
        form.transform.SetPositionAndRotation(firstResponder.HandLeft.position, firstResponder.HandLeft.rotation);

        return true;
    }
    // investigator on case form
    public bool SpawnFormInvestigatorOnCase(Player investigatorOnCase) {
        if (Vector3.Distance(investigatorOnCase.transform.position, player.transform.position) > DIST_CONVERSE) { return false; }



        dateTimeInvestigatorFilledUp = StaticUtils.DateTimeNowInEvening(DateTimeIncident);

        HandItem form = Instantiate(HolderData.PrefabFormInvestigatorOnCase).GetComponent<HandItem>();
        form.SetPaused(true);
        form.transform.SetPositionAndRotation(investigatorOnCase.HandLeft.position, investigatorOnCase.HandLeft.rotation);

        return true;
    }

    // evidence marker
    public void RemoveEvidenceMarker(EvidenceMarkerCopy evidenceMarkerCopy) {
        if (evidenceMarkerCopy.TypeEvidenceMarker == TypeEvidenceMarker.Item) {
            listEvidenceMarkerItemCopies[evidenceMarkerCopy.Index] = null;
        } else
        if (evidenceMarkerCopy.TypeEvidenceMarker == TypeEvidenceMarker.Body) {
            listEvidenceMarkerBodyCopies[evidenceMarkerCopy.Index] = null;
        }
        Destroy(evidenceMarkerCopy.gameObject);
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



        ClearCurrentThought();
        ClearCurrentConversation();
        currentWitness = witness;
        currentDialogue = witness.DialogueData;

        StartConservation();
    }
    public void StartConversation(Phone phone) {
        if (currentPhone != null || Vector3.Distance(phone.transform.position, player.transform.position) > DIST_CONVERSE) { return; }



        ClearCurrentThought();
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
        return typeRole switch {
            TypeRole.FirstResponder => "First Responder",
            TypeRole.InvestigatorOnCase => "Investigator-On-Case",
            TypeRole.SOCOTeamLead => "SOCO Team Lead",
            TypeRole.Photographer => "Photographer",
            TypeRole.Searcher => "Searcher",
            TypeRole.Measurer => "Measurer",
            TypeRole.Sketcher => "Sketcher",
            TypeRole.FingerprintSpecialist => "Fingerprint Specialist",
            TypeRole.Collector => "Collector",
            TypeRole.EvidenceCustodian => "Evidence Custodian",
            _ => "",
        };
    }
}