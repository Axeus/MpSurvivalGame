using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SyncRotation : NetworkBehaviour {

    [SyncVar]
    public Quaternion rot;
	// Use this for initialization
	void Start () {
        if (!isServer)
        {
             transform.rotation = rot;
        }
    }
}
