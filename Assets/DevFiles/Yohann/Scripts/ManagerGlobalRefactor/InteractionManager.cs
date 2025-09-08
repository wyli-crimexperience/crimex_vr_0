using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class InteractionManager : MonoBehaviour
{
    [Header("Interactors (assign in Inspector)")]
    [SerializeField] private NearFarInteractor interactorLeft;
    [SerializeField] private NearFarInteractor interactorRight;
    public NearFarInteractor InteractorLeft => interactorLeft;
    public NearFarInteractor InteractorRight => interactorRight;

    [Header("Containers / Prefab parents (assign in Inspector)")]
    [SerializeField] private Transform containerEvidenceMarker;
    [SerializeField] private Transform containerWipes;
    public Transform ContainerWipes => containerWipes;
    // local copy of a command post (same logic you had originally)
    private GameObject commandPostCopy;

    // hand item references
    private HandItem handItemLeft, handItemRight;

    // concrete components that may or may not be attached to the HandItem GameObject.
    // Use GetComponent<T>() because some of these classes may not inherit HandItem.
    private Notepad notepad;
    private PoliceTapeRoll policeTapeRoll;
    private Form form;
    private HandItem evidenceMarkerItem, evidenceMarkerBody;
    private FingerprintTapeRoll fingerprintTapeRoll;
    private FingerprintInk fingerprintInk;
    private FingerprintInkRoller fingerprintInkRoller;
    private FingerprintRecordStrip fingerprintRecordStrip;
    private FingerprintSpoon fingerprintSpoon;
    private EvidencePackSealTapeRoll evidencePackSealTapeRoll;
    private EvidencePack evidencePack;
    private Wipes wipes;

    // evidence marker bookkeeping
    private EvidenceMarkerCopy _evidenceMarkerCopy;
    private int _evidenceMarkerIndex;
    private readonly List<EvidenceMarkerCopy> listEvidenceMarkerItemCopies = new List<EvidenceMarkerCopy>();
    private readonly List<EvidenceMarkerCopy> listEvidenceMarkerBodyCopies = new List<EvidenceMarkerCopy>();

    public TypeItem TypeItemLeft => handItemLeft == null ? TypeItem.None : handItemLeft.TypeItem;
    public TypeItem TypeItemRight => handItemRight == null ? TypeItem.None : handItemRight.TypeItem;

    #region Init
    // Optional init if you want to set interactors at runtime
    public void Init(NearFarInteractor left, NearFarInteractor right)
    {
        interactorLeft = left;
        interactorRight = right;
    }
    #endregion

    #region Grab / Release
    public void GrabItem(HandItem handItem)
    {
        // set left/right if the handItem's interactable matches the interactor's selected
        if (interactorLeft != null && interactorLeft.firstInteractableSelected is XRGrabInteractable left && left == handItem.Interactable)
        {
            handItemLeft = handItem;
            AssignGrabbedItem(handItem);
        }

        if (interactorRight != null && interactorRight.firstInteractableSelected is XRGrabInteractable right && right == handItem.Interactable)
        {
            handItemRight = handItem;
            AssignGrabbedItem(handItem);
        }
    }

    public void ReleaseItem(HandItem handItem)
    {
        if (handItemLeft == handItem) { handItemLeft = null; UnassignGrabbedItem(handItem); }
        if (handItemRight == handItem) { handItemRight = null; UnassignGrabbedItem(handItem); }
    }

    private void AssignGrabbedItem(HandItem handItem)
    {
        // Many of your item classes probably inherit HandItem and the original pattern-match will work.
        // For types that are separate components (not subclasses of HandItem), use GetComponent<T>().

        // If the specific item *is* a subclass of HandItem, these casts will still work.
        if (handItem is Notepad n) notepad = n;
        else if (handItem is PoliceTapeRoll t) policeTapeRoll = t;
        else if (handItem is Form f) { form = f; form.Receive(); }

        // evidence markers remain simple references to the HandItem itself
        if (handItem.TypeItem == TypeItem.EvidenceMarkerItem) evidenceMarkerItem = handItem;
        if (handItem.TypeItem == TypeItem.EvidenceMarkerBody) evidenceMarkerBody = handItem;

        // Wipes
        if (handItem is Wipes w) wipes = w;

        // Fingerprint related - both pattern-match and GetComponent used to be safe
        var tr = handItem.GetComponent<FingerprintTapeRoll>();
        if (tr != null) fingerprintTapeRoll = tr;
        var ink = handItem.GetComponent<FingerprintInk>();
        if (ink != null) fingerprintInk = ink;
        var roller = handItem.GetComponent<FingerprintInkRoller>();
        if (roller != null) fingerprintInkRoller = roller;
        var rstrip = handItem.GetComponent<FingerprintRecordStrip>();
        if (rstrip != null) fingerprintRecordStrip = rstrip;
        var spoon = handItem.GetComponent<FingerprintSpoon>();
        if (spoon != null) fingerprintSpoon = spoon;

        // EvidencePack and EvidencePackSealTapeRoll: use GetComponent because they may not inherit HandItem
        var epseal = handItem.GetComponent<EvidencePackSealTapeRoll>();
        if (epseal != null) evidencePackSealTapeRoll = epseal;
        var epack = handItem.GetComponent<EvidencePack>();
        if (epack != null) evidencePack = epack;
    }

    private void UnassignGrabbedItem(HandItem handItem)
    {
        if (handItem is Notepad) notepad = null;
        else if (handItem is PoliceTapeRoll) policeTapeRoll = null;
        else if (handItem is Form) form = null;

        if (handItem.TypeItem == TypeItem.EvidenceMarkerItem) evidenceMarkerItem = null;
        if (handItem.TypeItem == TypeItem.EvidenceMarkerBody) evidenceMarkerBody = null;

        if (handItem is Wipes) wipes = null;

        // remove fingerprint comps if they were attached
        var tr = handItem.GetComponent<FingerprintTapeRoll>();
        if (tr != null && fingerprintTapeRoll == tr) fingerprintTapeRoll = null;
        var ink = handItem.GetComponent<FingerprintInk>();
        if (ink != null && fingerprintInk == ink) fingerprintInk = null;
        var roller = handItem.GetComponent<FingerprintInkRoller>();
        if (roller != null && fingerprintInkRoller == roller) fingerprintInkRoller = null;
        var rstrip = handItem.GetComponent<FingerprintRecordStrip>();
        if (rstrip != null && fingerprintRecordStrip == rstrip) fingerprintRecordStrip = null;
        var spoon = handItem.GetComponent<FingerprintSpoon>();
        if (spoon != null && fingerprintSpoon == spoon) fingerprintSpoon = null;

        var epseal = handItem.GetComponent<EvidencePackSealTapeRoll>();
        if (epseal != null && evidencePackSealTapeRoll == epseal) evidencePackSealTapeRoll = null;
        var epack = handItem.GetComponent<EvidencePack>();
        if (epack != null && evidencePack == epack) evidencePack = null;
    }
    #endregion

    #region Pinch Handling (moved all logic here)
    public void OnPinchLeft() => HandlePinch(TypeItemLeft, TypeItemRight);
    public void OnPinchRight() => HandlePinch(TypeItemRight, TypeItemLeft);

    private void HandlePinch(TypeItem typeItem1, TypeItem typeItem2)
    {
        var mgr = ManagerGlobal.Instance;
        if (mgr == null) return;

        var timeline = mgr.TimelineManager;

        // Pen logic
        if (typeItem1 == TypeItem.Pen)
        {
            // pen on notepad
            // NOTE: ManagerGlobal must expose the flags used below (see suggestions below).
            if (mgr.CanWriteNotepad && typeItem2 == TypeItem.Notepad && notepad != null)
            {
                if (!mgr.HasWrittenTimeOfArrival && mgr.HasCheckedTimeOfArrival)
                {
                    timeline.SetEventNow(TimelineEvent.FirstResponderArrived, timeline.GetEventTime(TimelineEvent.Incident).Value);
                    notepad.SetTextTime(timeline.GetEventTime(TimelineEvent.FirstResponderArrived).Value.ToString("HH: mm"));
                    mgr.HasWrittenTimeOfArrival = true;
                }
                else if (!mgr.HasWrittenPulse && mgr.HasCheckedPulse)
                {
                    notepad.SetTextPulse($"{mgr.Pulse} BPM");
                    mgr.HasWrittenPulse = true;
                }
            }

            if (mgr.CanWriteForm && typeItem2 == TypeItem.Form && form != null)
            {
                form.WriteOnPage();
            }

            if (mgr.CanWriteEvidencePackSeal && typeItem2 == TypeItem.EvidencePack && evidencePack != null && evidencePack.EvidencePackSeal.IsTaped)
            {
                evidencePack.EvidencePackSeal.SetMarked(true);
            }
        }

        // Police tape
        if (typeItem1 == TypeItem.PoliceTapeRoll && policeTapeRoll != null)
        {
            policeTapeRoll.TriggerTape();
            if (timeline != null)
                timeline.SetEventNow(TimelineEvent.Cordoned, timeline.GetEventTime(TimelineEvent.Incident).Value);
        }

        // Command post - handled entirely here (no ManagerGlobal method required)
        if (typeItem1 == TypeItem.CommandPost)
        {
            if (commandPostCopy == null && mgr != null)
            {
                commandPostCopy = Instantiate(mgr.HolderData.PrefabCommandPostCopy);
            }

            var playerObj = FindFirstObjectByType<Player>();

            if (playerObj != null && commandPostCopy != null)
            {
                Vector3 pos = playerObj.transform.position;
                pos.y = 0.486f;
                commandPostCopy.transform.SetPositionAndRotation(pos, Quaternion.Euler(new Vector3(0, -playerObj.transform.eulerAngles.y, 0)));
            }
        }

        // Form toggle
        if (typeItem1 == TypeItem.Form && form != null)
        {
            form.TogglePage();
        }

        // Evidence marker (item)
        if (typeItem1 == TypeItem.EvidenceMarkerItem && evidenceMarkerItem != null)
        {
            SpawnEvidenceMarker(TypeEvidenceMarker.Item, evidenceMarkerItem);
        }

        // Evidence marker (body)
        if (typeItem1 == TypeItem.EvidenceMarkerBody && evidenceMarkerBody != null)
        {
            SpawnEvidenceMarker(TypeEvidenceMarker.Body, evidenceMarkerBody);
        }

        // Wipes
        if (typeItem1 == TypeItem.Wipes && wipes != null)
        {
            wipes.SpawnWipe();
        }

        // Fingerprint tape
        if (typeItem1 == TypeItem.FingerprintTapeRoll && fingerprintTapeRoll != null)
        {
            if (fingerprintTapeRoll.FingerprintTapeLifted == null)
                fingerprintTapeRoll.ExtendTape();
            else if (fingerprintTapeRoll.CanLiftFingerprint)
                fingerprintTapeRoll.LiftFingerprint();
            else if (fingerprintTapeRoll.CanAttachToForm)
                fingerprintTapeRoll.AttachToForm();
        }

        // Fingerprint ink
        if (typeItem1 == TypeItem.FingerprintInk && fingerprintInk != null) fingerprintInk.ApplyInk();

        // Fingerprint roller
        if (typeItem1 == TypeItem.FingerprintInkRoller && fingerprintInkRoller != null) fingerprintInkRoller.SpreadInk();

        // Fingerprint record strip
        if (typeItem1 == TypeItem.FingerprintRecordStrip && fingerprintRecordStrip != null) fingerprintRecordStrip.ToggleSpoon();

        // Fingerprint spoon
        if (typeItem1 == TypeItem.FingerprintSpoon && fingerprintSpoon != null) fingerprintSpoon.MoveStrip();

        // Evidence pack
        if (typeItem1 == TypeItem.EvidencePack && evidencePack != null && evidencePack.EvidenceCurrent != null)
        {
            evidencePack.PackEvidence();
        }

        // Evidence pack seal tape -> attach tape to pack
        if (typeItem1 == TypeItem.EvidencePackSealTapeRoll && typeItem2 == TypeItem.EvidencePack &&
            evidencePackSealTapeRoll != null && evidencePackSealTapeRoll.EvidencePackSealCurrent != null)
        {
            evidencePackSealTapeRoll.EvidencePackSealCurrent.SetTaped(true);
        }
    }
    #endregion

    #region Evidence Marker Spawning
    private void SpawnEvidenceMarker(TypeEvidenceMarker type, HandItem sourceItem)
    {
        if (containerEvidenceMarker == null)
        {
            Debug.LogWarning("InteractionManager: containerEvidenceMarker not assigned in Inspector.");
            return;
        }

        var list = (type == TypeEvidenceMarker.Item) ? listEvidenceMarkerItemCopies : listEvidenceMarkerBodyCopies;
        _evidenceMarkerIndex = list.Count;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) { _evidenceMarkerIndex = i; break; }
        }

        var prefab = ManagerGlobal.Instance.HolderData.PrefabEvidenceMarkerCopy;
        _evidenceMarkerCopy = Instantiate(prefab, containerEvidenceMarker).GetComponent<EvidenceMarkerCopy>();
        _evidenceMarkerCopy.Init(type, _evidenceMarkerIndex, sourceItem.transform);

        if (_evidenceMarkerIndex == list.Count) list.Add(_evidenceMarkerCopy);
        else list[_evidenceMarkerIndex] = _evidenceMarkerCopy;
    }
    #endregion
}
