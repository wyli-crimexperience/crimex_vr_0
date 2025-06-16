using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using System;
using System.Linq;
using TMPro;

[System.Serializable]
public class CrimeSceneButton
{
    [Header("Crime Scene Configuration")]
    public string crimeSceneName;
    public Button crimeSceneButton;
    public GameObject lockedOverlay;
    public GameObject unlockedIndicator;

    [Header("Visual States")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;
    public Sprite lockedIcon;
    public Sprite unlockedIcon;

    [Header("Optional Components")]
    public Image crimeSceneIcon;
    public TextMeshProUGUI statusText;
}

public class CrimeSceneUnlockerManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private UserProfileDisplay userProfileDisplay; // Reference to your profile script

    [Header("Crime Scene Buttons")]
    [SerializeField] private List<CrimeSceneButton> crimeSceneButtons = new List<CrimeSceneButton>();

    [Header("Settings")]
    [SerializeField] private bool autoCheckOnStart = true;
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private float checkCooldown = 5f;
    [SerializeField] private float maxWaitTime = 30f;
    [SerializeField] private float profileCheckInterval = 0.5f;

    [Header("UI Feedback")]
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI statusText;

    // Private fields
    private FirebaseUser currentUser;
    private bool isChecking = false;
    private float lastCheckTime = 0f;
    private Dictionary<string, List<string>> classUnlockData = new Dictionary<string, List<string>>();
    private bool hasCheckedInitially = false;

    // Events
    public System.Action<string> OnCrimeSceneUnlocked;
    public System.Action<string> OnCrimeSceneLocked;
    public System.Action<List<string>> OnUnlockDataLoaded;
    public System.Action OnAllCrimeScenesLocked;

    // Properties
    public bool IsChecking => isChecking;
    public int UnlockedCrimeSceneCount => GetAllUnlockedCrimeScenes().Count;
    public bool HasUnlockedCrimeScenes => UnlockedCrimeSceneCount > 0;

    void Start()
    {
        ValidateReferences();
        InitializeButtons();

        // Show initial status
        UpdateStatusText("Initializing...");

        if (autoCheckOnStart)
        {
            // Start checking process
            StartCoroutine(WaitForProfileThenCheck());
        }
        else
        {
            UpdateStatusText("Ready - use RefreshUnlockStatus() to check");
        }
    }

    private void ValidateReferences()
    {
        if (userProfileDisplay == null)
        {
            LogWarning("UserProfileDisplay reference is missing! Searching for it automatically...");
            userProfileDisplay = FindFirstObjectByType<UserProfileDisplay>();

            if (userProfileDisplay == null)
            {
                LogError("Could not find UserProfileDisplay in scene. Crime scene unlocking may not work properly.");
            }
            else
            {
                LogDebug("Found UserProfileDisplay automatically.");
            }
        }

        if (crimeSceneButtons == null || crimeSceneButtons.Count == 0)
        {
            LogWarning("No crime scene buttons configured!");
        }
    }

    private void InitializeButtons()
    {
        foreach (var crimeSceneButton in crimeSceneButtons)
        {
            if (crimeSceneButton.crimeSceneButton != null)
            {
                // Initially lock all crime scenes
                SetCrimeSceneButtonState(crimeSceneButton, false);

                // Store the crime scene name for the button click handler
                string crimeSceneName = crimeSceneButton.crimeSceneName;

                // Add click listener that checks if crime scene is unlocked
                crimeSceneButton.crimeSceneButton.onClick.AddListener(() => {
                    if (IsCrimeSceneUnlocked(crimeSceneName))
                    {
                        OnCrimeSceneButtonClicked(crimeSceneName);
                    }
                    else
                    {
                        OnLockedCrimeSceneClicked(crimeSceneName);
                    }
                });
            }
            else
            {
                LogWarning($"Crime scene button is null for scene: {crimeSceneButton.crimeSceneName}");
            }
        }

        LogDebug($"Initialized {crimeSceneButtons.Count} crime scene buttons.");
    }

    private IEnumerator WaitForProfileThenCheck()
    {
        float waitTime = 0f;
        UpdateStatusText("Waiting for user authentication...");

        // Wait for user profile to load or timeout
        while (waitTime < maxWaitTime)
        {
            if (userProfileDisplay != null)
            {
                // Check if profile has valid user data
                if (HasValidUserProfile())
                {
                    LogDebug("User profile appears valid. Checking unlocked crime scenes.");
                    UpdateStatusText("Loading crime scene data...");
                    CheckUnlockedCrimeScenesAsync();
                    yield break;
                }
                // Check if we should assume guest user
                else if (waitTime > 5f) // Wait at least 5 seconds before assuming guest
                {
                    LogDebug("Assuming guest user or invalid profile. Locking crime scenes.");
                    UpdateStatusText("Guest user - crime scenes locked");
                    LockAllCrimeScenes();
                    yield break;
                }
            }

            yield return new WaitForSeconds(profileCheckInterval);
            waitTime += profileCheckInterval;
        }

        // If we timeout, try checking anyway
        LogWarning($"Timed out waiting for user profile after {maxWaitTime} seconds. Attempting check anyway.");
        UpdateStatusText("Connection timeout - attempting check...");
        CheckUnlockedCrimeScenesAsync();
    }

    private bool HasValidUserProfile()
    {
        // Check if we have a valid Firebase user
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null && user.IsEmailVerified)
        {
            return true;
        }

        // If you have specific properties on UserProfileDisplay you can check, add them here
        // For example:
        // if (userProfileDisplay != null && userProfileDisplay.HasValidUser)
        // {
        //     return true;
        // }

        return false;
    }

    public async void CheckUnlockedCrimeScenesAsync()
    {
        if (isChecking)
        {
            LogDebug("Crime scene unlock check already in progress.");
            return;
        }

        if (Time.time - lastCheckTime < checkCooldown)
        {
            LogDebug($"Check cooldown active. Wait {checkCooldown - (Time.time - lastCheckTime):F1} seconds.");
            return;
        }

        isChecking = true;
        lastCheckTime = Time.time;
        SetLoadingState(true);
        UpdateStatusText("Checking crime scene access...");

        try
        {
            currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

            if (currentUser != null && currentUser.IsEmailVerified)
            {
                LogDebug($"Authenticated user: {currentUser.Email}, ID: {currentUser.UserId}");
                await LoadUserUnlockData(currentUser.UserId);
                UpdateCrimeSceneButtonStates();
                hasCheckedInitially = true;
            }
            else
            {
                LogWarning("User is not logged in or email not verified.");
                UpdateStatusText("User not authenticated");
                LockAllCrimeScenes();
            }
        }
        catch (Exception ex)
        {
            LogError($"Error checking unlocked crime scenes: {ex.Message}");
            UpdateStatusText($"Error: {ex.Message}");
            LockAllCrimeScenes();
        }
        finally
        {
            isChecking = false;
            SetLoadingState(false);
        }
    }

    private async Task LoadUserUnlockData(string userId)
    {
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            // Get user's enrolled classes
            DocumentReference userDocRef = db.Collection("users").Document(userId);
            DocumentSnapshot userSnapshot = await userDocRef.GetSnapshotAsync();

            if (!userSnapshot.Exists)
            {
                LogWarning("User document not found in Firestore.");
                UpdateStatusText("User data not found");
                return;
            }

            // Debug logging
            LogDebug($"User document exists. Fields: {string.Join(", ", userSnapshot.ToDictionary().Keys)}");

            List<string> enrolledClasses = new List<string>();
            if (userSnapshot.ContainsField("enrolledClasses"))
            {
                var classesArray = userSnapshot.GetValue<List<object>>("enrolledClasses");
                if (classesArray != null)
                {
                    enrolledClasses = classesArray.Select(c => c.ToString()).ToList();
                    LogDebug($"Found enrolledClasses: {string.Join(", ", enrolledClasses)}");
                }
            }
            else
            {
                LogDebug("User document does not contain 'enrolledClasses' field");
            }

            LogDebug($"User enrolled in {enrolledClasses.Count} classes: {string.Join(", ", enrolledClasses)}");

            if (enrolledClasses.Count == 0)
            {
                LogDebug("User not enrolled in any classes.");
                UpdateStatusText("Not enrolled in any classes");
                LockAllCrimeScenes();
                return;
            }

            // Load unlock data for each enrolled class
            classUnlockData.Clear();
            foreach (string classId in enrolledClasses)
            {
                await LoadClassUnlockData(classId);
            }

            var allUnlockedCrimeScenes = GetAllUnlockedCrimeScenes();
            OnUnlockDataLoaded?.Invoke(allUnlockedCrimeScenes);

            UpdateStatusText($"Found {allUnlockedCrimeScenes.Count} unlocked crime scenes");
            LogDebug($"Total unlocked crime scenes: {string.Join(", ", allUnlockedCrimeScenes)}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to load user unlock data: {ex.Message}");
            UpdateStatusText("Failed to load crime scene data");
            throw;
        }
    }

    private async Task LoadClassUnlockData(string classId)
    {
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference classDocRef = db.Collection("classes").Document(classId);
            DocumentSnapshot classSnapshot = await classDocRef.GetSnapshotAsync();

            LogDebug($"Checking class document: {classId}, exists: {classSnapshot.Exists}");

            if (classSnapshot.Exists)
            {
                LogDebug($"Class document fields: {string.Join(", ", classSnapshot.ToDictionary().Keys)}");

                if (classSnapshot.ContainsField("unlockCrimeScene"))
                {
                    var unlockArray = classSnapshot.GetValue<List<object>>("unlockCrimeScene");
                    if (unlockArray != null)
                    {
                        List<string> unlockedCrimeScenes = unlockArray.Select(c => c.ToString().ToLower()).ToList();
                        classUnlockData[classId] = unlockedCrimeScenes;

                        LogDebug($"Class {classId} unlocks crime scenes: {string.Join(", ", unlockedCrimeScenes)}");
                        LogDebug($"Original values before toLowerCase: {string.Join(", ", unlockArray.Select(c => c.ToString()))}");
                    }
                    else
                    {
                        LogDebug($"Class {classId} has null unlockCrimeScene array");
                    }
                }
                else
                {
                    LogDebug($"Class {classId} does not contain 'unlockCrimeScene' field");
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"Failed to load unlock data for class {classId}: {ex.Message}");
        }
    }

    private void UpdateCrimeSceneButtonStates()
    {
        int unlockedCount = 0;

        foreach (var crimeSceneButton in crimeSceneButtons)
        {
            bool isUnlocked = IsCrimeSceneUnlocked(crimeSceneButton.crimeSceneName);
            SetCrimeSceneButtonState(crimeSceneButton, isUnlocked);

            if (isUnlocked)
            {
                unlockedCount++;
                OnCrimeSceneUnlocked?.Invoke(crimeSceneButton.crimeSceneName);
                LogDebug($"Crime scene unlocked: {crimeSceneButton.crimeSceneName}");
            }
            else
            {
                OnCrimeSceneLocked?.Invoke(crimeSceneButton.crimeSceneName);
            }
        }

        UpdateDebugText();

        if (unlockedCount == 0)
        {
            UpdateStatusText("No crime scenes available");
            OnAllCrimeScenesLocked?.Invoke();
        }
        else
        {
            UpdateStatusText($"{unlockedCount}/{crimeSceneButtons.Count} crime scenes available");
        }

        LogDebug($"Updated crime scene states: {unlockedCount}/{crimeSceneButtons.Count} unlocked");
    }

    private bool IsCrimeSceneUnlocked(string crimeSceneName)
    {
        if (string.IsNullOrEmpty(crimeSceneName))
            return false;

        string crimeSceneNameLower = crimeSceneName.ToLower().Trim();

        foreach (var classUnlocks in classUnlockData.Values)
        {
            if (classUnlocks.Contains(crimeSceneNameLower))
            {
                return true;
            }
        }

        return false;
    }

    private void SetCrimeSceneButtonState(CrimeSceneButton crimeSceneButton, bool isUnlocked)
    {
        if (crimeSceneButton.crimeSceneButton != null)
        {
            crimeSceneButton.crimeSceneButton.interactable = isUnlocked;

            // Update visual state
            var buttonImage = crimeSceneButton.crimeSceneButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isUnlocked ? crimeSceneButton.unlockedColor : crimeSceneButton.lockedColor;
            }
        }

        // Update overlay
        if (crimeSceneButton.lockedOverlay != null)
        {
            crimeSceneButton.lockedOverlay.SetActive(!isUnlocked);
        }

        if (crimeSceneButton.unlockedIndicator != null)
        {
            crimeSceneButton.unlockedIndicator.SetActive(isUnlocked);
        }

        // Update icon
        if (crimeSceneButton.crimeSceneIcon != null)
        {
            if (isUnlocked && crimeSceneButton.unlockedIcon != null)
            {
                crimeSceneButton.crimeSceneIcon.sprite = crimeSceneButton.unlockedIcon;
            }
            else if (!isUnlocked && crimeSceneButton.lockedIcon != null)
            {
                crimeSceneButton.crimeSceneIcon.sprite = crimeSceneButton.lockedIcon;
            }
        }

        // Update status text
        if (crimeSceneButton.statusText != null)
        {
            crimeSceneButton.statusText.text = isUnlocked ? "Available" : "Locked";
            crimeSceneButton.statusText.color = isUnlocked ? Color.green : Color.red;
        }
    }

    private void LockAllCrimeScenes()
    {
        foreach (var crimeSceneButton in crimeSceneButtons)
        {
            SetCrimeSceneButtonState(crimeSceneButton, false);
        }

        classUnlockData.Clear();
        UpdateDebugText();
        OnAllCrimeScenesLocked?.Invoke();

        if (!hasCheckedInitially)
        {
            UpdateStatusText("All crime scenes locked");
        }

        LogDebug("All crime scenes locked.");
    }

    private List<string> GetAllUnlockedCrimeScenes()
    {
        HashSet<string> allUnlocked = new HashSet<string>();

        foreach (var classUnlocks in classUnlockData.Values)
        {
            foreach (var crimeScene in classUnlocks)
            {
                allUnlocked.Add(crimeScene);
            }
        }

        return allUnlocked.ToList();
    }

    private void OnCrimeSceneButtonClicked(string crimeSceneName)
    {
        LogDebug($"Crime scene button clicked: {crimeSceneName}");
        UpdateStatusText($"Selected {crimeSceneName}...");

        // Add crime scene navigation logic here
    }

    private void OnLockedCrimeSceneClicked(string crimeSceneName)
    {
        LogDebug($"Locked crime scene clicked: {crimeSceneName}");
        UpdateStatusText($"{crimeSceneName} is locked");

        // Show locked crime scene message or requirements
    }

    private void SetLoadingState(bool isLoading)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(isLoading);
        }
    }

    private void UpdateDebugText()
    {
        if (debugText != null && enableDebugLogging)
        {
            var unlockedCrimeScenes = GetAllUnlockedCrimeScenes();
            debugText.text = unlockedCrimeScenes.Count > 0
                ? $"Unlocked: {string.Join(", ", unlockedCrimeScenes)}"
                : "No crime scenes unlocked";
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        LogDebug($"Status: {message}");
    }

    // Public Methods
    public bool IsCrimeSceneLocked(string crimeSceneName)
    {
        return !IsCrimeSceneUnlocked(crimeSceneName);
    }

    public List<string> GetUnlockedCrimeScenes()
    {
        return GetAllUnlockedCrimeScenes();
    }

    public List<string> GetLockedCrimeScenes()
    {
        var allCrimeScenes = crimeSceneButtons.Select(cb => cb.crimeSceneName).ToList();
        var unlockedCrimeScenes = GetAllUnlockedCrimeScenes();
        return allCrimeScenes.Where(crimeScene => !unlockedCrimeScenes.Contains(crimeScene.ToLower())).ToList();
    }

    public void RefreshUnlockStatus()
    {
        UpdateStatusText("Refreshing crime scene access...");
        CheckUnlockedCrimeScenesAsync();
    }

    public void ForceCheck()
    {
        lastCheckTime = 0f; // Reset cooldown
        CheckUnlockedCrimeScenesAsync();
    }

    public void UnlockCrimeSceneForTesting(string crimeSceneName)
    {
        if (enableDebugLogging)
        {
            LogDebug($"Manually unlocking crime scene for testing: {crimeSceneName}");

            // Add to a test class unlock data
            if (!classUnlockData.ContainsKey("test"))
            {
                classUnlockData["test"] = new List<string>();
            }

            classUnlockData["test"].Add(crimeSceneName.ToLower());
            UpdateCrimeSceneButtonStates();
        }
    }

    // Debug Methods
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[CrimeSceneUnlockerManager] {message}");
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogging)
            Debug.LogWarning($"[CrimeSceneUnlockerManager] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[CrimeSceneUnlockerManager] {message}");
    }

    // Context Menu Methods for Testing
    [ContextMenu("Check Unlocked Crime Scenes")]
    private void ContextCheckCrimeScenes()
    {
        CheckUnlockedCrimeScenesAsync();
    }

    [ContextMenu("Force Refresh")]
    private void ContextForceRefresh()
    {
        ForceCheck();
    }

    [ContextMenu("Lock All Crime Scenes")]
    private void ContextLockAll()
    {
        LockAllCrimeScenes();
    }

    [ContextMenu("Test Unlock Murder Scene")]
    private void ContextUnlockMurderScene()
    {
        UnlockCrimeSceneForTesting("murderscene");
    }

    [ContextMenu("Show Debug Info")]
    private void ContextShowDebugInfo()
    {
        LogDebug($"Is Checking: {isChecking}");
        LogDebug($"Current User: {(FirebaseAuth.DefaultInstance.CurrentUser?.Email ?? "None")}");
        LogDebug($"User Profile Display: {(userProfileDisplay != null ? "Found" : "Missing")}");
        LogDebug($"Unlock Data Count: {classUnlockData.Count}");
        LogDebug($"Unlocked Crime Scenes: {string.Join(", ", GetAllUnlockedCrimeScenes())}");
    }
}