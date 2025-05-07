using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // For TextMeshPro
using System.Collections; // For IEnumerator

public class SceneLoader : MonoBehaviour
{
    public TMP_Text loadingText; // TextMeshPro text for loading messages
    public Slider loadingSlider; // Slider for loading progress
    public GameObject rotatingObject; // Rotating object

    void Start()
    {
        // Start loading the scene stored in SceneData
        StartCoroutine(LoadSceneAsync(SceneData.sceneToLoad));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // While the scene is loading
        while (!operation.isDone)
        {
            // Rotate the object
            rotatingObject.transform.Rotate(Vector3.up, 50f * Time.deltaTime);

            // Calculate the loading progress (0 to 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Update the slider and text with the progress
            loadingSlider.value = progress;
            loadingText.text = "Loading... " + (progress * 100).ToString("F0") + "%";

            // Wait for the next frame before continuing the loop
            yield return null;
        }
    }
}
