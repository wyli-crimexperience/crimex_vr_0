using UnityEngine;

public class EvidencePackSeal : MonoBehaviour
{
    [SerializeField] private GameObject containerTape, containerMarking;
    private HandItem handItemPen;

    public bool IsTaped { get; private set; }
    public bool IsMarked { get; private set; }

    public void SetTaped(bool b)
    {
        IsTaped = b;
        containerTape.SetActive(b);
    }

    public void SetMarked(bool b)
    {
        IsMarked = b;
        containerMarking.SetActive(b);
    }

    private void Start()
    {
        SetTaped(false);
        SetMarked(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItem.TypeItem == TypeItem.Pen)
        {
            handItemPen = handItem;

            // Updated to use property instead of method
            if (ManagerGlobal.Instance != null)
            {
                ManagerGlobal.Instance.CanWriteEvidencePackSeal = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HandItem handItem = other.GetComponent<HandItem>();
        if (handItem != null && handItemPen != null && handItem == handItemPen)
        {
            // Updated to use property instead of method
            if (ManagerGlobal.Instance != null)
            {
                ManagerGlobal.Instance.CanWriteEvidencePackSeal = false;
            }

            handItemPen = null; // Clear the reference
        }
    }
}