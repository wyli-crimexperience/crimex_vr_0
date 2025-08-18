using UnityEngine;



public class Wipe : HandItem {

    private Wipes owner;
    public Wipes Owner => owner;



    public void SetOwner(Wipes wipes) {
        owner = wipes;
    }
    public override void Grab() {
        base.Grab();

        owner.GetWipe(this);
    }

}