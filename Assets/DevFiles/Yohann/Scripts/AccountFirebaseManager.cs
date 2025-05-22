
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccountFirebaseManager : MonoBehaviour
{
    [Header("Screen Elements")]
    public GameObject startPanel;
    public GameObject settingsPanel;
    public GameObject profilePanel;
    public GameObject profileLoginPanel;
    public GameObject loadingScreen;
    public GameObject loginUi;
    public GameObject signupUi;
    public GameObject SuccessUi;
    public GameObject statisticsScreen;

    [Header("UI Elements")]
    public TMP_InputField LoginEmail;
    public TMP_InputField loginPassword;
    public TMP_InputField SignupEmail;
    public TMP_InputField SignupPassword;
    public TMP_InputField SignupPasswordConfirm;
    public TMP_InputField SignupFirstName;
    public TMP_InputField SignupLastName;
    public TMP_Dropdown userTypeDropdown;
    public TMP_InputField classCodeInput;
    public TextMeshProUGUI logTxt;
    public TextMeshProUGUI successDescriptionText;

    [Header("Notification Settings")]
    public FadeTextScript fadeText;
    public float notificationDuration = 3.0f;
    public Color errorColor = Color.red;
    public Color successColor = Color.green;
    public Color warningColor = Color.yellow;

    [Header("Success Description")]
    public TextMeshProUGUI succesDescriptionText;

    [Header("Logout UI")]
    public GameObject logoutButton;
    public TextMeshProUGUI logoutText;

    [Header("Profile UI")]
    public TextMeshProUGUI usernameText;
    public RawImage profileImage;
    public TextMeshProUGUI userType;
    public TextMeshProUGUI classroomCodeText;
    public Texture2D defaultProfileImage;

    private const int MIN_PASSWORD_LENGTH = 6;
    private const string PASSWORD_ERROR_MESSAGE = "Password must be at least {0} characters long";
    private const int MaxRetryAttempts = 3;

    private FirebaseAuth auth;

    private enum NotificationType { Error, Success, Warning }

    private void ShowNotification(string message, NotificationType type)
    {
        logTxt.text = message;
        logTxt.color = type switch
        {
            NotificationType.Error => errorColor,
            NotificationType.Success => successColor,
            NotificationType.Warning => warningColor,
            _ => Color.white
        };
        fadeText.StartFade();
    }

    private bool ValidatePassword(string password) => password.Length >= MIN_PASSWORD_LENGTH;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (fadeText == null)
        {
            fadeText = GetComponent<FadeTextScript>();
        }

        DeactivateAllScreens();
        loadingScreen.SetActive(true);
        InitializeFirebaseAndCheckUser();
    }

    private void DeactivateAllScreens()
    {
        startPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
        profileLoginPanel.SetActive(false);
        loadingScreen.SetActive(false);
        loginUi.SetActive(false);
        signupUi.SetActive(false);
        SuccessUi.SetActive(false);
        statisticsScreen.SetActive(false);
    }

    private async void InitializeFirebaseAndCheckUser()
    {
        int retryCount = 0;
        const int maxRetries = 3;

        while (retryCount < maxRetries)
        {
            var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
            await dependencyTask;

            if (dependencyTask.Result == DependencyStatus.Available)
            {
                var user = auth.CurrentUser;

                if (user != null && user.IsEmailVerified)
                {
                    Debug.Log("User logged in and verified. Showing statistics screen...");
                    DeactivateAllScreens();
                    LoadUserProfile(user.UserId);
                    statisticsScreen.SetActive(true);
                }
                else
                {
                    Debug.Log("No user logged in or email not verified. Showing login UI...");
                    DeactivateAllScreens();
                    loginUi.SetActive(true);
                }
                return;
            }
            else
            {
                retryCount++;
                ShowNotification($"Firebase init failed: {dependencyTask.Result}. Retrying... ({retryCount}/{maxRetries})", NotificationType.Warning);
                await Task.Delay(2000);
            }
        }

        ShowNotification("Unable to initialize Firebase. Please check your internet connection or reinstall the app.", NotificationType.Error);
        DeactivateAllScreens();
    }
    private System.Collections.IEnumerator ShowLoadingAndReturnToSignup(string errorMessage)
    {
        DeactivateAllScreens();
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(1.5f); // Show loading for 1.5 seconds

        loadingScreen.SetActive(false);
        signupUi.SetActive(true);
        ShowNotification(errorMessage, NotificationType.Error);
    }
    private string GetErrorMessage(AuthError error) => error switch
    {
        AuthError.WeakPassword => "Password is too weak",
        AuthError.InvalidEmail => "Invalid email address",
        AuthError.EmailAlreadyInUse => "This email is already in use",
        _ => "Wrong pass code or an unknown error occurred"
    };

    public async void SignUp()
    {
        // Input Validation - Weak Password
        if (!ValidatePassword(SignupPassword.text))
        {
            StartCoroutine(ShowLoadingAndReturnToSignup(string.Format(PASSWORD_ERROR_MESSAGE, MIN_PASSWORD_LENGTH)));
            return;
        }

        // Input Validation - Password Mismatch
        if (SignupPassword.text != SignupPasswordConfirm.text)
        {
            StartCoroutine(ShowLoadingAndReturnToSignup("Passwords do not match"));
            return;
        }

        DeactivateAllScreens();
        loadingScreen.SetActive(true);

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(SignupEmail.text.Trim(), SignupPassword.text.Trim());

            string fullName = $"{SignupFirstName.text.Trim()} {SignupLastName.text.Trim()}".Trim();
            await result.User.UpdateUserProfileAsync(new UserProfile { DisplayName = fullName });

            SignupEmail.text = SignupPassword.text = SignupPasswordConfirm.text = "";
            SignupFirstName.text = SignupLastName.text = "";

            if (result.User.IsEmailVerified)
            {
                ShowNotification("Sign up Successful", NotificationType.Success);
            }
            else
            {
                ShowNotification("A verification email has been sent to activate your account", NotificationType.Warning);
                await result.User.SendEmailVerificationAsync();
            }

            string[] userTypes = { "Student", "Teacher", "Faculty Staff" };
            string selectedUserType = userTypes[Mathf.Clamp(userTypeDropdown.value, 0, userTypes.Length - 1)];

            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection("Students").Document(result.User.UserId);

            Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "displayName", string.IsNullOrWhiteSpace(fullName) ? "Guest" : fullName },
            { "email", result.User.Email },
            { "userType", selectedUserType },
            { "profileImageUrl", result.User.PhotoUrl?.ToString() ?? "" },
            { "createdAt", Timestamp.GetCurrentTimestamp() }
        };

            await docRef.SetAsync(userData, SetOptions.MergeAll);
        }
        catch (FirebaseException ex)
        {
            var error = (AuthError)ex.ErrorCode;
            string errorMsg = GetErrorMessage(error);
            StartCoroutine(ShowLoadingAndReturnToSignup(errorMsg));
        }
        finally
        {
            loadingScreen.SetActive(false); // Ensure it's deactivated in case success or early exit missed it
        }
    }
    public async void Login()
    {
        DeactivateAllScreens();
        loadingScreen.SetActive(true);
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(LoginEmail.text.Trim(), loginPassword.text.Trim());

            if (result.User.IsEmailVerified)
            {
                string enteredClassCode = classCodeInput.text.Trim();
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                DocumentReference classDocRef = db.Collection("Classes").Document(enteredClassCode);
                DocumentSnapshot classSnapshot = await classDocRef.GetSnapshotAsync();

                if (!classSnapshot.Exists)
                {
                    ShowNotification("Invalid class code. Please try again.", NotificationType.Error);
                    DeactivateAllScreens();
                    loginUi.SetActive(true);
                    return;
                }

                ShowNotification("Log in Successful", NotificationType.Success);
                successDescriptionText.text = "Id: " + result.User.UserId;
                LoadUserProfile(result.User.UserId);
                DocumentReference userDocRef = db.Collection("Students").Document(result.User.UserId);
                await userDocRef.SetAsync(new Dictionary<string, object>
                {
                    { "classCode", enteredClassCode }
                }, SetOptions.MergeAll);

                DeactivateAllScreens();
                statisticsScreen.SetActive(true);
            }
            else
            {
                ShowNotification("Please verify your email!", NotificationType.Warning);
                DeactivateAllScreens();
                loginUi.SetActive(true);
            }
        }
        catch (FirebaseException ex)
        {
            var error = (AuthError)ex.ErrorCode;
            ShowNotification(GetErrorMessage(error), NotificationType.Error);
            DeactivateAllScreens();
            loginUi.SetActive(true);
        }
    }

    public void OnProfileButtonPressed()
    {
        var user = auth.CurrentUser;
        if (user == null || !user.IsEmailVerified)
        {
            ShowNotification("Please log in or verify your email.", NotificationType.Warning);
            ShowLoginUI();
        }
        else
        {
            ShowStatisticsScreen();
        }
    }

    public void OnLogoutButtonPressed()
    {
        if (auth.CurrentUser != null)
            Debug.Log($"Logging out user: {auth.CurrentUser.Email}");

        auth.SignOut();
        DisplayGuestProfile();
        PlayerPrefs.SetInt("AutoLogin", 0);
        PlayerPrefs.Save();

        // Clear login inputs
        LoginEmail.text = "";
        loginPassword.text = "";

        // Clear signup inputs (optional but helpful)
        SignupEmail.text = "";
        SignupPassword.text = "";
        SignupPasswordConfirm.text = "";
        SignupFirstName.text = "";
        SignupLastName.text = "";
        classCodeInput.text = "";

        ShowLoginUI();
    }


    private void ShowLoginUI()
    {
        DeactivateAllScreens();
        loginUi.SetActive(true);
    }

    private void ShowStatisticsScreen()
    {
        DeactivateAllScreens();
        statisticsScreen.SetActive(true);
    }

    private void UpdateLogoutUI()
    {
        var user = auth.CurrentUser;
        bool showButton = user != null && user.IsEmailVerified;

        if (logoutButton != null)
            logoutButton.SetActive(showButton);

        if (logoutText != null)
        {
            logoutText.color = showButton ? new Color32(255, 255, 255, 255) : new Color32(128, 128, 128, 255);
            logoutText.text = showButton ? "Logout" : "";
            logoutText.ForceMeshUpdate();
            Debug.Log($"Logout text color set to: {logoutText.color}");
        }
    }

    private void DisplayGuestProfile()
    {
        Debug.Log("Displaying guest profile.");
        usernameText.text = "Guest";
        userType.text = "Guest Account";
        classroomCodeText.text = "No class assigned";
        profileImage.texture = defaultProfileImage;
    }


    private void LoadUserProfile(string userId)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("Students").Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(async task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                var snapshot = task.Result;
                string name = snapshot.TryGetValue("displayName", out string n) ? n : "Guest";
                string type = snapshot.TryGetValue("userType", out string t) ? t : "Registered Account";
                string imageUrl = snapshot.TryGetValue("profileImageUrl", out string url) ? url : "";
                string classCode = snapshot.TryGetValue("classCode", out string cc) ? cc : "";

                usernameText.text = name;
                userType.text = type;

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    Debug.Log("Loading profile image from: " + imageUrl);
                    StartCoroutine(LoadProfilePicture(imageUrl));
                }
                else
                {
                    profileImage.texture = defaultProfileImage;
                }

                if (!string.IsNullOrEmpty(classCode))
                {
                    var classDoc = await db.Collection("Classes").Document(classCode).GetSnapshotAsync();
                    classroomCodeText.text = classDoc.Exists && classDoc.ContainsField("className")
                        ? classDoc.GetValue<string>("className")
                        : "Class not found";
                }
                else
                {
                    classroomCodeText.text = "No class assigned";
                }
            }
            else
            {
                DisplayGuestProfile();
            }
        });
    }



    private System.Collections.IEnumerator LoadProfilePicture(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            profileImage.texture = texture;
            Debug.Log("Profile picture successfully loaded.");
        }
        else
        {
            Debug.LogWarning("Failed to load profile picture: " + request.error);
            profileImage.texture = defaultProfileImage;
        }
    }

    public void OpenStartPanel()
    {
        CloseAllPanels();
        startPanel.SetActive(true);
    }
    public void OpenSignUpPanel()
    {
        CloseAllPanels();
        signupUi.SetActive(true);
    }
    public void OpenLogInPanel()
    {
        DeactivateAllScreens();
        loginUi.SetActive(true);
    }

    public void OpenSettingsPanel()
    {
        CloseAllPanels();
        settingsPanel.SetActive(true);
    }

    public void ExitApplication()
    {
        Application.Quit();
        Debug.Log("Application has been closed.");
    }

    private void CloseAllPanels()
    {
        startPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
        profileLoginPanel.SetActive(false);
    }
}
