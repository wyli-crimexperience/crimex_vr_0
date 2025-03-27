using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class IXRFilter_HandToBriefcaseItem : MonoBehaviour, IXRHoverFilter, IXRSelectFilter {

    public bool canProcess => true;

    private HandItemBriefcase _handItemBriefcase;



    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) {
        if (interactable is MonoBehaviour gameObject) {
            return Process(gameObject);
        }
        return true;
    }
    public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable) {
        if (interactable is MonoBehaviour gameObject) {
            return Process(gameObject);
        }
        return true;
    }
    private bool Process(MonoBehaviour gameObject) {
        _handItemBriefcase = gameObject.GetComponent<HandItemBriefcase>();
        if (_handItemBriefcase != null && _handItemBriefcase.SocketBriefcase != null) {
            return _handItemBriefcase.SocketBriefcase.Briefcase.IsOpen;
        }
        return true;
    }
}
