using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;



public enum TypeItem {
    None,

    // first responder
    Notepad,
    Pen,
    PoliceTape,
    Phone,
    
    // investigator-on-case part 1
    FirstResponderForm,
    
    // soco photographer
    Camera,
    Sketchpad,

    // soco searcher
    EvidenceMarker,

    // soco measurer
    TapeMeasure,
    EvidenceRuler,
    CaseID,

    // soco specialist
    FingerprintBrush,
    FingerprintTape,
    LiftedFingerprint,

    // soco collector
    Chalk,
    SterileSwab,
    EvidencePack,

    // ioc or soco team leader part 2
    ItemOfIdentification,

    // evidence custodian
    EvidenceChecklist,

    // ioc part 3
    ReleaseOfCrimeSceneForm

}
public class ManagerGlobal : MonoBehaviour {
    public static ManagerGlobal Instance;

    [SerializeField] private InputActionReference primaryButtonLeft, secondaryButtonLeft, primaryButtonRight, secondaryButtonRight;
    [SerializeField] private HandItem handItemNotepad, handItemPen, handItemPoliceTape, handItemPhone;
    [SerializeField] private List<HandItem> handItemsLeft = new List<HandItem>(), handItemsRight = new List<HandItem>();

    private int handItemIndexLeft, handItemIndexRight;



    private void Awake() {
        Instance = this;

        primaryButtonLeft.action.performed += PrimaryButtonLeft;
        secondaryButtonLeft.action.performed += SecondaryButtonLeft;
        primaryButtonRight.action.performed += PrimaryButtonRight;
        secondaryButtonRight.action.performed += SecondaryButtonRight;

        handItemsLeft.Insert(0, null);
        handItemsRight.Insert(0, null);

        UpdateHandItemIndex(0, 0);
        UpdateHandItemIndex(1, 0);

        ActivateWristwatch(false);
    }
    private void OnDestroy() {
        primaryButtonLeft.action.performed -= PrimaryButtonLeft;
        secondaryButtonLeft.action.performed -= SecondaryButtonLeft;
        primaryButtonRight.action.performed -= PrimaryButtonRight;
        secondaryButtonRight.action.performed -= SecondaryButtonRight;
    }
    private void PrimaryButtonLeft(InputAction.CallbackContext context) {
        UpdateHandItemIndex(0, handItemIndexLeft + 1);
    }
    private void SecondaryButtonLeft(InputAction.CallbackContext context) {
        UpdateHandItemIndex(0, handItemIndexLeft - 1);
    }
    private void PrimaryButtonRight(InputAction.CallbackContext context) {
        UpdateHandItemIndex(1, handItemIndexRight + 1);
    }
    private void SecondaryButtonRight(InputAction.CallbackContext context) {
        UpdateHandItemIndex(1, handItemIndexRight - 1);
    }
    private void UpdateHandItemIndex(int typeHand, int index) {
        switch (typeHand) {
            case 0: {
                if (index == -1) { index = handItemsLeft.Count - 1; }
                if (index == handItemsLeft.Count) { index = 0; }
                handItemIndexLeft = index;

                SetHandItem(0, handItemsLeft[index]);
            } break;

            case 1: {
                if (index == -1) { index = handItemsRight.Count - 1; }
                if (index == handItemsRight.Count) { index = 0; }
                handItemIndexRight = index;

                SetHandItem(1, handItemsRight[index]);
            } break;

            default: break;
        }
    }



    // general utils
    private void SetHandItem(int typeHand, HandItem handItem) {
        switch (typeHand) {
            case 0: {
                foreach (HandItem hi in handItemsLeft) {
                    handItem.gameObject.SetActive(hi == handItem);
                }
            }
            break;

            case 1: {
                foreach (HandItem hi in handItemsRight) {
                    handItem.gameObject.SetActive(hi == handItem);
                }
            }
            break;

            default: break;
        }
    }



    // wristwatch
    [SerializeField] private GameObject containerPopupWristWatch;
    [SerializeField] private TextMeshProUGUI txtWristwatchTime;
    public void ActivateWristwatch(bool isActive) {
        if (isActive) {
            string text = System.DateTime.Now.ToString("hh:mm tt");
            txtWristwatchTime.text = text;

            containerPopupWristWatch.SetActive(true);
        } else {
            containerPopupWristWatch.SetActive(false);
        }
    }
}