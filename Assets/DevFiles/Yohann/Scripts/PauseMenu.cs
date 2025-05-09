using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public Canvas pauseMenuCanvas;
    public Button resumeButton;
    public Button statisticsButton;
    public Button restartButton;
    public Button quitToMainMenuButton;
    public Button exitToDesktopButton;
    public GameObject playerCamera;
    public InputActionReference primaryButtonLeft; // Assign this in the Inspector

    private bool isPaused = false;

    void Start()
    {
        // Add listeners to the buttons
        resumeButton.onClick.AddListener(ResumeGame);
        statisticsButton.onClick.AddListener(ShowStatistics);
        restartButton.onClick.AddListener(RestartGame);
        quitToMainMenuButton.onClick.AddListener(QuitToMainMenu);
        exitToDesktopButton.onClick.AddListener(ExitToDesktop);

        // Initially hide the pause menu
        pauseMenuCanvas.gameObject.SetActive(false);

        // Enable and bind the input action
        if (primaryButtonLeft != null)
        {
            primaryButtonLeft.action.Enable();
            primaryButtonLeft.action.performed += OnPrimaryButtonPressed;
        }
        else
        {
            Debug.LogWarning("Primary Button Left input action is not assigned.");
        }
    }

    void OnDestroy()
    {
        if (primaryButtonLeft != null)
        {
            primaryButtonLeft.action.performed -= OnPrimaryButtonPressed;
            primaryButtonLeft.action.Disable();
        }
    }

    void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void Update()
    {
        if (isPaused)
        {
            pauseMenuCanvas.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2f;
            pauseMenuCanvas.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward);
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuCanvas.gameObject.SetActive(true);
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuCanvas.gameObject.SetActive(false);
    }

    void ShowStatistics()
    {
        Debug.Log("Showing statistics...");
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void ExitToDesktop()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Exiting to desktop...");
    }
}
