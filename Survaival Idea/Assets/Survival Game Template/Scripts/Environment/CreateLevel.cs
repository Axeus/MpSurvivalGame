using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;



public class CreateLevel : NetworkBehaviour {

    public float waterHeight;

    [SerializeField]
    private List<objectSpawnInfo> objectsToSpawn = new List<objectSpawnInfo>();

    private Terrain terrain;
    private Transform thisTransform;

    [System.Serializable]
    public struct objectSpawnInfo
    {
        public GameObject obj;
        public int objCount;
        public float offset;
    }

    //public override void OnStartServer()
   // {
   // /    
    //}

    void Start()
    {
        if(isServer)
        {
            terrain = GetComponent<Terrain>();
            thisTransform = transform;

            StartSpawn();

        }
        
    }

    void StartSpawn()
    {
        for (int i = 0; i < objectsToSpawn.Count; i++)
        {
            for (int j = 0; j < objectsToSpawn[i].objCount; j++)
            {
                Spawn(objectsToSpawn[i].obj, objectsToSpawn[i].offset);
            }
        }
    }


    void Spawn(GameObject obj, float offset)
    {       
        Vector3 rand = new Vector3(Random.Range(thisTransform.position.x, thisTransform.position.x + terrain.terrainData.size.x), thisTransform.position.y, Random.Range(thisTransform.position.z, thisTransform.position.z + terrain.terrainData.size.z));

        if (terrain.SampleHeight(rand) > waterHeight)
        {
            GameObject locObj = Instantiate(obj).gameObject;
            float height = terrain.SampleHeight(rand) + thisTransform.position.y;
            locObj.transform.position = new Vector3(rand.x, height + offset, rand.z);
            Vector3 euler = locObj.transform.eulerAngles;
            euler.y = Random.Range(0f, 360f);
            locObj.transform.eulerAngles = euler;
            if(locObj.GetComponent<SyncRotation>() != null)
                locObj.GetComponent<SyncRotation>().rot = locObj.transform.rotation;

            NetworkServer.Spawn(locObj);
        }
        else
            Spawn(obj, offset);
    }
}
