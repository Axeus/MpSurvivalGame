using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : NetworkBehaviour {

    [SerializeField]
	private int startingHealth = 100;

    [SyncVar(hook = "OnHealthChange")]
    public int currentHealth;
	public AudioClip deathClip;
	public AudioClip damageClip;

    [SyncVar]
    public bool isDead;
    public float hurtTime;

    public List<loot> enemyLoot = new List<loot>();

    [HideInInspector]
    public Transform player;
    [HideInInspector]
    public Vector3 hitPos;
    [HideInInspector]
    public PlayerStats playerHealth;

    [SyncVar]
    [HideInInspector]
    public GameObject spawner;

    [HideInInspector]
    public MobsSpawner spawner2;

    [HideInInspector]
    public int ID;

    [HideInInspector]
    public float distance;

    [HideInInspector]
    public bool hurt;   

    private float hurtTimer;
    private Animator anim;
    private AudioSource enemyAudio;
    private Transform thisTransform;
    private float random;

    [System.Serializable]
	public struct loot
	{
		public string itemId;
		public int minItemAmount;
		public int maxItemAmount;
	}

    private void OnHealthChange(int healthChange)
    {
        if (healthChange < currentHealth && healthChange > 0)
        {
            if (enemyAudio != null)
            {
                enemyAudio.clip = damageClip;
                enemyAudio.Play();
            }
        }

        currentHealth = healthChange;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    void Start ()
    {
		anim = GetComponent <Animator> ();
		enemyAudio = GetComponent <AudioSource> ();
		
		currentHealth = startingHealth;
        
        thisTransform = transform;
        random = Random.Range(5f, 10f);

        if (isServer)
        {
            spawner2 = spawner.GetComponent<MobsSpawner>();
            StartCoroutine(CreateMob(random));
            
        }
    }

	public void TakeDamage (int amount, Vector3 hitPoint)
	{
        if (isServer)
        {
            if (isDead)
                return;

            currentHealth -= amount;

            if (currentHealth > 0)
            {
                this.hitPos = hitPoint;

                hurt = true;
                hurtTimer = hurtTime;
            }
        }
    }
	
	void Death ()
	{
        isDead = true;

        anim.SetTrigger("Dead");

        if (isServer)
            for (int i = 0; i < enemyLoot.Count; i++)
                Global.inventoryControl.DropBag(Global.inventoryControl.database.FetchItemByID(enemyLoot[i].itemId), Mathf.RoundToInt(Random.Range(enemyLoot[i].minItemAmount * 1.0f, enemyLoot[i].maxItemAmount * 1.0f)), transform);

        enemyAudio.clip = deathClip;
        enemyAudio.Play();

        if (isServer)
            spawner2.RemoveMob(ID);		
	}

    public static float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    void Update ()
    {
        if (isServer)
        {
            if (spawner2 == null)
            {
                Destroy(gameObject);
                return;
            }

            distance = Distance(spawner2.thisTransform.position, thisTransform.position);
            if (hurt && hurtTimer > 0)
            {
                hurtTimer -= Time.deltaTime;
                if (hurtTimer <= 0)
                    hurt = false;
            }
        }    
    }

    IEnumerator CreateMob(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (distance > spawner2.distanceMax + 50f)
        {
            if (isDead) Destroy(gameObject);

            else
            {
                bool willSpawn = true;

                Collider[] hitColliders2 = Physics.OverlapSphere(thisTransform.position, spawner2.distanceMin / 2f);
                int i2 = 0;
                while (i2 < hitColliders2.Length)
                {
                    if (hitColliders2[i2].tag == "Player")
                    {
                        willSpawn = false;
                        break;
                    }
                    i2++;
                }

                if (willSpawn)
                { 
                    Vector3 rand = new Vector3(Random.Range(spawner2.thisTransform.position.x - spawner2.distanceMax, spawner2.thisTransform.position.x + spawner2.distanceMax), spawner2.thisTransform.position.y, Random.Range(spawner2.thisTransform.position.z - spawner2.distanceMax, spawner2.thisTransform.position.z + spawner2.distanceMax));

                    if (Distance(rand, spawner2.thisTransform.position) > spawner2.distanceMin)
                    {
                        RaycastHit hit;
                        Ray ray = new Ray(rand + Vector3.up * spawner2.terrainHeight, Vector3.down);

                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            if (hit.collider.tag == "Terrain")
                            {
                                bool spawn = true;

                                Collider[] hitColliders = Physics.OverlapSphere(hit.point, spawner2.distanceMin / 2f);
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
                                    thisTransform.position = new Vector3(rand.x, hit.point.y + 0.5f, rand.z);
                                    currentHealth = startingHealth;
                                    hurt = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        StartCoroutine(CreateMob(10f));
    }
}
