using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;

//ROLE MANAGER
// Handles player role switching in a VR scenario. It manages the UI for role selection, 
// instantiates and tracks Player objects for each role, and coordinates the transfer of held items 
// and XR interaction setup when changing roles. The script also manages interactors and their filters, 
// ensures only one role is active at a time, and exposes events for role/menu changes. 
// It supports navigation of roles via UI or input, and ensures a smooth transition between roles 
// by pausing and restoring held items, updating the XR rig, and invoking role-specific logic.

public class RoleManager : MonoBehaviour 
{
    [Header("Role Management")]
    [SerializeField] private Player initialPlayer;
    [SerializeField] private ScrollRect srChangeRole;
    [SerializeField] private GameObject goChangeRole;
    [SerializeField] private Transform containerRoles;
    [SerializeField] private Transform xrInteractionSetup;
    
    [Header("Hand Targets")]
    [SerializeField] private Transform handLeftTarget, handRightTarget;
    public Transform HandLeftTarget => handLeftTarget;
    public Transform HandRightTarget => handRightTarget;
    
    [Header("Interactors")]
    [SerializeField] private NearFarInteractor interactorLeft;
    [SerializeField] private NearFarInteractor interactorRight;
    [SerializeField] private IXRFilter_HandToBriefcaseItem ixrFilter_handToBriefcaseItem;
    
    // Events
    public event System.Action<TypeRole, TypeRole> OnRoleChanged; // oldRole, newRole
    public event System.Action<bool> OnRoleMenuToggled;
    
    // Properties
    public Player CurrentPlayer { get; private set; }
    public TypeRole CurrentRole => CurrentPlayer?.TypeRole ?? TypeRole.None;
    public bool IsRoleMenuActive => goChangeRole.activeSelf;
    
    // Private fields
    private HolderData holderData;
    private List<ListItemRole> listItemRoles = new List<ListItemRole>();
    private Dictionary<TypeRole, Player> dictPlayers = new Dictionary<TypeRole, Player>();
    private int indexRole;
    private Vector3 playerStartPos;
    private Quaternion playerStartRot;
    private bool isChangingRole = false;
    private float thumbstickCooldown = 0.3f; // seconds between allowed navigations
    private float lastThumbstickTime = -1f;
    public void Initialize(HolderData holderData)
    {
        this.holderData = holderData;
        
        // Store initial player position/rotation
        if (initialPlayer != null) 
        {
            playerStartPos = initialPlayer.transform.position;
            playerStartRot = initialPlayer.transform.rotation;
            CurrentPlayer = initialPlayer;
            CurrentPlayer.Init(CurrentPlayer.TypeRole);
            dictPlayers.Add(CurrentPlayer.TypeRole, CurrentPlayer);
        }
        
        SetupRoleUI();
        SetupInteractorFilters();
    }
    
    private void SetupRoleUI()
    {
        if (containerRoles == null || holderData?.PrefabListItemRole == null) return;
        
        // Clear existing roles
        foreach (var role in listItemRoles)
        {
            if (role != null) Destroy(role.gameObject);
        }
        listItemRoles.Clear();
        
        // Create role UI items
        foreach (TypeRole typeRole in Enum.GetValues(typeof(TypeRole))) 
        {
            if (typeRole == TypeRole.None) continue;
            
            var listItemRole = Instantiate(holderData.PrefabListItemRole, containerRoles)
                .GetComponent<ListItemRole>();
            listItemRole.Init(typeRole);
            listItemRole.SetSelected(typeRole == CurrentRole);
            listItemRoles.Add(listItemRole);
            
            // Set initial index
            if (typeRole == CurrentRole)
            {
                indexRole = listItemRoles.Count - 1;
            }
        }
    }
    
    private void SetupInteractorFilters()
    {
        if (ixrFilter_handToBriefcaseItem == null) return;
        
        if (interactorLeft != null)
        {
            interactorLeft.selectFilters.Add(ixrFilter_handToBriefcaseItem);
            interactorLeft.hoverFilters.Add(ixrFilter_handToBriefcaseItem);
        }
        
        if (interactorRight != null)
        {
            interactorRight.selectFilters.Add(ixrFilter_handToBriefcaseItem);
            interactorRight.hoverFilters.Add(ixrFilter_handToBriefcaseItem);
        }
    }
    
    public void ToggleRoleMenu(bool? forceState = null)
    {
        if (isChangingRole) return;
        
        bool newState = forceState ?? !goChangeRole.activeSelf;
        goChangeRole.SetActive(newState);
        Time.timeScale = newState ? 0 : 1;
        
        OnRoleMenuToggled?.Invoke(newState);
        
        if (!newState)
        {
            StartCoroutine(CompleteRoleChange());
        }
    }

    public void NavigateRoles(float verticalInput)
    {
        if (!IsRoleMenuActive || listItemRoles.Count == 0) return;

        // Prevent rapid navigation: only allow if enough time has passed
        if (Time.unscaledTime - lastThumbstickTime < thumbstickCooldown) return;
        if (Mathf.Abs(verticalInput) < 0.5f) return; // Ignore small/no input

        lastThumbstickTime = Time.unscaledTime;

        if (verticalInput < 0)
        {
            indexRole = (indexRole + 1) % listItemRoles.Count;
        }
        else if (verticalInput > 0)
        {
            indexRole = (indexRole - 1 + listItemRoles.Count) % listItemRoles.Count;
        }

        // Update UI selection
        for (int i = 0; i < listItemRoles.Count; i++)
        {
            listItemRoles[i].SetSelected(i == indexRole);
        }

        // Update scroll position
        if (srChangeRole != null)
        {
            srChangeRole.verticalNormalizedPosition =
                Mathf.Clamp01(1f - (2f * indexRole / listItemRoles.Count));
        }
    }

    public void SetRole(TypeRole targetRole)
    {
        if (isChangingRole || CurrentRole == targetRole) return;
        
        // Find the index for this role
        for (int i = 0; i < listItemRoles.Count; i++)
        {
            if (listItemRoles[i].TypeRole == targetRole)
            {
                indexRole = i;
                break;
            }
        }
        
        StartCoroutine(ChangeToRole(targetRole));
    }
    
    private IEnumerator CompleteRoleChange()
    {
        if (listItemRoles.Count == 0 || indexRole >= listItemRoles.Count) 
            yield break;
            
        TypeRole targetRole = listItemRoles[indexRole].TypeRole;
        yield return StartCoroutine(ChangeToRole(targetRole));
    }
    
    private IEnumerator ChangeToRole(TypeRole targetRole)
    {
        if (isChangingRole || CurrentRole == targetRole) yield break;
        
        isChangingRole = true;
        TypeRole previousRole = CurrentRole;
        
        // Retain held items
        var heldItems = PreserveHeldItems();
        
        // Deactivate current player
        if (CurrentPlayer != null)
        {
            CurrentPlayer.SetActive(false);
        }
        
        // Get or create new player
        Player newPlayer = GetOrCreatePlayer(targetRole);
        if (newPlayer == null)
        {
            Debug.LogError($"Failed to create player for role: {targetRole}");
            isChangingRole = false;
            yield break;
        }
        
        // Switch to new player
        CurrentPlayer = newPlayer;
        CurrentPlayer.SetActive(true);
        
        // Position XR setup
        PositionXRSetup();
        
        // Restore held items
        RestoreHeldItems(heldItems);
        
        // Re-enable interactors
        EnableInteractors(true);
        
        // Notify listeners
        OnRoleChanged?.Invoke(previousRole, targetRole);
        
        // Handle special role logic (this could be moved to events)
        HandleRoleSpecificLogic(targetRole);
        
        isChangingRole = false;
    }
    
    private (HandItem left, HandItem right) PreserveHeldItems()
    {
        // Disable interactors
        EnableInteractors(false);
        
        HandItem handItemLeft = null, handItemRight = null;
        
        if (interactorLeft?.firstInteractableSelected is XRGrabInteractable interactableLeft)
        {
            handItemLeft = interactableLeft.GetComponent<HandItem>();
        }
        
        if (interactorRight?.firstInteractableSelected is XRGrabInteractable interactableRight)
        {
            handItemRight = interactableRight.GetComponent<HandItem>();
        }
        
        // Pause held items
        handItemLeft?.SetPaused(true);
        handItemRight?.SetPaused(true);
        
        return (handItemLeft, handItemRight);
    }
    
    private void RestoreHeldItems((HandItem left, HandItem right) heldItems)
    {
        // Unpause items (they should automatically re-attach)
        heldItems.left?.SetPaused(false);
        heldItems.right?.SetPaused(false);
    }
    
    private Player GetOrCreatePlayer(TypeRole targetRole)
    {
        // Check if player already exists
        if (dictPlayers.TryGetValue(targetRole, out Player existingPlayer))
        {
            return existingPlayer;
        }
        
        // Create new player
        if (holderData?.GetPrefabPlayer(targetRole) == null)
        {
            Debug.LogError($"No prefab found for role: {targetRole}");
            return null;
        }
        
        Player newPlayer = Instantiate(holderData.GetPrefabPlayer(targetRole))
            .GetComponent<Player>();
        newPlayer.transform.SetPositionAndRotation(playerStartPos, playerStartRot);
        newPlayer.Init(targetRole);
        
        dictPlayers.Add(targetRole, newPlayer);
        return newPlayer;
    }
    
    private void PositionXRSetup()
    {
        if (xrInteractionSetup != null && CurrentPlayer != null)
        {
            Vector3 pos = CurrentPlayer.transform.position;
            pos.y = playerStartPos.y;
            xrInteractionSetup.SetPositionAndRotation(pos, playerStartRot);
        }
    }
    
    private void EnableInteractors(bool enable)
    {
        if (interactorLeft != null) interactorLeft.allowSelect = enable;
        if (interactorRight != null) interactorRight.allowSelect = enable;
    }
    
    private void HandleRoleSpecificLogic(TypeRole role)
    {
        // This could be moved to events or a separate system
        if (role == TypeRole.InvestigatorOnCase)
        {
            // Timeline logic could be handled by subscribing to OnRoleChanged
            // timelineManager.SetEventNow(TimelineEvent.FirstResponderArrived, 
            //     timelineManager.GetEventTime(TimelineEvent.Incident).Value);
        }
    }
    
    public string GetRoleName(TypeRole typeRole)
    {
        return typeRole switch 
        {
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
    
    public Player GetPlayerForRole(TypeRole role)
    {
        return dictPlayers.TryGetValue(role, out Player player) ? player : null;
    }
    
    public bool HasPlayerForRole(TypeRole role)
    {
        return dictPlayers.ContainsKey(role);
    }
    
    private void OnDestroy()
    {
        // Clean up interactor filters
        if (ixrFilter_handToBriefcaseItem != null)
        {
            interactorLeft?.selectFilters.Remove(ixrFilter_handToBriefcaseItem);
            interactorLeft?.hoverFilters.Remove(ixrFilter_handToBriefcaseItem);
            interactorRight?.selectFilters.Remove(ixrFilter_handToBriefcaseItem);
            interactorRight?.hoverFilters.Remove(ixrFilter_handToBriefcaseItem);
        }
    }
}