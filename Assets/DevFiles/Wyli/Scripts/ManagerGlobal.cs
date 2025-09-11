using System;
using System.Collections.Generic;
using UnityEngine;

// ManagerGlobal
// This script acts as the central manager and singleton for the scene, providing global access to all core managers (DialogueManager, ThoughtManager, TimelineManager, InteractionManager, RoleManager, InputManager, GameStateManager, EvidenceManager, FormManager).
// It initializes and connects these managers, delegates convenience properties and methods for game state and player role, and handles input events and scene logic.
// The script also manages evidence marker removal (with legacy support), player role switching, and scene completion events, serving as the main entry point for global game logic in the Unity scene.
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
    [SerializeField] private VRRigManager vrRigManager;
    public DialogueManager DialogueManager => dialogueManager;
    public ThoughtManager ThoughtManager => thoughtManager;
    public TimelineManager TimelineManager => timelineManager;
    public InteractionManager InteractionManager => interactionManager;
    public RoleManager RoleManager => roleManager;
    public InputManager InputManager => inputManager;
    public GameStateManager GameStateManager => gameStateManager;
    public FormManager FormManager => formManager;
    public EvidenceManager EvidenceManager => evidenceManager;
    public VRRigManager VRRigManager => vrRigManager;

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

    public void OnSceneEnd()
    {
        gameStateManager?.CompleteScene();
    }

    private void OnDestroy()
    {
        // Cleanup is handled by individual managers
    }
}