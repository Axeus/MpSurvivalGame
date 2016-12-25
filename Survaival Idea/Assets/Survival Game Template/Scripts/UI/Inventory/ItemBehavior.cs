using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class ItemBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler{

	public Item item;
	public int amount;
	public int slot;	
	public bool dragSplit;	
	public bool dragged;

    private GameObject draggingItem;

    private Vector2 downPos;
    private Vector2 offset;
    private Inventory inv;
    private RectTransform rectTrans;

    void Start()
	{
		draggingItem = GameObject.Find("DraggingItem Panel").gameObject;

		if(inv == null)
			inv = transform.parent.parent.parent.GetComponent<Inventory>();

        rectTrans = GetComponent<Image>().rectTransform;
    }

	void OnGUI()
	{
		if(dragged)
			if(item != null)
                rectTrans.position = new Vector3(Event.current.mousePosition.x - offset.x, (-Event.current.mousePosition.y - offset.y) + Screen.height, 0);       
	}

    void Update()
    {
        if (Input.GetButtonUp("DropItem"))
        {
            if (dragged)
                if (item != null)
                {
                    Global.inventoryControl.DropBag(item, amount, Global.player.transform);
                    Destroy(gameObject);                
                }
        }
    }

    public void setInv(Inventory inv2)
	{
		inv = inv2;
	}

	public void ReleaseItem()
	{
		if(slot == inv.slotSelected)
			inv.SelectSlot(slot, true);

		dragSplit = false;
		dragged = false;

		transform.SetParent(inv.slots[slot].transform);
        transform.SetAsFirstSibling();
        transform.position = inv.slots[slot].transform.position;

		GetComponent<CanvasGroup>().blocksRaycasts = true;

        Global.inventoryControl.craftPanelComponent.UpdateCraft(Global.inventoryControl.workStation);
    }

	public void GrabItem(Vector2 position, bool maySplit, bool deleteItem)
	{
		if (item != null) 
		{ 
			offset = position - new Vector2(this.transform.position.x, this.transform.position.y); 
			bool split = false;
            Global.inventoryControl.tooltip.Deactivate();
			
			if (maySplit)
			{
				if (item.Stackable && amount > 1)
				{
					split = true;

					GameObject halfStack = Instantiate(inv.inventoryItem);
					ItemBehavior halfData = halfStack.GetComponent<ItemBehavior>();
					halfData.item = item;
					halfData.slot = slot;

					halfStack.transform.SetParent(inv.slots[slot].transform);
					halfData.setInv(inv);

					halfStack.transform.GetChild(0).GetComponent<Image>().sprite = item.Sprite;
					halfStack.name = item.Title;
										
					halfData.amount = amount / 2;
					amount = amount - amount / 2;
					
					halfStack.transform.GetChild(1).GetComponent<Text>().text = halfData.amount.ToString();
					transform.GetChild(1).GetComponent<Text>().text = amount.ToString();

					halfStack.transform.SetParent(draggingItem.transform);
					halfStack.transform.position = position - offset;

					halfData.GetComponent<CanvasGroup>().blocksRaycasts = false;
					
					halfData.dragged=true;
					halfData.dragSplit = true;
				}
			}
			if(!split)
			{
				transform.SetParent(draggingItem.transform);
				transform.position = position - offset;

				GetComponent<CanvasGroup>().blocksRaycasts = false;

				if(deleteItem)
					inv.items[slot] = new Item();
				
				dragged=true;

				if(inv.InventoryType == Inventory.InventoryTypeEnum.CharacterPanel)
                    Global.inventoryControl.playerStats.UpdateCharacterPanelInfo();
			}
            Global.inventoryControl.craftPanelComponent.UpdateCraft(Global.inventoryControl.workStation);
        } 
	}

	void TranslateTo(int invType)
	{
		if(inv.InventoryType != Inventory.InventoryTypeEnum.QuickPanel)
		{
			int translated = 0;

			if(invType == 0)
				translated = Global.inventoryControl.TranslateItemToQuickPanel(item, amount);
			else if(invType == 1)
				translated = Global.inventoryControl.TranslateItemToContainer(item, amount);
			else if(invType == 2)
				translated = Global.inventoryControl.TranslateItemToPlayerInventory(item, amount);
			else if(invType == 3)
				translated = Global.inventoryControl.TranslateItemToQuickPanel(item, amount);
			else if(invType == 4)
				translated = Global.inventoryControl.TranslateItemToCharacterPanel(this, amount);

			if(translated > 0)
			{
				amount -= translated;
				transform.GetChild(1).GetComponent<Text>().text = amount.ToString();

				if(amount <= 0)
				{
                    Global.inventoryControl.tooltip.Deactivate();

					inv.items[slot] = new Item();
					Destroy(gameObject);
				}
			}
			if(amount > 0 && invType == 2)
				TranslateTo(3);
		}
		else
		{
			int translated = 0;

			if(invType == 0)
				translated = Global.inventoryControl.TranslateItemToPlayerInventory(item, amount);
			else if(invType == 1)
				translated = Global.inventoryControl.TranslateItemToContainer(item, amount);
			else if(invType == 4)
				translated = Global.inventoryControl.TranslateItemToCharacterPanel(this, amount);

			if(translated > 0)
			{
				amount -= translated;
				transform.GetChild(1).GetComponent<Text>().text = amount.ToString();

				if(amount <= 0)
				{
                    Global.inventoryControl.tooltip.Deactivate();
					inv.items[slot] = new Item();

					if(slot==inv.slotSelected)
						inv.SelectSlot(slot, true);

					Destroy(gameObject);
				}
			}
		}
	}

	void Translate()
	{
		if(Global.inventoryControl.container != null)
		{
			if(inv.InventoryType != Inventory.InventoryTypeEnum.Container)
				TranslateTo(1);
			else
				TranslateTo(2);
		}
		else
		{
			if(Global.inventoryControl.characterInv.gameObject.activeSelf)
			{
				if(inv.InventoryType != Inventory.InventoryTypeEnum.CharacterPanel)
					TranslateTo(4);
				else
				{
					TranslateTo(2);
                    Global.inventoryControl.playerStats.UpdateCharacterPanelInfo();
				}
			}
			else TranslateTo(0);
		}
        Global.inventoryControl.craftPanelComponent.UpdateCraft(Global.inventoryControl.workStation);
    }

	public void OnPointerClick (PointerEventData eventData)
	{
		if (eventData.pointerId == -2)
		{
			{
				if(Global.inventoryControl.IsInventoryOrContainersActive())
				if (item != null) 
				{ 
					if(draggingItem.transform.childCount <= 0)				
						GrabItem(eventData.position, true, true);
				}
			}
		}
		else if (eventData.pointerId == -1 && Global.inventoryControl.IsInventoryOrContainersActive())
		{
			if (item != null) 
			{ 
				if(draggingItem.transform.childCount <= 0)
				{
					if (Input.GetButton("TranslateItem"))
						Translate();
					else
					{
						GrabItem(eventData.position, false, true);

						if(slot == inv.slotSelected)
							inv.SelectSlot(slot, true);
					}				
				}
				else
				{
					ItemBehavior droppedItem = draggingItem.transform.GetChild(0).GetComponent<ItemBehavior>();

					if (droppedItem.item.ID == item.ID && droppedItem.item.Stackable)
					{
						int localCount = 0;

						if(amount + droppedItem.amount > item.MaxStack)
							localCount = item.MaxStack - amount;
						else
							localCount = droppedItem.amount;

						amount += localCount;
						droppedItem.amount -= localCount;

						transform.GetChild(1).GetComponent<Text>().text = amount.ToString();
						droppedItem.transform.GetChild(1).GetComponent<Text>().text = droppedItem.amount.ToString();

						if(droppedItem.amount <= 0)
							Destroy(droppedItem.gameObject);

						return;
					}	

					int thisSlot=slot;
					int droppedSlot = droppedItem.slot;

						if(inv.InventoryType == Inventory.InventoryTypeEnum.CharacterPanel)
						{
							if(inv.slots[thisSlot].GetComponent<EquipmentSlot>().equipmentType != droppedItem.item.ItemType)
							{
								return;
							}
						}

					droppedItem.slot = thisSlot;
					inv.items[thisSlot] = droppedItem.item;

					droppedItem.setInv(inv);
						
					if(droppedSlot == inv.slotSelected)
						inv.SelectSlot(droppedSlot, true);

					droppedItem.ReleaseItem();
					GrabItem(eventData.position,false,false);	

					if(inv.InventoryType == Inventory.InventoryTypeEnum.CharacterPanel)
                        Global.inventoryControl.playerStats.UpdateCharacterPanelInfo();
				}
			} 
		}
	}

	public void OnPointerEnter (PointerEventData eventData)
	{
		if(draggingItem.transform.childCount <=0 && Global.inventoryControl.IsInventoryOrContainersActive())
            Global.inventoryControl.tooltip.Activate(item);
	}

	public void OnPointerExit (PointerEventData eventData)
	{
        Global.inventoryControl.tooltip.Deactivate();
	}
}
