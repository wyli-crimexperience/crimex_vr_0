using UnityEngine;



public class Form : HandItemBriefcase {

    [SerializeField] private Page[] pages;

    private GameObject penTip;
    private int pageIndex;



    private void Start() {
        pageIndex = 0;
    }
    private void OnCollisionEnter(Collision collision) {
        if (penTip == null && collision.collider.CompareTag("PenTip")) {
            penTip = collision.collider.gameObject;
            ManagerGlobal.Instance.SetCanWriteForm(true);
        }
    }
    private void OnCollisionExit(Collision collision) {
        if (collision.collider.CompareTag("PenTip") && collision.collider.gameObject == penTip) {
            ManagerGlobal.Instance.SetCanWriteForm(false);
            penTip = null;
        }
    }



    public virtual void Receive() { }
    public void TogglePage() {
        pageIndex += 1;
        if (pageIndex >= pages.Length) {
            pageIndex = 0;
        }

        for (int i = 0; i < pages.Length; i++) {
            pages[i].gameObject.SetActive(i == pageIndex);
        }
    }
    public void WriteOnPage() {
        pages[pageIndex].WriteNext();
    }

}