using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler{

    [HideInInspector]
	public int id;

	private Inventory inv;
    private GameObject draggingItem;

	void Start()
	{
		draggingItem = GameObject.Find("DraggingItem Panel").gameObject;
		inv = transform.parent.parent.GetComponent<Inventory>();
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		if (eventData.pointerId == -1 && draggingItem.transform.childCount > 0  && Global.inventoryControl.IsInventoryOrContainersActive())
		{			
			ItemBehavior droppedItem = draggingItem.transform.GetChild(0).GetComponent<ItemBehavior>();

			if(inv.items[id].isNull)
			{
				if(inv.InventoryType == Inventory.InventoryTypeEnum.CharacterPanel)
				{
					Item.ItemTypeEnum equipmentType;
					Item.ItemTypeEnum itemType;
					equipmentType = GetComponent<EquipmentSlot>().equipmentType;
					itemType = droppedItem.item.ItemType;

					if(equipmentType != itemType)
						return;
				}

				inv.items[id] = droppedItem.item;

				if(droppedItem.slot==inv.slotSelected)
					inv.SelectSlot(droppedItem.slot, true);

				droppedItem.slot = id;
				droppedItem.setInv(inv);
				droppedItem.ReleaseItem();

				if(inv.InventoryType == Inventory.InventoryTypeEnum.CharacterPanel)
                    Global.inventoryControl.playerStats.UpdateCharacterPanelInfo();

			}
		}
	}	
}
