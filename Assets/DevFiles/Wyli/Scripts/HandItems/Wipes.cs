using UnityEngine;



public class Wipes : HandItemBriefcase {

    private Wipe wipe;



    public void SpawnWipe() {
        if (wipe != null) { return; }

        wipe = Instantiate(ManagerGlobal.Instance.HolderData.PrefabWipe, ManagerGlobal.Instance.ContainerWipes).GetComponent<Wipe>();
        wipe.SetOwner(this);
    }
    public void GetWipe(Wipe w) {
        if (wipe == null) { return; }

        if (wipe == w) {
            wipe = null;
        }
    }

    private void Update() {
        if (wipe != null) {
            wipe.transform.SetPositionAndRotation(
                transform.position + transform.up * ManagerGlobal.Instance.HolderData.PrefabWipe.transform.position.y,
                transform.rotation);
        }
    }

}