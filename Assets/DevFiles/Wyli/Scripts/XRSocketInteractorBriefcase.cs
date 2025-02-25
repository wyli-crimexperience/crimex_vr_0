using System.Net.Sockets;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRSocketInteractorBriefcase : XRSocketInteractor {

    private HandItem _handItem;

    public override bool CanSelect(IXRSelectInteractable interactable) {
        if (interactable is MonoBehaviour gameObject) {
            _handItem = gameObject.GetComponent<HandItem>();
            if (_handItem == null) {
                return false;
            } else {
                if (_handItem.BriefcaseItemCenter == null) {
                    return false;
                } else {
                    if (_handItem.BriefcaseItemCenter.CurrentSocket != this) {
                        return false;
                    }
                }
            }
        }

        return base.CanSelect(interactable);
    }

}