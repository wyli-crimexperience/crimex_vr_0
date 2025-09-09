using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform handLeft;
    public Transform HandLeft => handLeft;

    [SerializeField] private Collider coll;
    [SerializeField] private IKTargetFollowVRRig ikTarget;

    [SerializeField] private TypeRole typeRole;
    public TypeRole TypeRole => typeRole;

    private List<MonoBehaviour> components = new List<MonoBehaviour>();
    private bool isDoneConversing;

    [SerializeField] private GameObject[] hideFromSight;

    private void Awake()
    {
        foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
        {
            if (component is XRSimpleInteractable) continue;
            components.Add(component);
        }

        foreach (GameObject go in hideFromSight)
        {
            go.layer = LayerMask.NameToLayer("TransparentFX");
        }

        if (coll != null) coll.enabled = false;
    }

    public void Init(TypeRole _typeRole)
    {
        typeRole = _typeRole;

        ikTarget.leftHand.vrTarget = ManagerGlobal.Instance.VRTargetLeftHand;
        ikTarget.rightHand.vrTarget = ManagerGlobal.Instance.VRTargetRightHand;
        ikTarget.head.vrTarget = ManagerGlobal.Instance.VRTargetHead;
    }

    public void SetActive(bool b)
    {
        foreach (MonoBehaviour component in components)
        {
            component.enabled = b;
        }
        foreach (GameObject go in hideFromSight)
        {
            go.layer = b ? LayerMask.NameToLayer("TransparentFX") : LayerMask.NameToLayer("Default");
        }
        if (coll != null) coll.enabled = !b;
    }

    public void GazePlayer()
    {
        if (isDoneConversing)
        {
            ManagerGlobal.Instance.ThoughtManager.ShowThought(gameObject,
                "I've already talked to them...");
        }
        else
        {
            // Investigator -> First Responder (handover form)
            if (ManagerGlobal.Instance.TypeRolePlayer == TypeRole.InvestigatorOnCase &&
                typeRole == TypeRole.FirstResponder)
            {
                if (ManagerGlobal.Instance.FormManager.SpawnFormFirstResponder(this))
                {
                    isDoneConversing = true;
                }
            }
            // SOCO Lead -> Investigator (handover form)
            else if (ManagerGlobal.Instance.TypeRolePlayer == TypeRole.SOCOTeamLead &&
                     typeRole == TypeRole.InvestigatorOnCase)
            {
                if (ManagerGlobal.Instance.FormManager.SpawnFormInvestigatorOnCase(this))
                {
                    isDoneConversing = true;
                }
            }
        }
    }

    public void DoneConversing()
    {
        isDoneConversing = true;
    }
}
