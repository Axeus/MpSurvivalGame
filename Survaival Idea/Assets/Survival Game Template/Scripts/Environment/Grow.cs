using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grow : NetworkBehaviour {

    public float time = 60f;
    float timer;
    public GameObject grownObj;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!isServer)
            return;

        timer += Time.deltaTime;

        if(timer >= time)
        {
            GameObject grownObjPrefab = Instantiate(grownObj, transform.position, transform.rotation) as GameObject;

            NetworkServer.Spawn(grownObjPrefab);

            Destroy(gameObject);
        }
    }
}
