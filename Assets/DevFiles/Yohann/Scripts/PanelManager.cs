using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject settingsPanel;
    public GameObject profilePanel;
    public GameObject profileLoginPanel;

    public void OpenStartPanel()
    {
        CloseAllPanels();
        startPanel.SetActive(true);
    }

    public void OpenSettingsPanel()
    {
        CloseAllPanels();
        settingsPanel.SetActive(true);
    }

    public void OpenProfilePanel()
    {
        CloseAllPanels();
        profilePanel.SetActive(true);
    }
    public void OpenProfileLoginPanel()
    {
        CloseAllPanels();
        profileLoginPanel.SetActive(true);
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
