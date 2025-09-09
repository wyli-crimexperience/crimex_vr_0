using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItemRole : MonoBehaviour
{
    [SerializeField] private Image imgButton;
    [SerializeField] private TextMeshProUGUI txtName;

    private TypeRole typeRole;
    public TypeRole TypeRole => typeRole;

    public void Init(TypeRole _typeRole)
    {
        typeRole = _typeRole;

        // Get role name from RoleManager instead of ManagerGlobal directly
        var mgr = ManagerGlobal.Instance;
        if (mgr?.RoleManager != null)
        {
            txtName.text = mgr.RoleManager.GetRoleName(typeRole);
        }
        else
        {
            // Fallback - you could also create a static utility method
            txtName.text = GetRoleNameFallback(typeRole);
        }
    }

    public void SetSelected(bool b)
    {
        imgButton.color = new Color(1, 1, 1, b ? 1 : 0);
    }

    /// <summary>
    /// Fallback method for getting role names when managers aren't available
    /// </summary>
    private string GetRoleNameFallback(TypeRole role)
    {
        return role switch
        {
            TypeRole.FirstResponder => "First Responder",
            TypeRole.InvestigatorOnCase => "Investigator-On-Case",
            TypeRole.SOCOTeamLead => "SOCO Team Lead",
            TypeRole.Photographer => "Photographer",
            TypeRole.Searcher => "Searcher",
            TypeRole.Measurer => "Measurer",
            TypeRole.Sketcher => "Sketcher",
            TypeRole.FingerprintSpecialist => "Fingerprint Specialist",
            TypeRole.Collector => "Collector",
            TypeRole.EvidenceCustodian => "Evidence Custodian",
            _ => role.ToString(),
        };
    }
}