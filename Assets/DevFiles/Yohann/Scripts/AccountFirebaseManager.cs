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
using System;
using System.Text.RegularExpressions;
using System.Linq;

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
    public TMP_InputField SignupCompany; // NEW: Company input field
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

    [Header("Logout Confirmation Elements")]
    public GameObject logoutConfirmationPanel;
    public Button confirmLogoutButton;  // "Yes" button
    public Button cancelLogoutButton;   // "No" button

    [Header("Profile UI")]
    public TextMeshProUGUI usernameText;
    public RawImage profileImage;
    public TextMeshProUGUI userType;
    public TextMeshProUGUI classroomCodeText;
    public Texture2D defaultProfileImage;

    [Header("Company Settings")]
    [SerializeField] private bool requireCompanyInput = true; // NEW: Option to make company field required
    [SerializeField] private string defaultCompany = "University of the Cordilleras"; // NEW: Default company if not provided

    [Header("Validation Settings")]
    [SerializeField] private int minPasswordLength = 6;
    [SerializeField] private bool enableDebugLogging = false;

    private const int MIN_PASSWORD_LENGTH = 6;
    private const string PASSWORD_ERROR_MESSAGE = "Password must be at least {0} characters long";
    private const int MaxRetryAttempts = 3;
    private const string EMAIL_REGEX = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
    private const string COLLECTION_USERS = "users";
    private const string COLLECTION_CLASSES = "classes";

    private FirebaseAuth auth;
    private FirebaseFirestore db; // NEW: Added Firestore reference

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

    // NEW: Enhanced email validation
    private bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowNotification("Email address is required", NotificationType.Error);
            return false;
        }

        if (!Regex.IsMatch(email, EMAIL_REGEX))
        {
            ShowNotification("Please enter a valid email address", NotificationType.Error);
            return false;
        }

        return true;
    }

    // NEW: Company validation method
    private bool ValidateCompany(string company)
    {
        if (requireCompanyInput && string.IsNullOrWhiteSpace(company))
        {
            ShowNotification("Company name is required", NotificationType.Error);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(company) && company.Trim().Length < 2)
        {
            ShowNotification("Company name must be at least 2 characters long", NotificationType.Error);
            return false;
        }

        return true;
    }

    // NEW: Helper method to get company value with fallback
    private string GetCompanyValue()
    {
        string companyInput = SignupCompany?.text?.Trim();

        if (!string.IsNullOrWhiteSpace(companyInput))
        {
            return companyInput;
        }

        return !string.IsNullOrWhiteSpace(defaultCompany) ? defaultCompany : "University of the Cordilleras";
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (fadeText == null)
        {
            fadeText = GetComponent<FadeTextScript>();
        }
        if (confirmLogoutButton != null)
            confirmLogoutButton.onClick.AddListener(ConfirmLogout);

        if (cancelLogoutButton != null)
            cancelLogoutButton.onClick.AddListener(CancelLogout);

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
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance; // NEW: Initialize Firestore

                var user = auth.CurrentUser;

                if (user != null && user.IsEmailVerified)
                {
                    if (enableDebugLogging)
                    {
                        Debug.Log("User logged in and verified. Showing statistics screen...");
                    }
                    DeactivateAllScreens();
                    LoadUserProfile(user.UserId);
                    statisticsScreen.SetActive(true);
                }
                else
                {
                    if (enableDebugLogging)
                    {
                        Debug.Log("No user logged in or email not verified. Showing login UI...");
                    }
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
        AuthError.WeakPassword => "Password is too weak. Please use a stronger password.",
        AuthError.InvalidEmail => "Please enter a valid email address.",
        AuthError.EmailAlreadyInUse => "This email address is already registered.",
        AuthError.UserNotFound => "No account found with this email address.",
        AuthError.WrongPassword => "Incorrect password. Please try again.",
        AuthError.TooManyRequests => "Too many failed attempts. Please try again later.",
        AuthError.UserDisabled => "This account has been disabled. Please contact support.",
        AuthError.NetworkRequestFailed => "Network error. Please check your internet connection.",
        _ => "Authentication failed. Please try again or contact support if the problem persists."
    };

    // NEW: Enhanced validation for signup inputs
    private bool ValidateSignupInputs()
    {
        if (string.IsNullOrWhiteSpace(SignupFirstName?.text))
        {
            ShowNotification("First name is required", NotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(SignupLastName?.text))
        {
            ShowNotification("Last name is required", NotificationType.Error);
            return false;
        }

        if (SignupPassword.text != SignupPasswordConfirm.text)
        {
            ShowNotification("Passwords do not match", NotificationType.Error);
            return false;
        }

        return ValidateEmail(SignupEmail.text) &&
               ValidatePassword(SignupPassword.text) &&
               ValidateCompany(SignupCompany?.text); // NEW: Added company validation
    }

    public async void SignUp()
    {
        if (!ValidateSignupInputs()) return;

        DeactivateAllScreens();
        loadingScreen.SetActive(true);

        try
        {
            string email = SignupEmail.text.Trim();
            string password = SignupPassword.text.Trim();
            string firstName = SignupFirstName.text.Trim();
            string lastName = SignupLastName.text.Trim();
            string company = GetCompanyValue(); // NEW: Get company value

            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            string fullName = $"{firstName} {lastName}".Trim();
            await result.User.UpdateUserProfileAsync(new UserProfile { DisplayName = fullName });

            // Send verification email
            await result.User.SendEmailVerificationAsync();

            // NEW: Create user document with new schema
            await CreateUserDocument(result.User, firstName, lastName, company);

            // Clear form
            ClearSignupFields();

            ShowNotification("Account created successfully! Please check your email for verification.", NotificationType.Success);

            DeactivateAllScreens();
            loginUi.SetActive(true);
        }
        catch (FirebaseException ex)
        {
            var error = (AuthError)ex.ErrorCode;
            string errorMsg = GetErrorMessage(error);
            StartCoroutine(ShowLoadingAndReturnToSignup(errorMsg));
        }
        finally
        {
            loadingScreen.SetActive(false);
        }
    }

    // NEW: Create user document with new schema
    private async Task CreateUserDocument(FirebaseUser user, string firstName, string lastName, string company)
    {
        string[] userTypes = { "Student", "Teacher", "Faculty Staff" };
        string selectedUserType = userTypes[Mathf.Clamp(userTypeDropdown.value, 0, userTypes.Length - 1)];

        var userData = new Dictionary<string, object>
        {
            { "company", company }, // NEW: Use the provided company value
            { "createdAt", Timestamp.GetCurrentTimestamp() },
            { "email", user.Email },
            { "enrolledClasses", new List<string>() }, // Empty array initially
            { "firstName", firstName },
            { "lastName", lastName },
            { "role", selectedUserType },
            { "uid", user.UserId }
        };

        DocumentReference docRef = db.Collection(COLLECTION_USERS).Document(user.UserId);
        await docRef.SetAsync(userData, SetOptions.MergeAll);

        if (enableDebugLogging)
        {
            Debug.Log($"User document created for {firstName} {lastName} at {company} with role {selectedUserType}");
        }
    }

    // NEW: Enhanced class code validation
    private async Task<bool> ValidateClassCode()
    {
        // If no class code input field or empty input, treat as optional
        if (classCodeInput == null || string.IsNullOrWhiteSpace(classCodeInput.text))
        {
            if (enableDebugLogging)
            {
                Debug.Log("No class code provided - proceeding without class validation");
            }
            return true; // Allow login without class code
        }

        if (db == null)
        {
            ShowNotification("Database not initialized. Please try again.", NotificationType.Error);
            return false;
        }

        try
        {
            string classCode = classCodeInput.text.Trim();
            string email = LoginEmail.text.Trim();

            if (enableDebugLogging)
            {
                Debug.Log($"Validating class code: {classCode} for user: {email}");
            }

            // STEP 1: Find the class by code
            Query classQuery = db.Collection(COLLECTION_CLASSES)
                .WhereEqualTo("code", classCode)
                .Limit(1);

            QuerySnapshot classQuerySnapshot = await classQuery.GetSnapshotAsync();

            if (classQuerySnapshot.Count == 0)
            {
                ShowNotification("Invalid class code. Please check and try again.", NotificationType.Error);
                return false;
            }

            DocumentSnapshot classSnapshot = classQuerySnapshot.Documents.First();
            string classDocumentId = classSnapshot.Id;

            // STEP 2: Check if user exists and is enrolled in this class
            Query userQuery = db.Collection(COLLECTION_USERS)
                .WhereEqualTo("email", email)
                .Limit(1);

            QuerySnapshot userQuerySnapshot = await userQuery.GetSnapshotAsync();

            if (userQuerySnapshot.Count == 0)
            {
                ShowNotification("User not found. Please check your email.", NotificationType.Error);
                return false;
            }

            DocumentSnapshot userSnapshot = userQuerySnapshot.Documents.First();

            if (!userSnapshot.ContainsField("enrolledClasses"))
            {
                ShowNotification("You are not enrolled in any classes.", NotificationType.Warning);
                return false;
            }

            var enrolledClassesArray = userSnapshot.GetValue<object[]>("enrolledClasses");
            List<string> enrolledClassIds = enrolledClassesArray?.Select(obj => obj.ToString()).ToList() ?? new List<string>();

            if (!enrolledClassIds.Contains(classDocumentId))
            {
                ShowNotification("You are not enrolled in this class.", NotificationType.Warning);
                return false;
            }

            // Additional class validation (active status, due date, etc.)
            if (classSnapshot.ContainsField("isActive"))
            {
                bool isActive = classSnapshot.GetValue<bool>("isActive");
                if (!isActive)
                {
                    ShowNotification("This class is currently inactive.", NotificationType.Warning);
                    return false;
                }
            }

            if (classSnapshot.ContainsField("dueDate"))
            {
                var dueDate = classSnapshot.GetValue<Timestamp>("dueDate");
                if (dueDate.ToDateTime() < DateTime.UtcNow)
                {
                    ShowNotification("This class has expired and is no longer accepting new students.", NotificationType.Warning);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            ShowNotification("Error validating class code. Please try again.", NotificationType.Error);
            if (enableDebugLogging)
            {
                Debug.LogError($"Class code validation error: {ex.Message}");
            }
            return false;
        }
    }

    public async void Login()
    {
        if (!ValidateEmail(LoginEmail.text) || string.IsNullOrWhiteSpace(loginPassword.text))
            return;

        // ✅ VALIDATE CLASS CODE FIRST (before authentication)
        if (!await ValidateClassCode())
        {
            // Class code validation failed - don't proceed with authentication
            return;
        }

        DeactivateAllScreens();
        loadingScreen.SetActive(true);

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(LoginEmail.text.Trim(), loginPassword.text.Trim());

            if (result.User.IsEmailVerified)
            {
                ShowNotification("Log in Successful", NotificationType.Success);
                successDescriptionText.text = "Welcome back!\nUser ID: " + result.User.UserId;
                LoadUserProfile(result.User.UserId);

                DeactivateAllScreens();
                statisticsScreen.SetActive(true);
            }
            else
            {
                ShowNotification("Please verify your email!", NotificationType.Warning);
                await result.User.SendEmailVerificationAsync();
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
        finally
        {
            loadingScreen.SetActive(false);
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
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[AccountFirebaseManager] Logout button pressed. Asking for confirmation for user: {auth.CurrentUser.Email}");
            }
            logoutConfirmationPanel.SetActive(true);
        }
    }

    public void ConfirmLogout()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[AccountFirebaseManager] Confirming logout.");
        }
        auth.SignOut();
        DisplayGuestProfile();
        PlayerPrefs.SetInt("AutoLogin", 0);
        PlayerPrefs.Save();

        // Clear all input fields
        ClearAllInputFields();

        logoutConfirmationPanel.SetActive(false);
        ShowLoginUI();
    }

    public void CancelLogout()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[AccountFirebaseManager] Logout canceled.");
        }
        logoutConfirmationPanel.SetActive(false);
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
            if (enableDebugLogging)
            {
                Debug.Log($"Logout text color set to: {logoutText.color}");
            }
        }
    }

    private void DisplayGuestProfile()
    {
        if (enableDebugLogging)
        {
            Debug.Log("Displaying guest profile.");
        }
        usernameText.text = "Guest";
        userType.text = "Guest Account";
        classroomCodeText.text = "No class assigned";
        profileImage.texture = defaultProfileImage;
    }

    private async void LoadUserProfile(string userId)
    {
        try
        {
            DocumentReference docRef = db.Collection(COLLECTION_USERS).Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Get basic user info
                string firstName = snapshot.TryGetValue("firstName", out string fn) ? fn : "";
                string lastName = snapshot.TryGetValue("lastName", out string ln) ? ln : "";
                string fullName = $"{firstName} {lastName}".Trim();
                if (string.IsNullOrEmpty(fullName)) fullName = "Guest";

                string role = snapshot.TryGetValue("role", out string r) ? r : "Registered Account";
                string company = snapshot.TryGetValue("company", out string c) ? c : "";

                // Update basic profile info
                usernameText.text = fullName;
                userType.text = role;
                profileImage.texture = defaultProfileImage;

                // FIXED: Always prioritize showing class names over company
                if (snapshot.ContainsField("enrolledClasses"))
                {
                    var enrolledClassesArray = snapshot.GetValue<object[]>("enrolledClasses");
                    List<string> enrolledClassIds = enrolledClassesArray?.Select(obj => obj.ToString()).ToList() ?? new List<string>();

                    if (enrolledClassIds.Count > 0)
                    {
                        // Show loading state while fetching class names
                        classroomCodeText.text = "Loading classes...";

                        // Fetch actual class names
                        List<string> classNames = await GetClassNamesByIds(enrolledClassIds);

                        if (classNames.Count > 0)
                        {
                            // Display class names (limit to prevent UI overflow)
                            if (classNames.Count <= 3)
                            {
                                classroomCodeText.text = string.Join(", ", classNames);
                            }
                            else
                            {
                                classroomCodeText.text = $"{string.Join(", ", classNames.Take(2))}, +{classNames.Count - 2} more";
                            }
                        }
                        else
                        {
                            classroomCodeText.text = $"Enrolled in {enrolledClassIds.Count} class(es) (names unavailable)";
                        }

                        if (enableDebugLogging)
                        {
                            Debug.Log($"User enrolled in classes: {string.Join(", ", classNames)}");
                        }
                    }
                    else
                    {
                        // Fallback to company if no classes are enrolled
                        classroomCodeText.text = !string.IsNullOrEmpty(company) ? company : "No classes enrolled";
                    }
                }
                else
                {
                    // Fallback to company if no enrolledClasses field exists
                    classroomCodeText.text = !string.IsNullOrEmpty(company) ? company : "No class assigned";
                }
            }
            else
            {
                DisplayGuestProfile();
            }
        }
        catch (Exception ex)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error loading user profile: {ex.Message}");
            }
            DisplayGuestProfile();
        }
    }
    private System.Collections.IEnumerator LoadProfilePicture(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            profileImage.texture = texture;
            if (enableDebugLogging)
            {
                Debug.Log("Profile picture successfully loaded.");
            }
        }
        else
        {
            Debug.LogWarning("Failed to load profile picture: " + request.error);
            profileImage.texture = defaultProfileImage;
        }
    }

    // NEW: Clear all input fields method
    private void ClearAllInputFields()
    {
        ClearLoginFields();
        ClearSignupFields();
    }

    private void ClearLoginFields()
    {
        if (LoginEmail != null) LoginEmail.text = "";
        if (loginPassword != null) loginPassword.text = "";
        if (classCodeInput != null) classCodeInput.text = "";
    }

    private void ClearSignupFields()
    {
        if (SignupEmail != null) SignupEmail.text = "";
        if (SignupPassword != null) SignupPassword.text = "";
        if (SignupPasswordConfirm != null) SignupPasswordConfirm.text = "";
        if (SignupFirstName != null) SignupFirstName.text = "";
        if (SignupLastName != null) SignupLastName.text = "";
        if (SignupCompany != null) SignupCompany.text = ""; // NEW: Clear company field
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
        if (enableDebugLogging)
        {
            Debug.Log("Application has been closed.");
        }
    }

    private void CloseAllPanels()
    {
        startPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
        profileLoginPanel.SetActive(false);
    }

    // NEW: Additional utility methods from the AR script

    // NEW: Method to set default company
    public void SetDefaultCompany()
    {
        if (SignupCompany != null && string.IsNullOrWhiteSpace(SignupCompany.text))
        {
            SignupCompany.text = defaultCompany;
        }
    }

    // NEW: Method to get user's enrolled classes
    public async Task<List<string>> GetEnrolledClasses()
    {
        if (auth.CurrentUser == null) return new List<string>();

        try
        {
            DocumentReference userDocRef = db.Collection(COLLECTION_USERS).Document(auth.CurrentUser.UserId);
            DocumentSnapshot userSnapshot = await userDocRef.GetSnapshotAsync();

            if (userSnapshot.Exists && userSnapshot.ContainsField("enrolledClasses"))
            {
                var enrolledClassesArray = userSnapshot.GetValue<object[]>("enrolledClasses");
                return enrolledClassesArray?.Select(obj => obj.ToString()).ToList() ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error getting enrolled classes: {ex.Message}");
            }
        }

        return new List<string>();
    }
    private async Task<List<string>> GetClassNamesByIds(List<string> classIds)
    {
        List<string> classNames = new List<string>();

        try
        {
            // Create tasks for all class document fetches
            List<Task<DocumentSnapshot>> tasks = new List<Task<DocumentSnapshot>>();

            foreach (string classId in classIds)
            {
                DocumentReference classDocRef = db.Collection(COLLECTION_CLASSES).Document(classId);
                tasks.Add(classDocRef.GetSnapshotAsync());
            }

            // Wait for all requests to complete
            DocumentSnapshot[] snapshots = await Task.WhenAll(tasks);

            // Extract class names from snapshots
            foreach (DocumentSnapshot snapshot in snapshots)
            {
                if (snapshot.Exists && snapshot.ContainsField("name"))
                {
                    string className = snapshot.GetValue<string>("name");
                    if (!string.IsNullOrEmpty(className))
                    {
                        classNames.Add(className);
                    }
                }
                else if (enableDebugLogging)
                {
                    Debug.LogWarning($"Class document {snapshot.Id} not found or missing 'name' field");
                }
            }
        }
        catch (Exception ex)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error fetching class names: {ex.Message}");
            }
        }

        return classNames;
    }

    #region Data Classes

    [System.Serializable]
    public class UserData
    {
        public string company;
        public string email;
        public List<string> enrolledClasses;
        public string firstName;
        public string lastName;
        public string role;
        public string uid;
    }

    [System.Serializable]
    public class ClassData
    {
        public string code;
        public string documentId;
        public string name;
        public string description;
        public string instructorId;
        public string assignmentType;
        public string type;
        public string title;
        public string linkedCrimeSceneId;
        public string linkedCrimeSceneName;
        public Timestamp? dueDate;
        public bool autoAssessOnLogin;
        public Timestamp? createdAt;
        public List<string> students;
    }

    #endregion
}