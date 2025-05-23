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

    [Header("Scene Selection UI")]
    public FadeTextScript fadeText;  // Assign this in Inspector

    private string selectedScene;
    private bool isPracticeMode;
    private bool showTooltips;

    void Start()
    {
        scene1Button.onClick.AddListener(() => OnSceneButtonClicked("Scene1"));
        scene2Button.onClick.AddListener(() => OnSceneButtonClicked("Scene2"));
        scene3Button.onClick.AddListener(() => OnSceneButtonClicked("Scene3"));
        scene4Button.onClick.AddListener(() => OnSceneButtonClicked("Scene4"));
        scene5Button.onClick.AddListener(() => OnSceneButtonClicked("Scene5"));
        scene6Button.onClick.AddListener(() => OnSceneButtonClicked("Scene6"));
        scene7Button.onClick.AddListener(() => OnSceneButtonClicked("Scene7"));

        practiceModeToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(practiceModeToggle); });
        tooltipsToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(tooltipsToggle); });

        deployButton.onClick.AddListener(DeployScene);
    }

    void OnSceneButtonClicked(string scene)
    {
        selectedScene = scene;
        Debug.Log("Selected scene: " + selectedScene);

        if (fadeText != null)
        {
            fadeText.GetComponent<TMPro.TextMeshProUGUI>().text = $"Selected: {selectedScene}";
            fadeText.ResetFade();  // Reset before starting new fade
            fadeText.StartFade();
        }
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

            SceneData.sceneToLoad = selectedScene;
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            Debug.LogWarning("No scene selected!");
        }
    }
}
