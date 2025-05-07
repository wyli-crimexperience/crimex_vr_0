using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectSceneManager : MonoBehaviour
{
    public Button scene1Button;
    public Button scene2Button;
    public Button scene3Button;
    public Button scene4Button;
    public Button scene5Button;
    public Button scene6Button;
    public Button scene7Button;
    public Toggle practiceModeToggle;
    public Toggle tooltipsToggle;
    public Button deployButton;

    private string selectedScene;
    private bool isPracticeMode;
    private bool showTooltips;

    void Start()
    {
        // Add listeners to the scene buttons
        scene1Button.onClick.AddListener(() => OnSceneButtonClicked("Scene1"));
        scene2Button.onClick.AddListener(() => OnSceneButtonClicked("Scene2"));
        scene3Button.onClick.AddListener(() => OnSceneButtonClicked("Scene3"));
        scene4Button.onClick.AddListener(() => OnSceneButtonClicked("Scene4"));
        scene5Button.onClick.AddListener(() => OnSceneButtonClicked("Scene5"));
        scene6Button.onClick.AddListener(() => OnSceneButtonClicked("Scene6"));
        scene7Button.onClick.AddListener(() => OnSceneButtonClicked("Scene7"));

        // Add listeners for toggles
        practiceModeToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(practiceModeToggle); });
        tooltipsToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(tooltipsToggle); });

        // Add listener for deploy button
        deployButton.onClick.AddListener(DeployScene);
    }

    void OnSceneButtonClicked(string scene)
    {
        selectedScene = scene;
        Debug.Log("Selected scene: " + selectedScene);
    }

    void ToggleValueChanged(Toggle change)
    {
        if (change == practiceModeToggle)
        {
            isPracticeMode = change.isOn;
        }
        else if (change == tooltipsToggle)
        {
            showTooltips = change.isOn;
        }
    }

    void DeployScene()
    {
        if (!string.IsNullOrEmpty(selectedScene))
        {
            Debug.Log("Deploying scene: " + selectedScene);
            Debug.Log("Practice Mode: " + isPracticeMode);
            Debug.Log("Show Tooltips: " + showTooltips);

            // Store the selected scene name in SceneData
            SceneData.sceneToLoad = selectedScene;

            // Load the loading screen
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            Debug.LogWarning("No scene selected!");
        }
    }
}
