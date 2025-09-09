using System;
using System.Collections.Generic;
using UnityEngine;

public class ManagerGlobal : MonoBehaviour
{
    public static ManagerGlobal Instance;

    public HolderData HolderData;

    // Managers
    [Header("Core Managers")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ThoughtManager thoughtManager;
    [SerializeField] private TimelineManager timelineManager;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private EvidenceManager evidenceManager;
    [SerializeField] private FormManager formManager;

    public DialogueManager DialogueManager => dialogueManager;
    public ThoughtManager ThoughtManager => thoughtManager;
    public TimelineManager TimelineManager => timelineManager;
    public InteractionManager InteractionManager => interactionManager;
    public RoleManager RoleManager => roleManager;
    public InputManager InputManager => inputManager;
    public GameStateManager GameStateManager => gameStateManager;
    public FormManager FormManager => formManager;


    public void RemoveEvidenceMarker(EvidenceMarkerCopy evidenceMarkerCopy)
    {
        // Delegate to the new EvidenceManager if available
        if (EvidenceManager.Instance != null)
        {
            EvidenceManager.Instance.RemoveEvidenceMarker(evidenceMarkerCopy);
            return;
        }

        // Fallback to old logic
        if (evidenceMarkerCopy.TypeEvidenceMarker == TypeEvidenceMarker.Item)
        {
            listEvidenceMarkerItemCopies[evidenceMarkerCopy.Index] = null;
        }
        else if (evidenceMarkerCopy.TypeEvidenceMarker == TypeEvidenceMarker.Body)
        {
            listEvidenceMarkerBodyCopies[evidenceMarkerCopy.Index] = null;
        }
        Destroy(evidenceMarkerCopy.gameObject);
    }

    // Convenience properties - delegate to GameStateManager
    public bool CanWriteNotepad
    {
        get => gameStateManager?.CanWriteNotepad ?? false;
        set { if (gameStateManager != null) gameStateManager.CanWriteNotepad = value; }
    }
    public bool CanWriteForm
    {
        get => gameStateManager?.CanWriteForm ?? false;
        set { if (gameStateManager != null) gameStateManager.CanWriteForm = value; }
    }
    public bool CanWriteEvidencePackSeal
    {
        get => gameStateManager?.CanWriteEvidencePackSeal ?? false;
        set { if (gameStateManager != null) gameStateManager.CanWriteEvidencePackSeal = value; }
    }
    public bool HasCheckedTimeOfArrival
    {
        get => gameStateManager?.HasCheckedTimeOfArrival ?? false;
        set { if (gameStateManager != null) gameStateManager.HasCheckedTimeOfArrival = value; }
    }
    public bool HasCheckedPulse
    {
        get => gameStateManager?.HasCheckedPulse ?? false;
        set { if (gameStateManager != null) gameStateManager.HasCheckedPulse = value; }
    }
    public bool HasWrittenTimeOfArrival
    {
        get => gameStateManager?.HasWrittenTimeOfArrival ?? false;
        set { if (gameStateManager != null) gameStateManager.HasWrittenTimeOfArrival = value; }
    }
    public bool HasWrittenPulse
    {
        get => gameStateManager?.HasWrittenPulse ?? false;
        set { if (gameStateManager != null) gameStateManager.HasWrittenPulse = value; }
    }
    public int Pulse
    {
        get => gameStateManager?.Pulse ?? 0;
        set { if (gameStateManager != null) gameStateManager.Pulse = value; }
    }

    // Convenience properties for role and player
    public TypeRole TypeRolePlayer => roleManager?.CurrentRole ?? TypeRole.None;
    public Player CurrentPlayer => roleManager?.CurrentPlayer;

    // Other references
    [SerializeField] private Transform containerPoliceTape;
    public Transform ContainerPoliceTape => containerPoliceTape;

    [SerializeField] private Transform vrTargetLeftHand, vrTargetRightHand, vrTargetHead;
    public Transform VRTargetLeftHand => vrTargetLeftHand;
    public Transform VRTargetRightHand => vrTargetRightHand;
    public Transform VRTargetHead => vrTargetHead;

    // Evidence markers (to be moved to EvidenceManager)
    private List<EvidenceMarkerCopy> listEvidenceMarkerItemCopies = new List<EvidenceMarkerCopy>();
    private List<EvidenceMarkerCopy> listEvidenceMarkerBodyCopies = new List<EvidenceMarkerCopy>();

    private void Awake()
    {
        Instance = this;
        InitializeAllManagers();
        SetupManagerConnections();
    }

    private void InitializeAllManagers()
    {
        // Initialize managers in dependency order
        inputManager?.Initialize(HolderData);
        gameStateManager?.Initialize(thoughtManager, timelineManager);
        roleManager?.Initialize(HolderData);

        if (dialogueManager != null && CurrentPlayer != null)
            dialogueManager.Init(CurrentPlayer);

        timelineManager?.InitIncident(DateTime.Now.AddHours(-0.5f));
        thoughtManager?.ClearCurrentThought();
        dialogueManager?.StopDialogue();

        // Start the scene
        gameStateManager?.StartScene();
    }

    private void SetupManagerConnections()
    {
        // Connect input events to handlers
        if (inputManager != null)
        {
            inputManager.OnPrimaryButtonPressed += HandlePrimaryButton;
            inputManager.OnSecondaryButtonLeftPressed += HandleSecondaryButtonLeft;
            inputManager.OnSecondaryButtonRightPressed += HandleSecondaryButtonRight;
            inputManager.OnPinchLeft += () => interactionManager?.OnPinchLeft();
            inputManager.OnPinchRight += () => interactionManager?.OnPinchRight();
            inputManager.OnThumbstickInput += HandleThumbstickInput;
        }

        // Connect role change events
        if (roleManager != null)
        {
            roleManager.OnRoleChanged += OnRoleChanged;
        }

        // Connect game state events
        if (gameStateManager != null)
        {
            gameStateManager.OnSceneCompleted += OnSceneCompleted;
            // Add other game state event subscriptions as needed
        }
    }

    #region Input Handlers

    private void HandlePrimaryButton()
    {
        if (!dialogueManager.IsInDialogue)
        {
            thoughtManager.ClearCurrentThought();
            roleManager.ToggleRoleMenu();
        }
        else
        {
            dialogueManager.NextDialogue();
        }
    }

    private void HandleSecondaryButtonLeft()
    {
        // TODO: Add functionality for secondary left button
    }

    private void HandleSecondaryButtonRight()
    {
        // TODO: Add functionality for secondary right button
    }

    private void HandleThumbstickInput(Vector2 input, InputHand hand)
    {
        // Role menu navigation
        if (roleManager.IsRoleMenuActive)
        {
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                roleManager.NavigateRoles(input.y);
            }
        }
    }

    #endregion

    private void OnRoleChanged(TypeRole oldRole, TypeRole newRole)
    {
        // Handle role-specific timeline events
        if (newRole == TypeRole.InvestigatorOnCase)
        {
            timelineManager.SetEventNow(TimelineEvent.FirstResponderArrived,
                timelineManager.GetEventTime(TimelineEvent.Incident).Value);
        }

        // Update dialogue manager with new player
        if (dialogueManager != null && CurrentPlayer != null)
        {
            dialogueManager.Init(CurrentPlayer);
        }
    }

    private void OnSceneCompleted()
    {
        Debug.Log("Scene completed!");
        // Handle scene completion logic
    }

    // Game logic methods - delegate to GameStateManager

    public void CheckWristwatch(GameObject sender)
    {
        gameStateManager?.CheckWristwatch(sender);
    }

    public void CheckPulse(GameObject sender)
    {
        gameStateManager?.CheckPulse(sender);
    }

    public void OnSceneEnd()
    {
        gameStateManager?.CompleteScene();
    }

    private void OnDestroy()
    {
        // Cleanup is handled by individual managers
    }
}