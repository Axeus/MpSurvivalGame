using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameTooltip))]
[RequireComponent(typeof(Collider))]

public class ItemBag : NetworkBehaviour {

    public class SyncListItems : SyncListStruct<itemInBag> {

    }
    
    public SyncListItems items = new SyncListItems();

    public List<itemInBag> items2 = new List<itemInBag>();

    private string tooltipText = "Press 'E' to collect";

    [System.Serializable]
	public struct itemInBag
	{
		public string itemID;
		public string itemName;
		public int itemAmount;

		public itemInBag( string id, string name, int amount)
		{
			itemID = id;
            itemName = name;
			itemAmount = amount;
		}
	}


    void Start () 
	{
        if (isServer)
        {
            for (int i = 0; i < items2.Count; i++)
            {
                items.Add(items2[i]);
            }
        }

        string tooltip = GetComponent<GameTooltip>().tooltip;

		if(tooltip == "")
		{
            for (int i = 0; i < items.Count; i++)
            {
                string itemName = items[i].itemName;
                if (itemName == "")
                    itemName = items[i].itemID;
                GetComponent<GameTooltip>().tooltip += itemName + ": " + items[i].itemAmount + "\n";
            }

			GetComponent<GameTooltip>().tooltip += tooltipText;
		}
	}

	public void AddToInventory(InventoryControl control)
	{
		for(int i = 0; i < items.Count; i++)
            control.AddItemsToInventory(items[i].itemID, items[i].itemAmount);
	}

	public void RemoveBag()
	{
		Destroy(gameObject);
	}

    void OnDestroy()
    {
        if (transform.parent != null && transform.parent.childCount < 2)
            Destroy(transform.parent.gameObject);
    }
}
