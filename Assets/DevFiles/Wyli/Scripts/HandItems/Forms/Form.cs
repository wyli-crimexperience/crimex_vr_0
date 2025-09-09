using UnityEngine;

public class Form : HandItemBriefcase
{
    [SerializeField] private Page[] pages;
    private GameObject penTip;
    private int pageIndex;

    // Properties for better encapsulation
    public int CurrentPageIndex => pageIndex;
    public Page CurrentPage => pages != null && pageIndex < pages.Length ? pages[pageIndex] : null;
    public int TotalPages => pages?.Length ?? 0;

    private void Start()
    {
        pageIndex = 0;
        InitializePages();
    }

    private void InitializePages()
    {
        if (pages == null || pages.Length == 0) return;

        // Show only the first page initially
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].gameObject.SetActive(i == pageIndex);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (penTip == null && collision.collider.CompareTag("PenTip"))
        {
            penTip = collision.collider.gameObject;

            // Updated to use property instead of method
            if (ManagerGlobal.Instance != null)
            {
                ManagerGlobal.Instance.CanWriteForm = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("PenTip") && collision.collider.gameObject == penTip)
        {
            // Updated to use property instead of method
            if (ManagerGlobal.Instance != null)
            {
                ManagerGlobal.Instance.CanWriteForm = false;
            }

            penTip = null;
        }
    }

    public virtual void Receive()
    {
        // Override in derived classes for specific form types
    }

    public void TogglePage()
    {
        if (pages == null || pages.Length == 0) return;

        pageIndex = (pageIndex + 1) % pages.Length;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].gameObject.SetActive(i == pageIndex);
        }
    }

    public void GoToPage(int index)
    {
        if (pages == null || index < 0 || index >= pages.Length) return;

        pageIndex = index;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].gameObject.SetActive(i == pageIndex);
        }
    }

    public void WriteOnPage()
    {
        if (CurrentPage != null)
        {
            CurrentPage.WriteNext();
        }
    }
}