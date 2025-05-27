using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;

public class StatisticsManager : MonoBehaviour
{
    [Header("User Info UI")]
    public TMP_Text classCodeText;
    public TMP_Text createdAtText;
    public TMP_Text displayNameText;
    public TMP_Text emailText;
    public TMP_Text lastLoginText;
    public TMP_Text userTypeText;
    public RawImage profileImage;

    [Header("History Log UI")]
    public Transform historyContentParent; // Assign ScrollView/Viewport/Content here
    public GameObject historyEntryPrefab;  // Assign a prefab with TMP_Text child

    [Header("No User UI (Optional)")]
    public GameObject noUserPanel; // Panel to show when no user is logged in

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // Subscribe to auth state changes
        auth.StateChanged += OnAuthStateChanged;

        // Check current user status
        CheckUserLoginStatus();
    }

    private void OnDestroy()
    {
        // Unsubscribe from auth state changes to prevent memory leaks
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }

    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        CheckUserLoginStatus();
    }

    private void CheckUserLoginStatus()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            Debug.Log($"User is logged in: {user.Email}");

            // Hide no user panel if it exists
            if (noUserPanel != null)
                noUserPanel.SetActive(false);

            // Load user data
            LoadUserInfo();
            LoadUserLogs();
        }
        else
        {
            Debug.Log("No user is logged in");

            // Show no user panel if it exists
            if (noUserPanel != null)
                noUserPanel.SetActive(true);

            ClearUserData();
        }
    }

    public void LoadUserInfo()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No user is logged in.");
            return;
        }

        DocumentReference userDocRef = firestore.Collection("Students").Document(user.UserId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                Dictionary<string, object> userData = task.Result.ToDictionary();

                classCodeText.text = userData.ContainsKey("classCode") ? userData["classCode"].ToString() : "N/A";
                createdAtText.text = userData.ContainsKey("createdAt") ? ConvertTimestamp(userData["createdAt"]) : "N/A";
                displayNameText.text = userData.ContainsKey("displayName") ? userData["displayName"].ToString() : "N/A";
                emailText.text = userData.ContainsKey("email") ? userData["email"].ToString() : "N/A";
                lastLoginText.text = userData.ContainsKey("lastLogin") ? ConvertTimestamp(userData["lastLogin"]) : "N/A";
                userTypeText.text = userData.ContainsKey("userType") ? userData["userType"].ToString() : "N/A";

                if (userData.ContainsKey("profileImageUrl"))
                {
                    string imageUrl = userData["profileImageUrl"].ToString();
                    StartCoroutine(LoadProfileImage(imageUrl));
                }

                Debug.Log("User info loaded successfully");
            }
            else
            {
                Debug.LogWarning("Failed to retrieve user info or document doesn't exist.");
            }
        });
    }

    public void LoadUserLogs()
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No user is logged in.");
            return;
        }

        CollectionReference logsRef = firestore.Collection("Students").Document(user.UserId).Collection("LogsVR");
        logsRef.OrderByDescending("timestamp").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to fetch user logs.");
                return;
            }

            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError("Task did not complete successfully.");
                return;
            }

            // Clear existing history entries
            foreach (Transform child in historyContentParent)
            {
                Destroy(child.gameObject);
            }

            QuerySnapshot snapshot = task.Result;
            int logCount = 0;

            foreach (DocumentSnapshot logDoc in snapshot.Documents)
            {
                Dictionary<string, object> logData = logDoc.ToDictionary();

                string eventType = logData.ContainsKey("eventType") ? logData["eventType"].ToString() : "Unknown Event";
                string contextInfo = "";

                Debug.Log($"Log Entry: {eventType} - Keys: {string.Join(", ", logData.Keys)}");

                // Determine context details based on eventType
                switch (eventType)
                {
                    case "SceneLoaded":
                        if (logData.TryGetValue("sceneName", out var scene))
                        {
                            contextInfo = ": " + logData["sceneName"].ToString();
                        }
                        else
                        {
                            Debug.LogWarning($"Missing 'sceneName' in log {logDoc.Id}. Keys: {string.Join(", ", logData.Keys)}");
                        }
                        break;

                    case "ARSceneOpened":
                        if (logData.TryGetValue("sceneName", out scene))
                        {
                            contextInfo = ": " + logData["sceneName"].ToString();
                        }
                        else
                        {
                            Debug.LogWarning($"Missing 'sceneName' in log {logDoc.Id}. Keys: {string.Join(", ", logData.Keys)}");
                        }
                        break;

                    case "Crime Scene Opened":
                        if (logData.TryGetValue("sceneName", out scene))
                        {
                            contextInfo = ": " + logData["sceneName"].ToString();
                        }
                        else
                        {
                            Debug.LogWarning($"Missing 'sceneName' in log {logDoc.Id}. Keys: {string.Join(", ", logData.Keys)}");
                        }
                        break;

                    case "ButtonClick":
                        if (logData.TryGetValue("buttonName", out var button))
                        {
                            contextInfo = ": " + button.ToString();
                        }
                        else
                        {
                            Debug.LogWarning($"Missing 'buttonName' in log {logDoc.Id}");
                        }
                        break;

                    case "AppQuit":
                    case "App Quit":
                        // No additional context needed
                        break;

                    case "App Started":
                        if (logData.TryGetValue("platform", out var platform))
                        {
                            contextInfo = $": {platform}";
                        }
                        break;

                    case "Tool Used":
                        if (logData.TryGetValue("tool", out var tool))
                        {
                            contextInfo = ": " + tool.ToString();
                        }
                        break;

                    case "Model Selected":
                        if (logData.TryGetValue("model", out var model))
                        {
                            contextInfo = ": " + model.ToString();
                        }
                        break;

                    default:
                        Debug.Log($"Unhandled eventType: {eventType} in log {logDoc.Id}");
                        break;
                }

                string timestamp = logData.ContainsKey("timestamp")
                    ? ((Timestamp)logData["timestamp"]).ToDateTime().ToLocalTime().ToString("g")
                    : "Unknown Time";

                GameObject entryGO = Instantiate(historyEntryPrefab, historyContentParent);
                TMP_Text entryText = entryGO.GetComponentInChildren<TMP_Text>();
                if (entryText != null)
                    entryText.text = $"{eventType}{contextInfo} - {timestamp}";

                logCount++;
            }

            Debug.Log($"Loaded {logCount} log entries");
        });
    }

    private void ClearUserData()
    {
        // Clear all UI elements when no user is logged in
        if (classCodeText != null) classCodeText.text = "";
        if (createdAtText != null) createdAtText.text = "";
        if (displayNameText != null) displayNameText.text = "";
        if (emailText != null) emailText.text = "";
        if (lastLoginText != null) lastLoginText.text = "";
        if (userTypeText != null) userTypeText.text = "";
        if (profileImage != null) profileImage.texture = null;

        // Clear history entries
        if (historyContentParent != null)
        {
            foreach (Transform child in historyContentParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    // Public method to manually refresh data (useful for account managers)
    public void RefreshUserData()
    {
        CheckUserLoginStatus();
    }

    private string ConvertTimestamp(object ts)
    {
        try
        {
            return ((Timestamp)ts).ToDateTime().ToLocalTime().ToString("g");
        }
        catch
        {
            return "Invalid Timestamp";
        }
    }

    System.Collections.IEnumerator LoadProfileImage(string url)
    {
        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to load profile image: " + uwr.error);
            }
            else
            {
                Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                profileImage.texture = tex;
            }
        }
    }
}