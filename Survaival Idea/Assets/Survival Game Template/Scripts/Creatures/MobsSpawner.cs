using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MobsSpawner : NetworkBehaviour {

    public int terrainHeight;
    public float distanceMax = 200f;
    public float distanceMin = 100f;

    public List<mobsSpawnInfo> mobs = new List<mobsSpawnInfo>();

    [HideInInspector]
    public Transform thisTransform;

    [System.Serializable]
    public struct mobsSpawnInfo
    {
        public GameObject mob;
        public int mobsCount;
    }

    void Start()
    {
        thisTransform = transform;

        if (isServer)
        {
            for (int i = 0; i < mobs.Count; i++)
            {
                for (int j = 0; j < mobs[i].mobsCount; j++)
                    StartCoroutine(CreateMob(i, 0f));
            }
        }
	}

    public void RemoveMob(int ID)
    {
        StartCoroutine(CreateMob(ID, 1f));
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    [Command]
    void CmdSpawn(int ID, Vector3 rand, float yCoord)
    {
        GameObject locMob = Instantiate(mobs[ID].mob, new Vector3(rand.x, yCoord + 0.5f, rand.z), Quaternion.identity) as GameObject;
        Vector3 euler = locMob.transform.eulerAngles;
        euler.y = Random.Range(0f, 360f);
        locMob.transform.eulerAngles = euler;
        EnemyHealth health = locMob.GetComponent<EnemyHealth>();
        health.spawner = gameObject;
        health.ID = ID;

        NetworkServer.Spawn(locMob);
    }

    IEnumerator CreateMob(int ID, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        Vector3 rand = new Vector3(Random.Range(transform.position.x - distanceMax, transform.position.x + distanceMax), transform.position.y, Random.Range(transform.position.z - distanceMax, transform.position.z + distanceMax));

        if (Distance(rand, transform.position) > distanceMin)
        {
            RaycastHit hit;
            Ray ray = new Ray(rand + Vector3.up * terrainHeight, Vector3.down);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Terrain")
                {
                    bool spawn = true;

                    Collider[] hitColliders = Physics.OverlapSphere(hit.point, 20);
                    int i = 0;
                    while (i < hitColliders.Length)
                    {
                        if (hitColliders[i].tag == "Player")
                        {
                            spawn = false;
                            break;
                        }
                        i++;
                    }

                    if (spawn)
                    {
                        CmdSpawn(ID, rand, hit.point.y);
                    }
                    else
                        StartCoroutine(CreateMob(ID, delayTime));
                }
                else
                    StartCoroutine(CreateMob(ID, delayTime));
            }
        }
        else
            StartCoroutine(CreateMob(ID, delayTime));
    }
}
