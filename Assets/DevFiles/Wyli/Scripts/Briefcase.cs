using System.Collections;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;



public class Briefcase : MonoBehaviour {

    [SerializeField] private XRSocketInteractor[] sockets;
    [SerializeField] private HandItem[] items;



    private IEnumerator Start() {
        foreach (XRSocketInteractor socket in sockets) {
            socket.socketActive = false;
        }
        yield return new WaitForEndOfFrame();

        foreach (HandItem item in items) {
            item.SocketBriefcase.socketActive = true;
            item.transform.position = item.SocketBriefcase.transform.position;
        }
        yield return new WaitForEndOfFrame();

        foreach (XRSocketInteractor socket in sockets) {
            socket.socketActive = true;
        }
    }

}