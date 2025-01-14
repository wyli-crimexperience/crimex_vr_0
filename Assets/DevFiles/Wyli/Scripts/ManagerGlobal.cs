using System.Collections.Generic;

using UnityEngine;

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

    [SerializeField] private HandItem handItemNotepad, handItemPen, handItemPoliceTape, handItemPhone;
    [SerializeField] private List<HandItem> handItemsLeft = new List<HandItem>(), handItemsRight = new List<HandItem>();

    private TypeItem typeItemLeft, typeItemRight;



    private void Awake() {
        Instance = this;

        SetHandItem(0, TypeItem.None);
        SetHandItem(1, TypeItem.None);

        ActivateWristwatch(false);
    }



    // general utils
    private void SetHandItem(int typeHand, TypeItem typeItem) {
        switch (typeHand) {
            case 0: {
                typeItemLeft = typeItem;
                foreach (HandItem handItem in handItemsLeft) {
                    handItem.gameObject.SetActive(handItem.TypeItem == typeItem);
                }
            }
            break;

            case 1: {
                typeItemRight = typeItem;
                foreach (HandItem handItem in handItemsRight) {
                    handItem.gameObject.SetActive(handItem.TypeItem == typeItem);
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