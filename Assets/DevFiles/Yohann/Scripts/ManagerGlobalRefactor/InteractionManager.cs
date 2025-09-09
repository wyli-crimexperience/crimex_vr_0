using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// INTERACTION MANAGER 
// Manages XR interactors,
// item grabbing/releasing,
// pinch interactions
// and evidence marker pooling for the VR scene.

// Handles caching of item components,
// event subscriptions,
// and provides a public API for querying and manipulating held items.

public class InteractionManager : MonoBehaviour
{
    [Header("Interactors")]
    [SerializeField] private NearFarInteractor interactorLeft;
    [SerializeField] private NearFarInteractor interactorRight;

    [Header("Containers")]
    [SerializeField] private Transform containerEvidenceMarker;
    [SerializeField] private Transform containerWipes;

    [Header("Prefabs")]
    [SerializeField] private GameObject commandPostPrefab;
    [SerializeField] private GameObject evidenceMarkerPrefab;

    // Public accessors
    public NearFarInteractor InteractorLeft => interactorLeft;
    public NearFarInteractor InteractorRight => interactorRight;
    public Transform ContainerWipes => containerWipes;

    // Hand item tracking using a more flexible dictionary approach
    private readonly Dictionary<XRBaseInteractor, HandItem> handItems = new();
    private readonly Dictionary<XRBaseInteractor, Dictionary<System.Type, Component>> componentCache = new();

    // Object pooling for evidence markers
    private readonly Dictionary<TypeEvidenceMarker, Stack<EvidenceMarkerCopy>> evidenceMarkerPool = new();
    private readonly Dictionary<TypeEvidenceMarker, List<EvidenceMarkerCopy>> activeEvidenceMarkers = new();

    // Command post instance
    private GameObject commandPostInstance;

    // Events for better decoupling
    public static event System.Action<HandItem, XRBaseInteractor> OnItemGrabbed;
    public static event System.Action<HandItem, XRBaseInteractor> OnItemReleased;
    public static event System.Action<TypeItem, XRBaseInteractor> OnPinchPerformed;

    #region Properties
    public TypeItem TypeItemLeft => GetHandItemType(interactorLeft);
    public TypeItem TypeItemRight => GetHandItemType(interactorRight);

    private TypeItem GetHandItemType(XRBaseInteractor interactor)
    {
        return handItems.TryGetValue(interactor, out var item) ? item.TypeItem : TypeItem.None;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponentCache();
        InitializeEvidenceMarkerPools();
    }

    private void OnEnable()
    {
        // Subscribe to XR events if interactors are assigned
        SubscribeToInteractorEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromInteractorEvents();
    }
    #endregion

    #region Initialization
    public void Init(NearFarInteractor left, NearFarInteractor right)
    {
        UnsubscribeFromInteractorEvents();

        interactorLeft = left;
        interactorRight = right;

        SubscribeToInteractorEvents();
    }

    private void InitializeComponentCache()
    {
        componentCache[interactorLeft] = new Dictionary<System.Type, Component>();
        componentCache[interactorRight] = new Dictionary<System.Type, Component>();
    }

    private void InitializeEvidenceMarkerPools()
    {
        foreach (TypeEvidenceMarker markerType in System.Enum.GetValues(typeof(TypeEvidenceMarker)))
        {
            evidenceMarkerPool[markerType] = new Stack<EvidenceMarkerCopy>();
            activeEvidenceMarkers[markerType] = new List<EvidenceMarkerCopy>();
        }
    }

    private void SubscribeToInteractorEvents()
    {
        if (interactorLeft != null)
        {
            interactorLeft.selectEntered.AddListener(OnSelectEntered);
            interactorLeft.selectExited.AddListener(OnSelectExited);
        }

        if (interactorRight != null)
        {
            interactorRight.selectEntered.AddListener(OnSelectEntered);
            interactorRight.selectExited.AddListener(OnSelectExited);
        }
    }

    private void UnsubscribeFromInteractorEvents()
    {
        if (interactorLeft != null)
        {
            interactorLeft.selectEntered.RemoveListener(OnSelectEntered);
            interactorLeft.selectExited.RemoveListener(OnSelectExited);
        }

        if (interactorRight != null)
        {
            interactorRight.selectEntered.RemoveListener(OnSelectEntered);
            interactorRight.selectExited.RemoveListener(OnSelectExited);
        }
    }
    #endregion

    #region XR Event Handlers
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.TryGetComponent<HandItem>(out var handItem))
        {
            GrabItem(handItem, args.interactorObject as XRBaseInteractor);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactableObject.transform.TryGetComponent<HandItem>(out var handItem))
        {
            ReleaseItem(handItem, args.interactorObject as XRBaseInteractor);
        }
    }
    #endregion

    #region Item Management
    public void GrabItem(HandItem handItem, XRBaseInteractor interactor)
    {
        if (interactor == null || handItem == null) return;

        handItems[interactor] = handItem;
        CacheComponents(handItem, interactor);

        // Handle special grab logic
        if (TryGetCachedComponent<Form>(interactor, out var form))
        {
            form.Receive();
        }
        else
        {
            Debug.LogWarning("Form component not found on held item.");
        }

        OnItemGrabbed?.Invoke(handItem, interactor);
    }

    public void ReleaseItem(HandItem handItem, XRBaseInteractor interactor)
    {
        if (interactor == null || handItem == null) return;

        handItems.Remove(interactor);
        ClearComponentCache(interactor);

        OnItemReleased?.Invoke(handItem, interactor);
    }

    // Legacy methods for backward compatibility
    public void GrabItem(HandItem handItem)
    {
        var interactor = GetInteractorForHandItem(handItem);
        if (interactor != null) GrabItem(handItem, interactor);
    }

    public void ReleaseItem(HandItem handItem)
    {
        var interactor = GetInteractorForHandItem(handItem);
        if (interactor != null) ReleaseItem(handItem, interactor);
    }

    private XRBaseInteractor GetInteractorForHandItem(HandItem handItem)
    {
        return handItems.FirstOrDefault(kvp => kvp.Value == handItem).Key;
    }
    #endregion

    #region Component Caching
    private void CacheComponents(HandItem handItem, XRBaseInteractor interactor)
    {
        if (!componentCache.ContainsKey(interactor))
            componentCache[interactor] = new Dictionary<System.Type, Component>();

        var cache = componentCache[interactor];
        cache.Clear();

        // Cache all relevant components
        var componentTypes = new System.Type[]
        {
            typeof(Notepad), typeof(PoliceTapeRoll), typeof(Form), typeof(Wipes),
            typeof(FingerprintTapeRoll), typeof(FingerprintInk), typeof(FingerprintInkRoller),
            typeof(FingerprintRecordStrip), typeof(FingerprintSpoon), typeof(EvidencePackSealTapeRoll),
            typeof(EvidencePack)
        };

        foreach (var type in componentTypes)
        {
            if (handItem.TryGetComponent(type, out var component))
            {
                cache[type] = component;
            }
        }
    }

    private void ClearComponentCache(XRBaseInteractor interactor)
    {
        if (componentCache.ContainsKey(interactor))
        {
            componentCache[interactor].Clear();
        }
    }

    private bool TryGetCachedComponent<T>(XRBaseInteractor interactor, out T component) where T : Component
    {
        component = null;
        if (componentCache.TryGetValue(interactor, out var cache) &&
            cache.TryGetValue(typeof(T), out var cachedComponent))
        {
            component = cachedComponent as T;
            return component != null;
        }
        return false;
    }

    private T GetCachedComponent<T>(XRBaseInteractor interactor) where T : Component
    {
        TryGetCachedComponent<T>(interactor, out var component);
        return component;
    }
    #endregion

    #region Pinch Handling
    public void OnPinchLeft() => HandlePinch(interactorLeft, interactorRight);
    public void OnPinchRight() => HandlePinch(interactorRight, interactorLeft);

    private void HandlePinch(XRBaseInteractor primaryInteractor, XRBaseInteractor secondaryInteractor)
    {
        if (!handItems.TryGetValue(primaryInteractor, out var primaryItem)) return;

        var primaryType = primaryItem.TypeItem;
        var secondaryType = handItems.TryGetValue(secondaryInteractor, out var secondaryItem)
            ? secondaryItem.TypeItem
            : TypeItem.None;

        OnPinchPerformed?.Invoke(primaryType, primaryInteractor);

        // Use strategy pattern for cleaner interaction handling
        HandleInteraction(primaryType, secondaryType, primaryInteractor, secondaryInteractor);
    }

    private void HandleInteraction(TypeItem primaryType, TypeItem secondaryType,
        XRBaseInteractor primaryInteractor, XRBaseInteractor secondaryInteractor)
    {
        var mgr = ManagerGlobal.Instance;
        if (mgr == null) return;

        switch (primaryType)
        {
            case TypeItem.Pen:
                HandlePenInteraction(secondaryType, primaryInteractor, secondaryInteractor, mgr);
                break;
            case TypeItem.PoliceTapeRoll:
                HandlePoliceTapeInteraction(primaryInteractor, mgr);
                break;
            case TypeItem.CommandPost:
                HandleCommandPostInteraction();
                break;
            case TypeItem.Form:
                HandleFormInteraction(primaryInteractor);
                break;
            case TypeItem.EvidenceMarkerItem:
                SpawnEvidenceMarker(TypeEvidenceMarker.Item, primaryInteractor);
                break;
            case TypeItem.EvidenceMarkerBody:
                SpawnEvidenceMarker(TypeEvidenceMarker.Body, primaryInteractor);
                break;
            case TypeItem.Wipes:
                HandleWipesInteraction(primaryInteractor);
                break;
            case TypeItem.FingerprintTapeRoll:
                HandleFingerprintTapeInteraction(primaryInteractor);
                break;
            case TypeItem.FingerprintInk:
                HandleFingerprintInkInteraction(primaryInteractor);
                break;
            case TypeItem.FingerprintInkRoller:
                HandleFingerprintRollerInteraction(primaryInteractor);
                break;
            case TypeItem.FingerprintRecordStrip:
                HandleFingerprintRecordStripInteraction(primaryInteractor);
                break;
            case TypeItem.FingerprintSpoon:
                HandleFingerprintSpoonInteraction(primaryInteractor);
                break;
            case TypeItem.EvidencePack:
                HandleEvidencePackInteraction(primaryInteractor);
                break;
            case TypeItem.EvidencePackSealTapeRoll:
                HandleEvidencePackSealTapeInteraction(secondaryType, primaryInteractor);
                break;
        }
    }
    #endregion

    #region Specific Interaction Handlers
    private void HandlePenInteraction(TypeItem secondaryType, XRBaseInteractor primaryInteractor,
        XRBaseInteractor secondaryInteractor, ManagerGlobal mgr)
    {
        var timeline = mgr?.TimelineManager;
        if (timeline == null) return;

        switch (secondaryType)
        {
            case TypeItem.Notepad when mgr.CanWriteNotepad:
                if (TryGetCachedComponent<Notepad>(secondaryInteractor, out var notepad))
                {
                    HandlePenOnNotepad(notepad, mgr, timeline);
                }
                break;
            case TypeItem.Form when mgr.CanWriteForm:
                if (TryGetCachedComponent<Form>(secondaryInteractor, out var form))
                {
                    form.WriteOnPage();
                }
                break;
            case TypeItem.EvidencePack when mgr.CanWriteEvidencePackSeal:
                if (TryGetCachedComponent<EvidencePack>(secondaryInteractor, out var evidencePack) &&
                    evidencePack.EvidencePackSeal.IsTaped)
                {
                    evidencePack.EvidencePackSeal.SetMarked(true);
                }
                break;
        }
    }

    private void HandlePenOnNotepad(Notepad notepad, ManagerGlobal mgr, object timeline)
    {
        if (!mgr.HasWrittenTimeOfArrival && mgr.HasCheckedTimeOfArrival)
        {
            // Example: write current time or a placeholder
            notepad.SetTextTime(System.DateTime.Now.ToString("HH:mm"));
            mgr.HasWrittenTimeOfArrival = true;
            mgr.GameStateManager?.WriteTimeOfArrival();
        }
        else if (!mgr.HasWrittenPulse && mgr.HasCheckedPulse)
        {
            notepad.SetTextPulse($"{mgr.Pulse} BPM");
            mgr.HasWrittenPulse = true;
            mgr.GameStateManager?.WritePulse();
        }
    }

    private void HandlePoliceTapeInteraction(XRBaseInteractor interactor, ManagerGlobal mgr)
    {
        if (TryGetCachedComponent<PoliceTapeRoll>(interactor, out var tape))
        {
            tape.TriggerTape();
            mgr.TimelineManager?.SetEventNow(TimelineEvent.Cordoned,
                mgr.TimelineManager.GetEventTime(TimelineEvent.Incident).Value);
        }
    }

    private void HandleCommandPostInteraction()
    {
        if (commandPostInstance == null)
        {
            var prefab = commandPostPrefab ?? ManagerGlobal.Instance?.HolderData?.PrefabCommandPostCopy;
            if (prefab != null)
            {
                commandPostInstance = Instantiate(prefab);
            }
        }

        if (commandPostInstance != null)
        {
            PositionCommandPost();
        }
    }

    private void PositionCommandPost()
    {
        var player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            var position = player.transform.position;
            position.y = 0.486f;
            var rotation = Quaternion.Euler(0, -player.transform.eulerAngles.y, 0);
            commandPostInstance.transform.SetPositionAndRotation(position, rotation);
        }
    }

    private void HandleFormInteraction(XRBaseInteractor interactor)
    {
        if (TryGetCachedComponent<Form>(interactor, out var form))
        {
            form.TogglePage();
        }
        else
        {
            Debug.LogWarning("Form component not found on held item.");
        }
    }

    private void HandleWipesInteraction(XRBaseInteractor interactor)
    {
        if (TryGetCachedComponent<Wipes>(interactor, out var wipes))
        {
            wipes.SpawnWipe();
        }
    }

    private void HandleFingerprintTapeInteraction(XRBaseInteractor interactor)
    {
        if (TryGetCachedComponent<FingerprintTapeRoll>(interactor, out var tape))
        {
            if (tape.FingerprintTapeLifted == null)
                tape.ExtendTape();
            else if (tape.CanLiftFingerprint)
                tape.LiftFingerprint();
            else if (tape.CanAttachToForm)
                tape.AttachToForm();
        }
    }

    private void HandleFingerprintInkInteraction(XRBaseInteractor interactor)
    {
        GetCachedComponent<FingerprintInk>(interactor)?.ApplyInk();
    }

    private void HandleFingerprintRollerInteraction(XRBaseInteractor interactor)
    {
        GetCachedComponent<FingerprintInkRoller>(interactor)?.SpreadInk();
    }

    private void HandleFingerprintRecordStripInteraction(XRBaseInteractor interactor)
    {
        GetCachedComponent<FingerprintRecordStrip>(interactor)?.ToggleSpoon();
    }

    private void HandleFingerprintSpoonInteraction(XRBaseInteractor interactor)
    {
        GetCachedComponent<FingerprintSpoon>(interactor)?.MoveStrip();
    }

    private void HandleEvidencePackInteraction(XRBaseInteractor interactor)
    {
        if (TryGetCachedComponent<EvidencePack>(interactor, out var pack) && pack.EvidenceCurrent != null)
        {
            pack.PackEvidence();
        }
    }

    private void HandleEvidencePackSealTapeInteraction(TypeItem secondaryType, XRBaseInteractor interactor)
    {
        if (secondaryType == TypeItem.EvidencePack &&
            TryGetCachedComponent<EvidencePackSealTapeRoll>(interactor, out var sealTape) &&
            sealTape.EvidencePackSealCurrent != null)
        {
            sealTape.EvidencePackSealCurrent.SetTaped(true);
        }
    }
    #endregion

    #region Evidence Marker Management with Object Pooling
    private void SpawnEvidenceMarker(TypeEvidenceMarker type, XRBaseInteractor interactor)
    {
        if (containerEvidenceMarker == null)
        {
            Debug.LogWarning("Evidence marker container not assigned");
            return;
        }

        if (!handItems.TryGetValue(interactor, out var sourceItem))
        {
            Debug.LogWarning("No source item in hand to spawn marker from");
            return;
        }

        var marker = GetPooledEvidenceMarker(type);
        if (marker == null)
        {
            var prefab = evidenceMarkerPrefab ?? ManagerGlobal.Instance?.HolderData?.PrefabEvidenceMarkerCopy;
            if (prefab == null)
            {
                Debug.LogError("Evidence marker prefab not assigned!");
                return;
            }

            marker = Instantiate(prefab, containerEvidenceMarker).GetComponent<EvidenceMarkerCopy>();
        }

        var index = activeEvidenceMarkers[type].Count;
        marker.Initialize(
            type,
            index,
            index + 1,
            sourceItem.transform.position,
            sourceItem.transform.rotation
        );

        marker.gameObject.SetActive(true);
        activeEvidenceMarkers[type].Add(marker);

        Debug.Log($"Spawned {type} evidence marker #{index + 1} at {sourceItem.transform.position}");
    }


    private EvidenceMarkerCopy GetPooledEvidenceMarker(TypeEvidenceMarker type)
    {
        if (evidenceMarkerPool[type].Count > 0)
        {
            return evidenceMarkerPool[type].Pop();
        }
        return null;
    }

    public void ReturnEvidenceMarkerToPool(EvidenceMarkerCopy marker, TypeEvidenceMarker type)
    {
        marker.gameObject.SetActive(false);
        marker.transform.SetParent(containerEvidenceMarker);
        activeEvidenceMarkers[type].Remove(marker);
        evidenceMarkerPool[type].Push(marker);
    }
    #endregion

    #region Public API for External Systems
    public bool HasItemInHand(TypeItem itemType, XRBaseInteractor interactor = null)
    {
        if (interactor != null)
        {
            return handItems.TryGetValue(interactor, out var item) && item.TypeItem == itemType;
        }

        return handItems.Values.Any(item => item.TypeItem == itemType);
    }

    public T GetHeldComponent<T>(XRBaseInteractor interactor) where T : Component
    {
        return TryGetCachedComponent<T>(interactor, out var component) ? component : null;
    }

    public HandItem GetHeldItem(XRBaseInteractor interactor)
    {
        return handItems.TryGetValue(interactor, out var item) ? item : null;
    }

    private bool EnsureComponent<T>(XRBaseInteractor interactor, out T component) where T : Component
    {
        if (TryGetCachedComponent(interactor, out component)) return true;
        Debug.LogWarning($"{typeof(T).Name} not found on held item.");
        return false;
    }
    #endregion
}