using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CategoryPanel : MonoBehaviour {

	public CraftPanel.subsections subsection;    
	public GameObject craftButton;  
      
    [HideInInspector]
	public Button categoryBut;

    private GameObject craftPanel;
    private CraftPanel craft;

    private Blueprint activeBlueprint;
    private InputField inputAmount;

    private CraftDatabase craftDatabase;
    private ItemDatabase database;

    public void Init()
    {
        craftDatabase = GameObject.Find("Database").GetComponent<CraftDatabase>();
        database = GameObject.Find("Database").GetComponent<ItemDatabase>();

        craftPanel = GameObject.Find("Craft Panel").gameObject;
        craft = craftPanel.GetComponent<CraftPanel>();

        categoryBut.onClick.AddListener(delegate { OpenCategory(gameObject); });

        for (int i = 0; i < craftDatabase.craftDatabase.Count; i++)
        {
            Blueprint blueprint = craftDatabase.craftDatabase[i];

            if (blueprint.subsection == subsection)
            {
                Item item = database.FetchItemByID(blueprint.itemId);
                GameObject craftObj = Instantiate(craftButton);

                craftObj.transform.SetParent(transform, false);

                string desc = item.Title;
                if (blueprint.itemAmount > 1) desc += " x" + blueprint.itemAmount;
                craftObj.transform.GetChild(0).GetComponent<Text>().text = desc;

                blueprint.desc = desc;
                craftObj.transform.GetChild(1).GetComponent<Image>().sprite = item.Sprite;
                craftObj.GetComponent<CraftButton>().itemID = item.ID;

                craftObj.GetComponent<Button>().onClick.AddListener(delegate { craft.OpenTooltip(blueprint); });
            }
        }
    }

    public void UpdatePanel(CraftPanel.workstations workStation)
    {
        bool mainActive = false;

        foreach (Transform child in transform)
        {
            for (int i = 0; i < craftDatabase.craftDatabase.Count; i++)
            {                
                Blueprint blueprint = craftDatabase.craftDatabase[i];
                string ID = child.GetComponent<CraftButton>().itemID;
   
                if (blueprint.itemId == ID)
                {
                    bool active = true;
                    string color = "<color=#ffffff>";

                    if ((blueprint.workStation & workStation) != 0)
                    {
                        for (int j = 0; j < blueprint.requiredItems.Count; j++)
                        {
                            if (!Global.inventoryControl.CheckItemsInInventory(blueprint.requiredItems[j].itemId, blueprint.requiredItems[j].itemAmount))
                            {
                                color = "<color=#828282>";
                                break;
                            }
                        }
                    }
                    else
                        active = false;

                    child.transform.GetChild(0).GetComponent<Text>().text = color + blueprint.desc + "</color>";

                    GameObject obj = child.gameObject;

                    if (obj.activeSelf && !active)
                        craftPanel.transform.GetChild(0).GetComponent<ScrollRect>().verticalScrollbar.value = 1;

                    obj.SetActive(active);

                    if (active)                                           
                        mainActive = true;
                    
                    break;
                }
            }
        }
        categoryBut.gameObject.SetActive(mainActive);
    }

	void OpenCategory(GameObject panel)
	{
		GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
		grid.enabled = !grid.enabled;

		if(grid.enabled)
			categoryBut.transform.GetChild(1).GetComponent<Text>().text = "-";
		else
			categoryBut.transform.GetChild(1).GetComponent<Text>().text = "+";
	}
}
