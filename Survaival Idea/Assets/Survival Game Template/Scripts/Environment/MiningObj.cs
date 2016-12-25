using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MiningObj : NetworkBehaviour {
   
    public ObjTypeEnum objectType; 

    public AudioClip deathClip;
    public AudioClip damageClip;

    public GameObject explosion;
    public GameObject particles;

    [SyncVar(hook = "OnHealthChange")]
    public int currentHealth;
    [SyncVar]
    public bool isDead;

    [SerializeField]
    private int startingHealth = 20;

    public List<loot> resourceLoot = new List<loot>();

    [SyncVar]
    private string player;

    private AudioSource miningAudio;
    private Animation miningAnimation;

    public enum ObjTypeEnum {
        None,
		Tree,
		Iron
	}

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
            if (miningAudio != null)
            {
                miningAudio.PlayOneShot(damageClip);
            }
        }

        currentHealth = healthChange;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    void Start () {

        miningAnimation = GetComponent<Animation>();
        miningAudio = GetComponent<AudioSource>();

		currentHealth = startingHealth;
	}

    void OnDestroy()
    {
        if (transform.parent != null && transform.parent.childCount < 2)
            Destroy(transform.parent.gameObject);       
    }

    public void TakeDamage (int amount, string player)
	{
        if (isServer)
        {
            if (isDead)
                return;

            this.player = player;

            currentHealth -= amount;
        }
    }

	void Death ()
	{
		isDead = true;
        
        if (miningAnimation != null)
            miningAnimation.Play("Death");

        if (player == Global.playerID)
        {
            for (int i = 0; i < resourceLoot.Count; i++)
                Global.inventoryControl.AddItemsToInventory(resourceLoot[i].itemId, Mathf.RoundToInt(Random.Range(resourceLoot[i].minItemAmount * 1.0f, resourceLoot[i].maxItemAmount * 1.0f)));
        }

        if (isServer)
        {
            GameObject explos = null;     
            if (explosion != null)
            {
                explos = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
                NetworkServer.Spawn(explos);
            }

            if (deathClip != null && explos != null)
            {
                RpcChangeDeathSound(explos);
                explos.GetComponent<AudioSource>().PlayOneShot(deathClip);
            }

            Destroy(gameObject);
        }
	}

    [ClientRpc]
    void RpcChangeDeathSound(GameObject explos)
    {
        explos.GetComponent<AudioSource>().PlayOneShot(deathClip);
    }
}
