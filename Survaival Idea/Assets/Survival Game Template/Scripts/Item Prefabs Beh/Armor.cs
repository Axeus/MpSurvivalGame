using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Armor : NetworkBehaviour {

    [SyncVar]
    public GameObject parent;

	// Use this for initialization
	void Start () {
        transform.SetParent(parent.GetComponentInChildren<ThirdPerson>().transform, false);
        GameObject mesh = SkinnedMeshTools.AddSkinnedMeshTo(gameObject, transform.parent, false)[0];
        mesh.layer = LayerMask.NameToLayer("Character");
        gameObject.SetActive(false);
        if(!transform.parent.GetChild(0).gameObject.activeSelf)
            mesh.SetActive(false);
    }
}
