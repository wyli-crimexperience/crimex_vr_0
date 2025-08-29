using UnityEngine;

public class SpectatorCameraManager : MonoBehaviour
{
    [Header("Spectator Cameras")]
    [SerializeField] private Camera[] spectatorCameras;
    private int currentIndex = 0;

    void Start()
    {
        ActivateCamera(0); // start with first camera
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ActivateCamera(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ActivateCamera(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ActivateCamera(2);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentIndex = (currentIndex + 1) % spectatorCameras.Length;
            ActivateCamera(currentIndex);
        }
    }

    private void ActivateCamera(int index)
    {
        for (int i = 0; i < spectatorCameras.Length; i++)
        {
            spectatorCameras[i].gameObject.SetActive(i == index);
        }
    }
}
