using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NavigatorManager : MonoBehaviour
{
    // Singleton instance
    public static NavigatorManager Instance { get; private set; }

    // List to track the scene history
    private static List<string> sceneHistory = new List<string>();

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function to load a specific scene
    public void LoadScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Save the current scene before loading the next one
        if (currentScene != sceneName)
        {
            sceneHistory.Add(currentScene);
            Debug.Log($"Scene added to history: {currentScene}");
        }

        Debug.Log("Current scene history: " + string.Join(", ", sceneHistory));

        // Load the specified scene
        SceneManager.LoadScene(sceneName);
    }

    // Function to go back to the previous scene
    public void GoBackToPreviousScene()
    {
        if (sceneHistory.Count > 0)
        {
            string previousScene = sceneHistory[sceneHistory.Count - 1];
            sceneHistory.RemoveAt(sceneHistory.Count - 1); // Remove the last scene from the history
            Debug.Log($"Going back to previous scene: {previousScene}");
            Debug.Log("Updated scene history: " + string.Join(", ", sceneHistory));

            SceneManager.LoadScene(previousScene);
        }
        else
        {
            Debug.LogWarning("No previous scene to go back to.");
        }
    }

    // Categories
    public void GoToScene1()
    {
        LoadScene("Scene1");
    }
    public void GoToScene2()
    {
        LoadScene("Scene2");
    }
    public void GoToScene3()
    {
        LoadScene("Scene3");
    }
    public void GoToScene4()
    {
        LoadScene("Scene4");
    }
    public void GoToScene5()
    {
        LoadScene("Scene5");
    }
    public void GoToScene6()
    {
        LoadScene("Scene6");
    }
    public void GoToScene7()
    {
        LoadScene("Scene7");
    }


}
