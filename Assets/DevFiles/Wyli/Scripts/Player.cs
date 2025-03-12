using UnityEngine;



public class Player : MonoBehaviour {

    private TypeRole typeRole;
    public TypeRole TypeRole => typeRole;



    public void Init(TypeRole _typeRole) {
        typeRole = _typeRole;
    }

}