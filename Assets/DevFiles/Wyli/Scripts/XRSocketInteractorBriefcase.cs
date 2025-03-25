using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class XRSocketInteractorBriefcase : XRSocketInteractor {
    
    private HandItemBriefcase _handItemBriefcase;
    // briefcase
    public Briefcase Briefcase { get; private set; }
    public void SetBriefcase(Briefcase briefcase) {
        Briefcase = briefcase;
    }



    public override bool CanSelect(IXRSelectInteractable interactable) {
        if (interactable is MonoBehaviour gameObject) {
            _handItemBriefcase = gameObject.GetComponent<HandItemBriefcase>();
            if (_handItemBriefcase == null) {
                return false;
            } else {
                if (_handItemBriefcase.BriefcaseItemCenter == null) {
                    return false;
                } else {
                    if (_handItemBriefcase.BriefcaseItemCenter.CurrentSocket != this) {
                        return false;
                    }
                }
            }
        }

        return base.CanSelect(interactable);
    }



    protected override void OnSelectEntered(SelectEnterEventArgs args) {
        base.OnSelectEntered(args);
        if (args.interactableObject is MonoBehaviour gameObject) {
            _handItemBriefcase = gameObject.GetComponent<HandItemBriefcase>();
            if (_handItemBriefcase != null) {
                _handItemBriefcase.SetSocketBriefcase(this);
            }
        }
    }
    protected override void OnSelectExited(SelectExitEventArgs args) {
        base.OnSelectExited(args);
        if (args.interactableObject is MonoBehaviour gameObject) {
            _handItemBriefcase = gameObject.GetComponent<HandItemBriefcase>();
            if (_handItemBriefcase != null) {
                _handItemBriefcase.SetSocketBriefcase(null);
            }
        }
    }
}