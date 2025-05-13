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
    public GameObject leftRay;
    public GameObject rightRay;
    public InputActionReference primaryButtonLeft;

    private bool isPaused = false;

    void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        statisticsButton.onClick.AddListener(ShowStatistics);
        restartButton.onClick.AddListener(RestartGame);
        quitToMainMenuButton.onClick.AddListener(QuitToMainMenu);
        exitToDesktopButton.onClick.AddListener(ExitToDesktop);

        pauseMenuCanvas.gameObject.SetActive(false);

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

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        PositionPauseMenu();
        pauseMenuCanvas.gameObject.SetActive(true);
        leftRay.gameObject.SetActive(true);
        rightRay.gameObject.SetActive(true);
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuCanvas.gameObject.SetActive(false);
        leftRay.gameObject.SetActive(false);
        rightRay.gameObject.SetActive(false);
    }

    void PositionPauseMenu()
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0; // Optional: keep the menu level
        forward.Normalize();

        pauseMenuCanvas.transform.position = playerCamera.transform.position + forward * 0.7f;
        pauseMenuCanvas.transform.rotation = Quaternion.LookRotation(forward);
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
        SceneManager.LoadScene("LobbyArea");
    }

    void ExitToDesktop()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Exiting to desktop...");
    }
}
