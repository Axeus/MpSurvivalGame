using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(GameTooltip))]
public class Container : NetworkBehaviour {

    public class SyncListItems : SyncListStruct<ItemInContainer>
    {

    }

    public SyncListItems items = new SyncListItems();

    public List<ItemInContainer> items2 = new List<ItemInContainer>();

    [SerializeField]
    private GameObject containerPanel;

    [HideInInspector]
    public GameObject containerPanelLocal;

    private GameObject invPanel;

    private Inventory inv;
    [SyncVar]
    public GameObject player;

    private string tooltipText = "Press 'E' to open";

    [System.Serializable]
    public struct ItemInContainer
    {
        public string itemId;
        public int itemAmount;
        public int slot;
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

        invPanel = GameObject.Find("Inventory").transform.GetChild(0).gameObject;

        string tooltip = GetComponent<GameTooltip>().tooltip;
        if (tooltip == "")
            GetComponent<GameTooltip>().tooltip = tooltipText;
	}

	void Update () 
	{
		if(isServer && containerPanelLocal != null)
		{
			if(Vector3.Distance(player.transform.position, transform.position) > 4f)
				CloseContainer();
		}
	}

	public void OpenContainer()
	{
		containerPanelLocal = (Instantiate(containerPanel));

		inv = containerPanelLocal.GetComponent<Inventory>();

		containerPanelLocal.transform.SetParent(invPanel.transform, false);
		containerPanelLocal.transform.SetSiblingIndex(2);

		for(int i = 0; i < items.Count; i++)
			inv.AddItem(items[i].itemId, items[i].itemAmount, items[i].slot);

        Global.inventoryControl.containerActive = true;
	}

	public void CloseContainer()
	{
		if(inv.items.Count > 0)
		{
            Global.inventoryControl.CmdClearContainer(gameObject);

			for(int i = 0; i < inv.items.Count; i++)
			{
				if(!inv.items[i].isNull)
				{
					ItemInContainer item = new ItemInContainer();
					ItemBehavior invItemData = inv.slots[i].transform.GetChild(0).GetComponent<ItemBehavior>();

					item.itemId = inv.items[i].ID;
					item.itemAmount = invItemData.amount;
					item.slot = invItemData.slot;

                    Global.inventoryControl.CmdAddItemsToContainer(gameObject, item.itemId, item.itemAmount, item.slot);
				}
			}
		}

		Destroy(containerPanelLocal);
		inv = null;

        Global.inventoryControl.containerActive = false;
        Global.inventoryControl.container = null;
        Global.inventoryControl.tooltip.Deactivate();
	}

    public override void OnNetworkDestroy()
    {
        if(player != null && player == Global.player.gameObject)
        {
            Destroy(containerPanelLocal);

            Global.inventoryControl.containerActive = false;
            Global.inventoryControl.container = null;
            Global.inventoryControl.tooltip.Deactivate();
        }
    }
}
