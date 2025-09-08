using UnityEngine;

public class Wipes : HandItemBriefcase
{
    private Wipe wipe;

    public void SpawnWipe()
    {
        if (wipe != null) return;

        var mgr = ManagerGlobal.Instance;
        if (mgr == null || mgr.InteractionManager == null || mgr.HolderData == null) return;

        var prefab = mgr.HolderData.PrefabWipe;
        var parent = mgr.InteractionManager.ContainerWipes;

        wipe = Object.Instantiate(prefab, parent).GetComponent<Wipe>();
        wipe.SetOwner(this);
    }

    public void GetWipe(Wipe w)
    {
        if (wipe == null) return;
        if (wipe == w) wipe = null;
    }

    private void Update()
    {
        if (wipe != null)
        {
            var mgr = ManagerGlobal.Instance;
            if (mgr == null || mgr.HolderData == null) return;

            float offsetY = mgr.HolderData.PrefabWipe.transform.localPosition.y;

            wipe.transform.SetPositionAndRotation(
                transform.position + transform.up * offsetY,
                transform.rotation
            );
        }
    }
}
