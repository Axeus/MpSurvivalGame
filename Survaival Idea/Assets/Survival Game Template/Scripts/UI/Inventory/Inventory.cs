using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

	public InventoryTypeEnum InventoryType;

	public GameObject inventorySlot;
	public GameObject inventoryItem;
	public int slotAmount;
    
	public List<Item>items = new List<Item>();

    [HideInInspector]
    public ItemDatabase database;

    [HideInInspector]
	public GameObject itemPlaceableGhost;

	[HideInInspector]
	public BuildCollisionProperties itemPlaceableGhostCheck;

    [HideInInspector]
	public List<GameObject>slots = new List<GameObject>();

	[HideInInspector]
	public GameObject inventoryPanel;

	[HideInInspector]
	public int slotSelected =- 1;

	[HideInInspector]
	public GameObject itemSelectedPrefab;

	[HideInInspector]
	public GameObject containerPrefab;

	[HideInInspector]
	public GameObject slotPanel;

    private GameObject selectedSlot;

    public enum InventoryTypeEnum
    {
        PlayerInventory,
        QuickPanel,
        Container,
        CharacterPanel
    }
    void Awake()
	{
        if (database == null)
			database = GameObject.Find("Database").GetComponent<ItemDatabase>();

		inventoryPanel = transform.gameObject;
		slotPanel = transform.GetChild(0).gameObject;

		if(InventoryType == InventoryTypeEnum.CharacterPanel)
		{
			for(int i = 0; i < slotAmount; i++)
			{
				items.Add(new Item());
				slots.Add(slotPanel.transform.GetChild(i).gameObject);
				slots[i].GetComponent<Slot>().id=i;
			}
		}
		else
			for(int i = 0; i < slotAmount; i++)
			{
				items.Add(new Item());
				slots.Add(Instantiate(inventorySlot));
				slots[i].GetComponent<Slot>().id=i;

                slots[i].transform.SetParent(slotPanel.transform,false);
			}

		if(InventoryType == InventoryTypeEnum.QuickPanel)
		{
			selectedSlot = GameObject.Find("Selected Slot").gameObject;
			SelectSlot(0);
        }
	}

	public int RemoveItems(string id, int count)
	{
		int remainingCount = count; //The remaining amount of items

        for (int i = 0; i < items.Count; i++)
		{
			if (items[i].ID == id) 
			{
				ItemBehavior data = slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();

				if(data.amount >= remainingCount)
				{
					data.amount -= remainingCount;
					data.transform.GetComponentInChildren<Text>().text = data.amount.ToString();
					remainingCount = 0;
				}
				else
				{
					remainingCount -= data.amount;
					data.amount = 0;
				}

				if(data.amount <= 0)
				{
					items[i] = new Item();

					if(InventoryType == InventoryTypeEnum.QuickPanel)
					{
						if(i==slotSelected)
							SelectSlot(i, true);
					}
				
					GameObject t = slots[i].transform.GetChild(0).gameObject;

					t.transform.SetParent(null);
					Destroy(t);
				}

				if(remainingCount <= 0) return 0;
			}
		}

		return (remainingCount);
	}

	public void CleanInventory()
	{
		for(int i = 0; i < items.Count; i++)
		{
			if(!items[i].isNull)
			{
				items[i] = new Item();
				Transform t = slots[i].transform.GetChild(0);
				Destroy(t.gameObject);
			}
		}
	}

    public static void ChangeLayersRecursively(Transform thisTransform)
	{
        thisTransform.gameObject.layer = LayerMask.NameToLayer("ItemGhost");
		foreach(Transform child in thisTransform)
			ChangeLayersRecursively(child.transform);
	}

    public void ChangeMaterialsRecursively(Transform thisTransform)
    {    
        Renderer r = thisTransform.GetComponent<Renderer>();

        if (r != null)
        {
            Material[] sharedMaterialsCopy = r.sharedMaterials;

            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                sharedMaterialsCopy[i] = Global.inventoryControl.ghostMaterial;
            }

            r.sharedMaterials = sharedMaterialsCopy;
        }

        foreach (Transform child in thisTransform)
            ChangeMaterialsRecursively(child.transform);
    }

    public void SelectSlot(int slot, bool reSelect = false)
	{
		if(InventoryType == InventoryTypeEnum.QuickPanel)
		{
			if(slotSelected == slot && !reSelect) return;

            if (itemSelectedPrefab != null)
            {
                Destroy(itemSelectedPrefab);
                Global.inventoryControl.CmdDeleteWeapon(Global.player.gameObject);
            }
            if (itemPlaceableGhost != null)
                Destroy(itemPlaceableGhost);

            if (!Global.playerStats.isDead)
            {
                if (items[slot].itemPrefab != null)
                {
                    itemSelectedPrefab = Instantiate(items[slot].itemPrefab, Global.inventoryControl.itemsSpawner.position, Global.inventoryControl.itemsSpawner.rotation) as GameObject;

                    if (itemSelectedPrefab.GetComponent<ItemInHands>().rightHand)
                        itemSelectedPrefab.transform.parent = Global.inventoryControl.itemsSpawner;
                    else
                        itemSelectedPrefab.transform.parent = Global.inventoryControl.itemsSpawnerLeft;

                    itemSelectedPrefab.transform.localPosition = items[slot].itemPrefab.transform.position;
                    itemSelectedPrefab.transform.localRotation = items[slot].itemPrefab.transform.rotation;

                    if (items[slot].ItemType == Item.ItemTypeEnum.Weapon)
                        itemSelectedPrefab.GetComponent<Characteristics>().power = items[slot].Power;
                    if (items[slot].ItemType == Item.ItemTypeEnum.Consumable)
                        itemSelectedPrefab.GetComponent<Characteristics>().power = items[slot].Power;

                    Global.player.GetChild(0).GetComponentInChildren<Animator>().SetInteger("WeaponIndex", itemSelectedPrefab.GetComponent<ItemInHands>().AnimationIndex);
                    Global.inventoryControl.CmdAnimSetInteger(Global.player.gameObject, "WeaponIndex", itemSelectedPrefab.GetComponent<ItemInHands>().AnimationIndex);

                    Global.inventoryControl.CmdSpawnWeapon(items[slot].ID, Global.inventoryControl.itemsSpawner.position, Global.inventoryControl.itemsSpawner.rotation, Global.player.gameObject, itemSelectedPrefab.GetComponent<ItemInHands>().rightHand);
                }
                else
                {
                    Global.player.GetChild(0).GetComponentInChildren<Animator>().SetInteger("WeaponIndex", 0);
                    Global.inventoryControl.CmdAnimSetInteger(Global.player.gameObject, "WeaponIndex", 0);
                }
                if (items[slot].itemPlaceablePrefab != null)
                {
                    itemPlaceableGhost = Instantiate(items[slot].itemPlaceablePrefab) as GameObject;

                    ChangeMaterialsRecursively(itemPlaceableGhost.transform);

                    itemPlaceableGhostCheck = itemPlaceableGhost.GetComponentInChildren<BuildCollisionProperties>();
                    itemPlaceableGhostCheck.active = false;
                    ChangeLayersRecursively(itemPlaceableGhost.transform);

                    if (itemPlaceableGhostCheck.isBuilding)
                        Global.inventoryControl.buildRayDistance = Global.inventoryControl.buildBuildingsDistance;                   
                    else
                        Global.inventoryControl.buildRayDistance = Global.inventoryControl.buildFurnitureDistance;
                }
            }

            slotSelected = slot;
            selectedSlot.transform.SetParent(slots[slotSelected].transform, false);
            selectedSlot.transform.SetAsLastSibling();
        }
	}

	public int IsHaveItems(string id, int count)
	{
		int amount = 0;

		for(int i = 0; i < items.Count; i++)
		{
			if (items[i].ID == id)
			{
				ItemBehavior data = slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();
				amount += data.amount;
			}
			if(amount >= count)
				return count;
		}
		return amount;
	}
		
	public int CheckPlaceForItems(string id, int count)
	{
		Item item = database.FetchItemByID(id);
		int amount = 0; //The remaining amount of items

        for (int i = 0; i < items.Count; i++)
		{
			if (items[i].ID == id)
			{
				ItemBehavior data = slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();
				int locAmount = item.MaxStack - data.amount;
				if(locAmount >= count)
				{
					return count;
				}
				else
					amount += locAmount;
			}
			else if (items[i].isNull)
				amount += item.MaxStack;

			if(amount >= count)
				return count;
		}
		return amount;
	}

	public void AddItem(string id, int count, int toSlot = -1)
	{
		int countAdd = 0; 
		int remainingCount = count; //The remaining amount of items
        int maxStackSize = 0; //The maximum number of items in one stack

        Item itemToAdd = database.FetchItemByID(id);

		maxStackSize = itemToAdd.MaxStack;
		countAdd = Mathf.CeilToInt((count * 1.0f) / (maxStackSize * 1.0f));

		for(int i = 0; i < countAdd; i++)
		{
			if(remainingCount>maxStackSize)
			{
				AddItemOnestack(itemToAdd, maxStackSize, toSlot);
				remainingCount -= maxStackSize;
			}
			else
				AddItemOnestack(itemToAdd, remainingCount, toSlot);
		}
	}
	
	public void AddItemOnestack(Item item, int count, int toSlot = -1)
	{
		int nullSlotID = toSlot; //Number of empty slot
        int remainingCount = count; //The remaining amount of items

        if (toSlot == -1)
		for(int i = 0; i < items.Count; i++)
		{
			if(items[i].ID == item.ID)
			{
				ItemBehavior data = slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();
				if(data.amount < item.MaxStack)
				{
					int localCount;
					
					if(remainingCount + data.amount > item.MaxStack)
						localCount = item.MaxStack-data.amount;
					else
						localCount = remainingCount;
					
					data.amount += localCount;
					data.transform.GetChild(1).GetComponent<Text>().text = data.amount.ToString();
					remainingCount -= localCount;
					
					if(remainingCount <= 0)return;
				}
			}
			else if(nullSlotID < 0 && items[i].isNull)
			{
				nullSlotID = i;
			}
		}

		items[nullSlotID] = item;

		GameObject itemObj = Instantiate(inventoryItem);
		ItemBehavior itemData = itemObj.GetComponent<ItemBehavior>();

		itemData.item = item;
		itemData.slot = nullSlotID;
		itemData.amount = remainingCount;
		if(item.Stackable)itemData.transform.GetChild(1).GetComponent<Text>().text = itemData.amount.ToString();

		itemObj.transform.SetParent(slots[nullSlotID].transform, false);
		itemObj.transform.localPosition = Vector3.zero;
		itemObj.transform.GetChild(0).GetComponent<Image>().sprite = item.Sprite;
        itemObj.transform.SetAsFirstSibling();
        itemObj.name = item.Title;		

		if(InventoryType == InventoryTypeEnum.QuickPanel)
		{
			if(nullSlotID == slotSelected)
				SelectSlot(nullSlotID, true);
		}
	}
}
