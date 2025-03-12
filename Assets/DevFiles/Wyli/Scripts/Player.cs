using System.Collections.Generic;

using UnityEngine;



public class Player : MonoBehaviour {

    [SerializeField] IKTargetFollowVRRig ikTarget;

    [SerializeField] private TypeRole typeRole;
    public TypeRole TypeRole => typeRole;

    private List<MonoBehaviour> components = new List<MonoBehaviour>();



    private void Awake() {
        foreach (MonoBehaviour component in GetComponents<MonoBehaviour>()) {
            components.Add(component);
        }
    }



    public void Init(TypeRole _typeRole) {
        typeRole = _typeRole;

        ikTarget.leftHand.vrTarget = ManagerGlobal.Instance.VRTargetLeftHand;
        ikTarget.rightHand.vrTarget = ManagerGlobal.Instance.VRTargetRightHand;
        ikTarget.head.vrTarget = ManagerGlobal.Instance.VRTargetHead;
    }
    public void SetActive(bool b) {
        foreach (MonoBehaviour component in components) {
            component.enabled = b;
        }
    }

}