using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class InventoryControl : NetworkBehaviour {

    public Material ghostMaterial;
    public Transform itemsSpawner;
    public Transform itemsSpawnerLeft;

    public float buildFurnitureDistance;
    public float buildBuildingsDistance;

    public GameObject itemBag;

    [HideInInspector]
	public GameObject inventoryPanel;

	[HideInInspector]
	public Inventory quickInv;

	[HideInInspector]
	public Inventory playerInv;

	[HideInInspector]
	public Inventory characterInv;

	[HideInInspector]
	public GameObject characterInvPanel;

	[HideInInspector]
	public GameObject craftPanel;

	[HideInInspector]
	public CraftPanel craftPanelComponent;

	[HideInInspector]
	public Tooltip tooltip;

	[HideInInspector]
	public ItemDatabase database;

	[HideInInspector]
	public Container container;

	[HideInInspector]
	public bool containerActive;

	[HideInInspector]
	public PlayerStats playerStats;
    	
    [HideInInspector]
    public float buildRayDistance;

    [HideInInspector]
    public CraftPanel.workstations workStation;

    private WorkStation activeWorkstation;
    private Canvas inventoryCanvas;
    private Canvas gameCanvas;

    [HideInInspector]
    public float craftTimer;
    [HideInInspector]
    public int craftAmount;
    [HideInInspector]
    public Blueprint craftBlueprint;
    [HideInInspector]
    public Canvas menuCanvas;
    [HideInInspector]
    public GameObject gloves;

    private TempText tempText;

    void Start () 
	{
        if (isLocalPlayer)
        {
            Cursor.visible = false;

            playerStats = GetComponent<PlayerStats>();

            Global.inventoryControl = this;
            Global.playerStats = playerStats;
            Global.player = transform;
            Global.playerID = NetworkManager.FindObjectOfType<PlayerData>().playerName;

            GameObject.Find("Inventory").transform.GetChild(0).gameObject.SetActive(true);

            inventoryCanvas = GameObject.Find("Inventory").GetComponent<Canvas>();
            gameCanvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();

            workStation = CraftPanel.workstations.Hands;
            database = GameObject.Find("Database").GetComponent<ItemDatabase>();

            tooltip = GameObject.Find("Tooltip").GetComponent<Tooltip>();
            tooltip.gameObject.SetActive(false);

            inventoryPanel = GameObject.Find("Inventory Panel");
            inventoryPanel.SetActive(false);

            craftPanel = GameObject.Find("Craft Panel");
            craftPanelComponent = craftPanel.GetComponent<CraftPanel>();
            craftPanel.SetActive(false);

            playerInv = inventoryPanel.GetComponent<Inventory>();

            characterInvPanel = GameObject.Find("Character Panel");
            characterInvPanel.SetActive(false);

            characterInv = characterInvPanel.GetComponent<Inventory>();

            quickInv = GameObject.Find("Quick Panel").GetComponent<Inventory>();

            GameObject.Find("Cheat").GetComponent<Button>().onClick.AddListener(delegate { AddAllItemsToInventory(); });

            menuCanvas = GameObject.Find("MenuCanvas").GetComponent<Canvas>();

            tempText = GameObject.Find("TempText").transform.GetChild(0).GetComponent<TempText>();

            //SkinnedMeshTools.AddSkinnedMeshTo(perchi, Global.player.GetChild(0).GetComponentInChildren<Animator>().transform, false);

            AddItemsToInventory(database.database[0].ID, database.database[0].MaxStack, true);
            AddItemsToInventory("Water", 40, false);
        }
    }

	public bool CheckItemsInInventory(string ID, int count)
	{
		int amount = playerInv.IsHaveItems(ID, count);
		if(amount < count)	amount += quickInv.IsHaveItems(ID, count - amount);
		if(amount >= count) return true;
		return false;
	}

	public void RemoveItemsInInventory(string ID, int count)
	{
		int remaining = playerInv.RemoveItems(ID, count);
		if(remaining > 0) quickInv.RemoveItems(ID, remaining);

        craftPanelComponent.UpdateCraft(workStation);
    }

	public bool IsInventoryOrContainersActive()
	{
		if(containerActive) return true;
		if(inventoryPanel.activeSelf) return true;
		if(characterInvPanel.activeSelf) return true;
		if(craftPanel.activeSelf) return true;
        if (menuCanvas.enabled) return true;
		return false;
	}

	public bool CheckPlaceForItemsInInventory(string id, int count)
	{
		int amount = playerInv.CheckPlaceForItems(id, count);
		if(amount >= count) return true;

		amount += quickInv.CheckPlaceForItems(id, count-amount);
		if(amount >= count) return true;

		return false;
	}

	public void AddItemsToInventory(string id, int count, bool toQuickPanel = false)
	{
		int countAdd = 0; 
		int remainingCount = count; 
		int maxStackSize = 0;

		Item itemToAdd = database.FetchItemByID(id);

		maxStackSize=itemToAdd.MaxStack;
		countAdd = (int)Mathf.Ceil((count * 1.0f) / (maxStackSize * 1.0f));

		for(int i = 0; i < countAdd; i++)
		{
			if(remainingCount > maxStackSize)
			{
				AddItemToInventory(itemToAdd, maxStackSize, toQuickPanel);
				remainingCount -= maxStackSize;
			}
			else
				AddItemToInventory(itemToAdd, remainingCount, toQuickPanel);	
		}

        tempText.setText("Added Item: " + itemToAdd.Title + " x" + count.ToString());

        craftPanelComponent.UpdateCraft(workStation);
    }

	void AddItemToInventory(Item item, int count, bool toQuickPanel = false)
	{
		int mayTranslate = 0;
		if(!toQuickPanel)
		{
			mayTranslate = playerInv.CheckPlaceForItems(item.ID, count);
			if(mayTranslate > 0) playerInv.AddItemOnestack(item, mayTranslate);
		}
		if(mayTranslate < count) 
		{			
			mayTranslate = quickInv.CheckPlaceForItems(item.ID, count - mayTranslate);
			if(mayTranslate > 0) quickInv.AddItemOnestack(item, mayTranslate);
		}
		if(mayTranslate < count) 
		{
            DropBag(item, count - mayTranslate, transform);
        }
	}

    [Command]
    public void CmdDamage(GameObject mob,int power, Vector3 pos, bool isPlayer)
    {
        if(!isPlayer)
            mob.GetComponent<EnemyHealth>().TakeDamage(power, pos);
        else if(Global.playerStats.enablePvp)
            mob.GetComponent<PlayerStats>().TakeDamage(power);
    }

    [Command]
    public void CmdMine(int dmg, GameObject obj, string player)
    {
        obj.GetComponentInParent<MiningObj>().TakeDamage(dmg, player);
    }

    [Command]
    void CmdDropBag(Vector3 position, string ID, string title, int count)
    {
        GameObject itemBagLoc = Instantiate(itemBag, position + new Vector3(Random.value, 0, Random.value) + (Vector3.up / 3f), Quaternion.identity) as GameObject;
        itemBagLoc.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 200f);
        ItemBag bag = itemBagLoc.GetComponent<ItemBag>();
        bag.items.Add(new ItemBag.itemInBag(ID, title, count));

        NetworkServer.Spawn(itemBagLoc);
    }

    public static void ChangeLayersRecursively(Transform thisTransform)
    {
        thisTransform.gameObject.layer = LayerMask.NameToLayer("Character");
        foreach (Transform child in thisTransform)
        {
            ChangeLayersRecursively(child.transform);
        }
    }

    [Command]
    public void CmdDeleteWeapon(GameObject player)
    {
        if(player.GetComponentInChildren<ThirdPerson>().itemsSpawner.childCount != 0)
            foreach (Transform child in player.GetComponentInChildren<ThirdPerson>().itemsSpawner.transform)
                 Destroy(child.gameObject);
        if (player.GetComponentInChildren<ThirdPerson>().itemsSpawnerLeft.childCount != 0)
            foreach (Transform child in player.GetComponentInChildren<ThirdPerson>().itemsSpawnerLeft.transform)
                Destroy(child.gameObject);
    }

    [Command]
    public void CmdAnimSetInteger(GameObject player, string value, int value2)
    {
        RpcAnimSetInteger(player, value, value2);
    }

    [ClientRpc]
    void RpcAnimSetInteger(GameObject player, string value, int value2)
    {
        player.GetComponentInChildren<ThirdPerson>().GetComponent<Animator>().SetInteger(value, value2);
    }

    [Command]
    public void CmdAnimSetFloat(GameObject player, string value, float value2)
    {
        RpcAnimSetFloat(player, value, value2);
    }

    [ClientRpc]
    void RpcAnimSetFloat(GameObject player, string value, float value2)
    {
        player.GetComponentInChildren<ThirdPerson>().GetComponent<Animator>().SetFloat(value, value2);
    }

    [Command]
    public void CmdAnimSetTrigger(GameObject player, string value)
    {
        RpcAnimSetTrigger(player, value);
    }

    [ClientRpc]
    void RpcAnimSetTrigger(GameObject player, string value)
    {
        player.GetComponentInChildren<ThirdPerson>().GetComponent<Animator>().SetTrigger(value);
    }

    [Command]
    public void CmdAnimSetBool(GameObject player, string value, bool value2)
    {
        RpcAnimSetBool(player, value, value2);
    }

    [ClientRpc]
    void RpcAnimSetBool(GameObject player, string value, bool value2)
    {
        player.GetComponentInChildren<ThirdPerson>().GetComponent<Animator>().SetBool(value, value2);
    }

    [Command]
    public void CmdSpawnWeapon(string ID, Vector3 pos, Quaternion rot, GameObject player, bool rightHand)
    {
        Item item = Global.inventoryControl.database.FetchItemByID(ID);

        Transform parent = null;
        if (rightHand)
            parent = player.GetComponentInChildren<ThirdPerson>().itemsSpawner;
        else
            parent = player.GetComponentInChildren<ThirdPerson>().itemsSpawnerLeft;

        GameObject itemSelectedPrefab = Instantiate(item.itemPrefab, parent.position, parent.rotation) as GameObject;

        itemSelectedPrefab.transform.parent = parent;

        itemSelectedPrefab.transform.localPosition = item.itemPrefab.transform.position;
        itemSelectedPrefab.transform.localRotation = item.itemPrefab.transform.rotation;

        if (itemSelectedPrefab.GetComponentInChildren<Bow>() != null)
            itemSelectedPrefab.GetComponentInChildren<Bow>().arrowOnBow.GetComponent<Renderer>().enabled = true;

        foreach (Behaviour childCompnent in itemSelectedPrefab.GetComponentsInChildren<Behaviour>())
            childCompnent.enabled = false;

        itemSelectedPrefab.GetComponent<ItemInHands>().obj = player.transform.gameObject;
        itemSelectedPrefab.GetComponent<ItemInHands>().rot = parent.rotation;
        itemSelectedPrefab.GetComponent<ItemInHands>().localRot = item.itemPrefab.transform.rotation;
        itemSelectedPrefab.GetComponent<ItemInHands>().localPos = item.itemPrefab.transform.position;

        ChangeLayersRecursively(itemSelectedPrefab.transform);

        if (item.ItemType == Item.ItemTypeEnum.Weapon)
            itemSelectedPrefab.GetComponent<Characteristics>().power = item.Power;
        if (item.ItemType == Item.ItemTypeEnum.Consumable)
            itemSelectedPrefab.GetComponent<Characteristics>().power = item.Power;

        NetworkServer.Spawn(itemSelectedPrefab);
    }


    [Command]
    public void CmdSpawnArrow(GameObject player, Vector3 pos, Quaternion rot, float timer, float length, float power, float arrowPhysicsStrength)
    {
        GameObject arrowObjLoc;

        arrowObjLoc = Instantiate(GetComponentInChildren<Bow>().arrowPrefab, pos, rot) as GameObject;
        arrowObjLoc.name = "Arrow";
        arrowObjLoc.GetComponent<Arrow>().posBow = transform.position;
        arrowObjLoc.GetComponent<Arrow>().setDamage((int)((timer / length) * (power * 1.0f)));

        NetworkServer.Spawn(arrowObjLoc);

        RpcSyncArrowOnce(timer, arrowPhysicsStrength, rot, arrowObjLoc);
    }

    [ClientRpc]
    public void RpcSyncArrowOnce(float timer, float arrowPhysicsStrength, Quaternion rot, GameObject arrowObjLoc)
    {
        arrowObjLoc.transform.rotation = rot;
        Vector3 force = arrowObjLoc.transform.forward * (timer * arrowPhysicsStrength);
        arrowObjLoc.GetComponent<Rigidbody>().AddForce(force);
    }

    [Command]
    public void CmdClearContainer(GameObject container)
    {
        container.GetComponent<Container>().player = null;
        container.GetComponent<Container>().items.Clear();
    }

    [Command]
    public void CmdAddItemsToContainer(GameObject container, string ID, int count, int slot)
    {
        Container.ItemInContainer item = new Container.ItemInContainer();

        item.itemId = ID;
        item.itemAmount = count;
        item.slot = slot;

        container.GetComponent<Container>().items.Add(item);
    }

    public void DropBag(Item item, int count, Transform pos)
    {
        CmdDropBag(pos.position, item.ID, item.Title, count);
    }

    public int TranslateItemToQuickPanel(Item item, int count)
	{
		int translated = quickInv.CheckPlaceForItems(item.ID, count);

		if(translated > 0)
			quickInv.AddItemOnestack(item, translated);
		
		return translated;
	}

	public int TranslateItemToPlayerInventory(Item item, int count)
	{
		int translated = playerInv.CheckPlaceForItems(item.ID, count);

		if(translated > 0)
			playerInv.AddItemOnestack(item, translated);

		return translated;
	}

	public int TranslateItemToCharacterPanel(ItemBehavior item, int count)
	{
		Item.ItemTypeEnum equipmentType;
		Item.ItemTypeEnum itemType;

		itemType = item.item.ItemType;

		int nullSlot = -1;

		for(int i = 0; i < characterInv.items.Count; i++)
		{
			if(characterInv.items[i].isNull)
			{				
				equipmentType = characterInv.slots[i].GetComponent<EquipmentSlot>().equipmentType;

				if(equipmentType == itemType)
				{
					nullSlot = i;
					break;
				}
			}
		}

		if(nullSlot > -1)
		{
			characterInv.AddItem(item.item.ID, count, nullSlot);

			playerStats.UpdateCharacterPanelInfo();

			return count;
		}

		return 0;
	}  

	public int TranslateItemToContainer(Item item, int count)
	{
		int translated = container.containerPanelLocal.GetComponent<Inventory>().CheckPlaceForItems(item.ID, count);

		if(translated > 0)
			container.containerPanelLocal.GetComponent<Inventory>().AddItemOnestack(item, translated);

		return translated;
	}

	public void SetInventoryVisible(bool visible, WorkStation station = null)
	{       
        if (visible)
		{
            if (activeWorkstation != null)
                activeWorkstation.activeStation = false;

            if (station == null)            
                workStation = CraftPanel.workstations.Hands;
            else
                workStation = station.type;

            activeWorkstation = station;
            if (activeWorkstation != null)
                activeWorkstation.activeStation = true;

            inventoryPanel.SetActive(true);
			craftPanel.SetActive(true);
            craftPanelComponent.UpdateCraft(workStation, false);

            Cursor.visible = true;
        }
		else
		{
            if (activeWorkstation != null)
            {
                activeWorkstation.activeStation = false;
                activeWorkstation = null;
            }

            inventoryPanel.SetActive(false);
			craftPanel.SetActive(false);
			if(container != null) 
			{
				container.CloseContainer(); 
				container = null;
			}

            if (characterInvPanel.activeSelf)
                characterInvPanel.SetActive(false);

            tooltip.Deactivate();

            Cursor.visible = false;
        }
	}

    public void SetUIEnabled(bool active)
    {
        gameCanvas.enabled = active;
        inventoryCanvas.enabled = active;
        
    }

    public void AddAllItemsToInventory()
    {
        for (int i = 0; i < database.database.Count; i++)
            AddItemsToInventory(database.database[i].ID, database.database[i].MaxStack * 1);
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (!playerStats.isDead && !menuCanvas.enabled)
            {
                if(craftAmount > 0)
                {
                    craftTimer -= Time.deltaTime;

                    if(craftTimer <= 0)
                    {
                        craftAmount --;
                        craftPanelComponent.addCraftedItem();
                    }
                }

                if (Input.GetButtonDown("Inventory"))
                {
                    SetInventoryVisible(!inventoryPanel.activeSelf);
                }

                if (Input.GetButtonDown("CharPanel"))
                {
                    characterInvPanel.SetActive(!characterInvPanel.activeSelf);

                    if (!characterInvPanel.activeSelf)
                    {
                        tooltip.Deactivate();

                        if (!IsInventoryOrContainersActive())
                            Cursor.visible = false;
                    }
                    else
                        Cursor.visible = true;
                }

                if (Input.GetButtonUp("Slot1"))
                    quickInv.SelectSlot(0);

                if (Input.GetButtonUp("Slot2"))
                    quickInv.SelectSlot(1);

                if (Input.GetButtonUp("Slot3"))
                    quickInv.SelectSlot(2);

                if (Input.GetButtonUp("Slot4"))
                    quickInv.SelectSlot(3);

                if (Input.GetButtonUp("Slot5"))
                    quickInv.SelectSlot(4);

                if (Input.GetButtonUp("Slot6"))
                    quickInv.SelectSlot(5);

                if (Input.GetButtonUp("Slot7"))
                    quickInv.SelectSlot(6);

                if (Input.GetButtonUp("Slot8"))
                    quickInv.SelectSlot(7);

                if (Input.GetButtonUp("Slot9"))
                    quickInv.SelectSlot(8);

                if (Input.GetButtonUp("Slot10"))
                    quickInv.SelectSlot(9);
            }
        }
	}
}
