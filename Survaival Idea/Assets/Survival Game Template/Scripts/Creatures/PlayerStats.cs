using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour {
    public bool disableThirst;
    public bool disableFullness;

    public int startingHealth = 100;
    [SyncVar(hook = "OnHealthChange")]
    public int currentHealth;
    public int startingDefence;
    public int startingStamina = 100;
    public int startingFullness = 100;
    public int startingThirst = 100;
    [SyncVar]
    public int currentDefence;
    public AudioClip deathClip;
	public AudioClip damageClip;

    public float thirst { get; private set; }
    public float fullness { get; private set; }
    public float stamina { get; private set; }
    [SyncVar]
    public bool isDead;

	public float staminaSpeed; 
	public float hungryFrequency;
    public float thirstFrequency;

    public float respawnTime = 5f;

    public Image damageImage;
    private Image damageImg;

    public float flashSpeed = 5f;
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);

    public bool enablePvp = true;

    private float respawnTimer;

    private InventoryControl inventoryControl;

    private Text fullnessText;
    private Text thirstText;
    private Text staminaText;
    private Text hpText;
    private Slider staminaSlider;
    private Slider fullnessSlider;
    private Slider thirstSlider;
    private Slider hpSlider;
    private AudioSource playerAudio;
    private PlayerController controller;
    private Animator weaponAnimator;
    private GameObject deathText;

    private void OnHealthChange(int healthChange)
    {
        if (healthChange < currentHealth && healthChange < startingHealth && healthChange > 0)
        {
            playerAudio.PlayOneShot(damageClip);
            damaged = true;
        }

        currentHealth = healthChange;

        if (currentHealth <= 0)
        {
            Death();
        }  
    }

    [SyncVar]
    bool damaged;

    void Start () {
		playerAudio = GetComponent <AudioSource> ();
        
        currentHealth = startingHealth;
		currentDefence = startingDefence;

        if (isLocalPlayer)
        {
            weaponAnimator = transform.GetChild(0).GetComponentInChildren<Animator>();
            controller = GetComponent<PlayerController>();

            inventoryControl = GetComponent<InventoryControl>();
            fullness = startingFullness;
            thirst = startingThirst;
            stamina = startingStamina;

            staminaSpeed /= 100f;
            
            if(!disableFullness)
                InvokeRepeating("Hungry", hungryFrequency, hungryFrequency);
            if (!disableThirst)
                InvokeRepeating("DelThirst", thirstFrequency, thirstFrequency);

            fullnessSlider = GameObject.Find("FullnessSlider").GetComponent<Slider>();
            fullnessText = fullnessSlider.transform.GetChild(2).GetComponent<Text>();
            if (disableFullness)
                fullnessSlider.gameObject.SetActive(false);

            thirstSlider = GameObject.Find("ThirstSlider").GetComponent<Slider>();
            thirstText = thirstSlider.transform.GetChild(2).GetComponent<Text>();
            if (disableThirst)
                thirstSlider.gameObject.SetActive(false);

            staminaSlider = GameObject.Find("StaminaSlider").GetComponent<Slider>();
            staminaText = staminaSlider.transform.GetChild(2).GetComponent<Text>();

            hpSlider = GameObject.Find("HPSlider").GetComponent<Slider>();
            hpText = hpSlider.transform.GetChild(2).GetComponent<Text>();

            staminaText.text = stamina.ToString();
            fullnessText.text = fullness.ToString();
            thirstText.text = thirst.ToString();
            hpText.text = currentHealth.ToString();

            deathText = GameObject.Find("DeathText");
            deathText.SetActive(false);

            damageImg = Instantiate(damageImage);
            damageImg.transform.SetParent(GameObject.Find("GameCanvas").transform, false);

        }
    }

    public void DelThirst()
    {
        thirst--;
        if (thirst < 0)
        {
            CmdTakeDamage(1, false);
            thirst = 0;
        }
        thirstSlider.value = thirst;
        thirstText.text = thirst.ToString();
    }

    public void AddThirst(int count)
    {
        thirst += count;
        if (thirst > startingThirst) thirst = startingThirst;
        thirstSlider.value = thirst;
        thirstText.text = thirst.ToString();
    }

    public void AddFullness(int count)
	{
		fullness += count;
		if(fullness > startingFullness) fullness = startingFullness;
		fullnessSlider.value = fullness;
		fullnessText.text = fullness.ToString();
	}

	public void AddStamina(float count)
	{
		stamina += count;
		if(stamina > startingStamina) stamina = startingStamina;
		staminaSlider.value = stamina;
		staminaText.text = ((int)stamina).ToString();
	}

    [Command]
    void CmdSpawn( GameObject obj, string slug)
    {
        GameObject arm = Instantiate(Resources.Load<GameObject>("Armor Prefabs/" + slug));
        arm.GetComponent<Armor>().parent = obj;

        NetworkServer.Spawn(arm);
    }

    [Command]
    void CmdDeleteArmor(GameObject obj, int i)
    {
        RpcDeleteArmor(obj, i);
    }

    [ClientRpc]
    void RpcDeleteArmor(GameObject obj, int s)
    {
        Transform objTrans = obj.GetComponentInChildren<ThirdPerson>().transform;

        for (int i = 2; i < s; i++)
            Destroy(objTrans.GetChild(i).gameObject);
    }

    public void UpdateCharacterPanelInfo()
	{
		int locDefence = startingDefence;

        if (inventoryControl.gloves != null)
            Destroy(inventoryControl.gloves);

        CmdDeleteArmor(Global.player.gameObject, Global.player.gameObject.GetComponentInChildren<ThirdPerson>().transform.childCount);

        for (int i = 0; i < inventoryControl.characterInv.items.Count; i++)
		{
            if (!inventoryControl.characterInv.items[i].isNull)
            {
                locDefence += inventoryControl.characterInv.items[i].Defence;

                if (inventoryControl.characterInv.items[i].ItemType == Item.ItemTypeEnum.Gloves)
                {                   
                    inventoryControl.gloves = SkinnedMeshTools.AddSkinnedMeshTo(Resources.Load<GameObject>("Armor Prefabs/" + inventoryControl.characterInv.items[i].Slug), Global.player.GetChild(0).GetComponentInChildren<Animator>().transform, false)[0];
                }
                CmdSpawn(Global.player.gameObject, inventoryControl.characterInv.items[i].Slug);                
            }
        }

		if(locDefence != currentDefence)
		{
			currentDefence = locDefence;
			inventoryControl.characterInvPanel.transform.FindChild("Info").GetComponent<Text>().text = "Defence: " + currentDefence.ToString();
		}
	}

	public void Hungry()
	{
		fullness --;
		if(fullness < 0)
		{
            CmdTakeDamage(1, false);
            fullness = 0;
		}
		fullnessSlider.value = fullness;
		fullnessText.text = fullness.ToString();
	}

	public void Tired(float count)
	{
		stamina -= count;
		if(stamina < 0)
		{
			stamina = 0;
		}
		staminaSlider.value = stamina;
		staminaText.text = ((int)stamina).ToString();
	}

    [Command]
    void CmdTakeDamage(int amount, bool applyDefence)
    {
        TakeDamage(amount, applyDefence);
    }

    public void TakeDamage (int amount, bool applyDefence = true)
	{
        if (isServer)
        {
            if (isDead)
                return;

            

            if (applyDefence)
                currentHealth -= Mathf.RoundToInt(amount - (float)(amount * (currentDefence / 100f)));
            else
                currentHealth -= amount;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
            }
        }      
    }

    public void Heal(int amount)
    {
        CmdHeal(amount);
    }

    void Death ()
	{
        isDead = true;

        if (isLocalPlayer)
        {
            inventoryControl.CmdAnimSetBool(Global.player.gameObject, "Death", isDead);
            weaponAnimator.SetBool("Death", isDead);

            inventoryControl.CmdAnimSetInteger(Global.player.gameObject, "AttackIndex", 1);
            weaponAnimator.SetInteger("AttackIndex", 1);

            for (int i = 0; i < inventoryControl.playerInv.items.Count; i++)
            {
                if (!inventoryControl.playerInv.items[i].isNull)
                {
                    ItemBehavior data = inventoryControl.playerInv.slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();

                    inventoryControl.DropBag(data.item, data.amount, transform);
                }
            }
            inventoryControl.playerInv.CleanInventory();

            for (int i = 0; i < inventoryControl.characterInv.items.Count; i++)
            {
                if (!inventoryControl.characterInv.items[i].isNull)
                {
                    ItemBehavior data = inventoryControl.characterInv.slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();

                    inventoryControl.DropBag(data.item, data.amount, transform);
                }
            }
            inventoryControl.characterInv.CleanInventory();

            inventoryControl.quickInv.SelectSlot(0, true);

            if (inventoryControl.IsInventoryOrContainersActive())
                inventoryControl.SetInventoryVisible(false);

            UpdateCharacterPanelInfo();

            inventoryControl.characterInvPanel.SetActive(false);

            deathText.SetActive(true);
        }

        playerAudio.clip = deathClip;
        playerAudio.Play();
    }

    [Command]
    void CmdRevive()
    {
        isDead = false;
        
        currentHealth = startingHealth;       
    }

    [Command]
    void CmdHeal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;

        if (currentHealth > startingHealth)
        {
            currentHealth = startingHealth;
        }
    }

    [Command]
    void CmdSuicide()
    {
        TakeDamage(300);
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            hpSlider.value = currentHealth;
            hpText.text = currentHealth.ToString();

            if (damaged)
            {
                // ... set the colour of the damageImage to the flash colour.
                damageImg.color = flashColour;
            }
            // Otherwise...
            else
            {
                // ... transition the colour back to clear.
                damageImg.color = Color.Lerp(damageImg.color, Color.clear, flashSpeed * Time.deltaTime);

            }

            // Reset the damaged flag.
            damaged = false;

            if (Input.GetButton("Suicide"))
                CmdSuicide();

            if (isDead)
            {
                respawnTimer += Time.deltaTime;

                if (respawnTimer >= respawnTime)
                {
                    CmdRevive();
                    isDead = false;

                    inventoryControl.CmdAnimSetBool(Global.player.gameObject, "Death", isDead);
                    weaponAnimator.SetBool("Death", isDead);
                    fullness = startingFullness;
                    thirst = startingThirst;
                    stamina = startingStamina;

                    transform.position = controller.spawnPoint;
                    respawnTimer = 0;

                    AddFullness(0);
                    AddThirst(0);
                    AddStamina(0);
                    Heal(0);

                    inventoryControl.quickInv.SelectSlot(0, true);

                    deathText.SetActive(false);
                }
            }
        }
    }
}
