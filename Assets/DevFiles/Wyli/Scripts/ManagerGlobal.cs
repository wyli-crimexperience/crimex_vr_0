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

    Form,

    // first responder
    Notepad,
    Pen,
    PoliceTapeRoll,
    Phone,

    // soco team leader
    CommandPost,
    
    // soco photographer
    Camera,

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

    Wipes,
    Wipe,
    FingerprintInkingSlab,
    FingerprintInk,
    FingerprintInkRoller,
    FingerprintSpoon,
    FingerprintRecordStrip,

    // soco collector
    SterileSwab,
    EvidencePack,
    EvidencePackSealTapeRoll,

    // ioc or soco team leader part 2
    ItemOfIdentification,

    // evidence custodian
    EvidenceChecklist
}
public enum TypeItemForm {
    FirstResponder,
    InvestigatorOnCase,
    Sketcher,
    LatentFingerprint,
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
    Magnetic,
    Ink
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

    //Refactor Stuff

    //Hookup
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ThoughtManager thoughtManager;
    [SerializeField] private TimelineManager timelineManager;
    [SerializeField] private InteractionManager interactionManager;
    public DialogueManager DialogueManager => dialogueManager;
    public ThoughtManager ThoughtManager => thoughtManager;
    public TimelineManager TimelineManager => timelineManager;
    public InteractionManager InteractionManager => interactionManager;

    //end of Refactor Stuff
    // Expose flags used by InteractionManager (read/write as needed)
    public bool CanWriteNotepad { get => canWriteNotepad; set => canWriteNotepad = value; }
    public bool CanWriteForm { get => canWriteForm; set => canWriteForm = value; }
    public bool CanWriteEvidencePackSeal { get => canWriteEvidencePackSeal; set => canWriteEvidencePackSeal = value; }

    public bool HasCheckedTimeOfArrival { get => hasCheckedTimeOfArrival; set => hasCheckedTimeOfArrival = value; }
    public bool HasCheckedPulse { get => hasCheckedPulse; set => hasCheckedPulse = value; }
    public bool HasWrittenTimeOfArrival { get => hasWrittenTimeOfArrival; set => hasWrittenTimeOfArrival = value; }
    public bool HasWrittenPulse { get => hasWrittenPulse; set => hasWrittenPulse = value; }

    public int Pulse { get => pulse; set => pulse = value; }


    [SerializeField] private NearFarInteractor interactorLeft, interactorRight;

    [SerializeField] private IXRFilter_HandToBriefcaseItem ixrFilter_handToBriefcaseItem;

    [SerializeField] private Player player;
    public TypeRole TypeRolePlayer => player == null ? TypeRole.None : player.TypeRole;

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
    [SerializeField] private GameObject goChangeRole;

    [SerializeField] private Transform containerRoles;

    private GameObject commandPostCopy;

    private EvidenceMarkerCopy _evidenceMarkerCopy;
    private int _evidenceMarkerIndex;
    private List<EvidenceMarkerCopy> listEvidenceMarkerItemCopies = new List<EvidenceMarkerCopy>();
    private List<EvidenceMarkerCopy> listEvidenceMarkerBodyCopies = new List<EvidenceMarkerCopy>();

    

    // item flags
    private bool canWriteNotepad, canWriteEvidencePackSeal, hasCheckedTimeOfArrival, hasCheckedPulse, hasWrittenTimeOfArrival, hasWrittenPulse, canWriteForm;

    private int pulse;


    private List<ListItemRole> listItemRoles = new List<ListItemRole>();
    private int indexRole;

    private Vector3 playerStartPos;
    private Quaternion playerStartRot;
    private Dictionary<TypeRole, Player> dictPlayers = new Dictionary<TypeRole, Player>();

    private void Awake() {

        //Init Refactored Managers
        dialogueManager.Init(player);
        timelineManager.InitIncident(DateTime.Now.AddHours(-0.5f));

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


        if (thoughtManager != null)
            thoughtManager.ClearCurrentThought();
        if (dialogueManager != null)
            dialogueManager.StopDialogue();

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
    private void Update()
    {
        // Example debug thought (optional)
        // thoughtManager.ShowThought(gameObject, $"{srChangeRole.verticalNormalizedPosition}");

        // No more dialogue checks here
        // DialogueManager now handles witness/phone distance in its own Update().
    }
    private void PrimaryButtonLeft(InputAction.CallbackContext context) {
        PrimaryButton();
    }
    private void PrimaryButtonRight(InputAction.CallbackContext context) {
        PrimaryButton();
    }
    private void PrimaryButton()
    {
        if (!dialogueManager.IsInDialogue)   // ✅ use DialogueManager state
        {
            thoughtManager.ClearCurrentThought();   // ✅ instead of goThought.SetActive(false)
            ToggleMenuChangeRole(!goChangeRole.activeSelf);
        }
        else
        {
            dialogueManager.NextDialogue();   // ✅ delegate dialogue progression
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

    public void OnSceneEnd()
    {
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
                timelineManager.SetEventNow(TimelineEvent.FirstResponderArrived, timelineManager.GetEventTime(TimelineEvent.Incident).Value);
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
    private void PinchLeft(InputAction.CallbackContext context)
    {
        if (context.performed) interactionManager.OnPinchLeft();
    }

    private void PinchRight(InputAction.CallbackContext context)
    {
        if (context.performed) interactionManager.OnPinchRight();
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

    // wristwatch
    public void CheckWristwatch(GameObject sender) {
        if (!hasCheckedTimeOfArrival) {
            // todo: this is only scene 1. make it adapt
            timelineManager.SetEventNow(TimelineEvent.FirstResponderArrived, timelineManager.GetEventTime(TimelineEvent.Incident).Value);
            hasCheckedTimeOfArrival = true;
        }
        thoughtManager.ShowThought(sender, "They have no more pulse...");
    }

    // pulse
    public void CheckPulse(GameObject sender) {
        hasCheckedPulse = true;

        // todo: pulse should be set in EvidenceBody, and assigned to their TouchAreaPulse/s instead of here
        if (pulse == 0) {
            thoughtManager.ShowThought(sender, "They have no more pulse...");
        } else {
            thoughtManager.ShowThought(sender, "They have no more pulse...");
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

        //TimeLineManager event
        timelineManager.SetEventNow(TimelineEvent.FirstResponderFilledUp,
                               timelineManager.GetEventTime(TimelineEvent.Incident).Value);

        HandItem form = Instantiate(HolderData.PrefabFormFirstResponder).GetComponent<HandItem>();
        form.SetPaused(true);
        form.transform.SetPositionAndRotation(firstResponder.HandLeft.position, firstResponder.HandLeft.rotation);

        return true;
    }
    // investigator on case form
    public bool SpawnFormInvestigatorOnCase(Player investigatorOnCase) {
        if (Vector3.Distance(investigatorOnCase.transform.position, player.transform.position) > DIST_CONVERSE) { return false; }



        timelineManager.SetEventNow(TimelineEvent.InvestigatorFilledUp, timelineManager.GetEventTime(TimelineEvent.Incident).Value);

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