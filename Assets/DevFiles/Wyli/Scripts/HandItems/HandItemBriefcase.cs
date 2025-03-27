using UnityEngine;



public class HandItemBriefcase : HandItem {

    // the briefcase socket i should start at
    [SerializeField] private XRSocketInteractorBriefcase socketBriefcase;
    public XRSocketInteractorBriefcase SocketBriefcase => socketBriefcase;
    // my briefcase center detector
    [SerializeField] private BriefcaseItemCenter briefcaseItemCenter;
    public BriefcaseItemCenter BriefcaseItemCenter => briefcaseItemCenter;



    public void InitBriefcase() {
        // assumes that socketBriefcase != null
        socketBriefcase.startingSelectedInteractable = interactable;
    }



    public void SetSocketBriefcase(XRSocketInteractorBriefcase _socketBriefcase) {
        socketBriefcase = _socketBriefcase;
    }

}