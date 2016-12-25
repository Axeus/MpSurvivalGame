using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class BuildCollisionProperties : NetworkBehaviour {
    
	public bool canObjectsBePlacedOnThis;

	public bool isBuilding;

    [SyncVar]
    public Quaternion direction;

    [SyncVar]
    [HideInInspector]
    public bool active = true;

    [HideInInspector]
    public int collide;

    void Start () {
        if (isServer)
            direction = transform.rotation;
        else
            transform.rotation = direction;

        Rigidbody body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeAll;
        body.useGravity = false;

        if (!active)
        {
            if (GetComponent<MeshCollider>() != null)
                GetComponent<MeshCollider>().convex = true;
            GetComponent<Collider>().isTrigger = true;
        }
        else
        {
            body.isKinematic = true;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }       
    }

	void OnTriggerEnter(Collider other)
	{
		if(!active)
		if(other.tag != "Terrain")
		{
			collide ++;
		}
	}
   
	void OnTriggerExit(Collider other)
	{
		if(!active)
		if(other.tag != "Terrain")
		{
			collide --;
		}
	}
}
