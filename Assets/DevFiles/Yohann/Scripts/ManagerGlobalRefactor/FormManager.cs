
using UnityEngine;

// FORM MANAGER
// Responsible for managing the instantiation and assignment of form objects to players in the scene.
// It holds references to HolderData (for prefab access), TimelineManager (for event tracking), and RoleManager (for player/role context).
// The Initialize method sets up these references.
// SpawnFormFirstResponder and SpawnFormInvestigatorOnCase instantiate the appropriate form prefab, position it in the player's left hand,
// and update the timeline with the relevant event, but only if the player is close enough to the current player.
// Both methods return true if the form was successfully spawned, or false if the distance check fails.

public class FormManager : MonoBehaviour
{
    [SerializeField] private HolderData holderData;
    [SerializeField] private TimelineManager timelineManager;
    [SerializeField] private RoleManager roleManager;

    public void Initialize(HolderData data, TimelineManager timeline, RoleManager role)
    {
        holderData = data;
        timelineManager = timeline;
        roleManager = role;
    }

    public bool SpawnFormFirstResponder(Player firstResponder)
    {
        const float DIST_CONVERSE = 1.5f;

        if (Vector3.Distance(firstResponder.transform.position, roleManager.CurrentPlayer.transform.position) > DIST_CONVERSE)
            return false;

        timelineManager.SetEventNow(TimelineEvent.FirstResponderFilledUp,
            timelineManager.GetEventTime(TimelineEvent.Incident).Value);

        HandItem form = Instantiate(holderData.PrefabFormFirstResponder).GetComponent<HandItem>();
        form.SetPaused(true);
        form.transform.SetPositionAndRotation(firstResponder.HandLeft.position, firstResponder.HandLeft.rotation);
        return true;
    }

    public bool SpawnFormInvestigatorOnCase(Player investigatorOnCase)
    {
        const float DIST_CONVERSE = 1.5f;

        if (Vector3.Distance(investigatorOnCase.transform.position, roleManager.CurrentPlayer.transform.position) > DIST_CONVERSE)
            return false;

        timelineManager.SetEventNow(TimelineEvent.InvestigatorFilledUp,
            timelineManager.GetEventTime(TimelineEvent.Incident).Value);

        HandItem form = Instantiate(holderData.PrefabFormInvestigatorOnCase).GetComponent<HandItem>();
        form.SetPaused(true);
        form.transform.SetPositionAndRotation(investigatorOnCase.HandLeft.position, investigatorOnCase.HandLeft.rotation);
        return true;
    }
}
