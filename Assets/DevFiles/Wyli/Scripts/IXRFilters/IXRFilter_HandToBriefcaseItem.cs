using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class IXRFilter_HandToBriefcaseItem : MonoBehaviour, IXRHoverFilter, IXRSelectFilter {

    public bool canProcess => true;

    private HandItem _handItem;



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
        _handItem = gameObject.GetComponent<HandItem>();
        if (_handItem != null && _handItem.SocketBriefcase != null) {
            return _handItem.SocketBriefcase.Briefcase.IsOpen;
        }
        return true;
    }
}
