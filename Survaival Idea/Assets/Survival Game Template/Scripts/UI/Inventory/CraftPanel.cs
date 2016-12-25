using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CraftPanel : MonoBehaviour {

    public GameObject craftButton;
    public GameObject categoryButton;
    public GameObject categoryPanel;

    private CraftDatabase craftDatabase;
    private ItemDatabase database;    
    private GameObject contentPanel;
    private GameObject tooltipPanel;
    private GameObject tooltipContentPanel;

    private Blueprint activeBlueprint;
    private InputField inputAmount;

    private Text craftText;
    private Slider craftSlider;
    private TempText tempText;
    private Image craftItemImage;

    private string spaceText = "Not enough space!";
    private string itemsText = "Not enough items!";

    public enum subsections
    {
        Armor,
        Weapon,
        Tools,
        Materials,
        Containers,
        Food,
        Furniture
    }

    [System.Flags]
    public enum workstations : byte
    {
        Hands = 1,
        Workbench = 2,
        Furnace = 4
    }

    void Awake()
    {
        tempText = GameObject.Find("TempText").transform.GetChild(0).GetComponent<TempText>();
        craftItemImage = GameObject.Find("CraftItem").transform.GetChild(0).GetComponent<Image>();
        craftItemImage.transform.parent.gameObject.SetActive(false);

        craftDatabase = GameObject.Find("Database").GetComponent<CraftDatabase>();
        database = GameObject.Find("Database").GetComponent<ItemDatabase>();

        tooltipPanel = transform.GetChild(1).gameObject;
        tooltipContentPanel = tooltipPanel.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        craftSlider = tooltipPanel.transform.GetChild(4).GetComponent<Slider>();
        craftText = craftSlider.transform.GetChild(2).GetComponent<Text>();

        contentPanel = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        inputAmount = tooltipPanel.transform.GetChild(1).GetComponent<InputField>();

        tooltipPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { Craft(); });
        tooltipPanel.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CancelCraft(); });

        for (int i = 0; i < System.Enum.GetValues(typeof(subsections)).Length; i++)
        {
            GameObject catBut = Instantiate(categoryButton);
            GameObject catPanel = Instantiate(categoryPanel);

            catBut.transform.SetParent(contentPanel.transform, false);
            catPanel.transform.SetParent(contentPanel.transform, false);

            CategoryPanel catPanelComponent = catPanel.GetComponent<CategoryPanel>();

            catPanelComponent.categoryBut = catBut.GetComponent<Button>();
            catPanelComponent.subsection = (subsections)System.Enum.GetValues(typeof(subsections)).GetValue(i);

            catBut.transform.GetChild(0).GetComponent<Text>().text = System.Enum.GetValues(typeof(subsections)).GetValue(i).ToString();

            catPanelComponent.Init();
        }

        tooltipPanel.SetActive(false);
    }

    public void UpdateCraft(CraftPanel.workstations workStation, bool reOpen = true)
    {
        if (gameObject.activeSelf)
        {           
            foreach (Transform child in contentPanel.transform)
            {
                CategoryPanel catPanel = child.GetComponent<CategoryPanel>();
                if (catPanel != null)
                {                   
                    catPanel.UpdatePanel(workStation);
                }
            }
            OpenTooltip(null, reOpen);
        }
    }

    public void OpenTooltip(Blueprint blueprint, bool reOpen = false)
    {
        if (!reOpen)
        {
            if (blueprint == null)
                tooltipPanel.SetActive(false);
            else if (activeBlueprint == blueprint)
                tooltipPanel.SetActive(!tooltipPanel.activeSelf);  
            else
                tooltipPanel.SetActive(true);
        }

        if (tooltipPanel.activeSelf)
        {
            if (!reOpen)
            {
                activeBlueprint = blueprint;
                inputAmount.text = "1";
            }
            else
                blueprint = activeBlueprint;

            tooltipPanel.transform.GetChild(0).GetComponent<ScrollRect>().verticalScrollbar.value = 1;

            Item item = database.FetchItemByID(blueprint.itemId);

            tooltipContentPanel.transform.GetChild(0).GetComponent<Image>().sprite = item.Sprite;
            tooltipContentPanel.transform.GetChild(1).GetComponent<Text>().text = Global.inventoryControl.tooltip.GetDataString(item);

            Text items = tooltipContentPanel.transform.GetChild(2).GetComponent<Text>();
            items.text = "Components:\n";

            for (int i = 0; i < activeBlueprint.requiredItems.Count; i++)
            {
                Item itemRequired = database.FetchItemByID(blueprint.requiredItems[i].itemId);
                string color = "<color=#ffffff>";

                if (!Global.inventoryControl.CheckItemsInInventory(blueprint.requiredItems[i].itemId, blueprint.requiredItems[i].itemAmount))
                    color = "<color=#828282>";

                items.text += color + itemRequired.Title + ": ";
                items.text += blueprint.requiredItems[i].itemAmount.ToString() + "</color>\n";
            }
        }
    }

    public void InputAmountChange(int change)
    {
        int count = 1;
        int.TryParse(inputAmount.text, out count);

        if (Input.GetButton("TranslateItem"))
            count += change * 10;
        else
            count += change;

        if (count <= 0)
            count = 0;

        inputAmount.text = count.ToString();
    }

    void Craft()
	{		
		int count = 1;
		int.TryParse(inputAmount.text, out count);

        if (Global.inventoryControl.craftAmount > 0 && Global.inventoryControl.craftBlueprint == activeBlueprint)
            Global.inventoryControl.craftAmount += count;
        else
        {
            Global.inventoryControl.craftAmount = 0;

            for (int j = 0; j < activeBlueprint.requiredItems.Count; j++)
            {
                if (!Global.inventoryControl.CheckItemsInInventory(activeBlueprint.requiredItems[j].itemId, activeBlueprint.requiredItems[j].itemAmount))
                {
                    tempText.setText(itemsText);
                    return;
                }
            }

            if (Global.inventoryControl.CheckPlaceForItemsInInventory(activeBlueprint.itemId, activeBlueprint.itemAmount))
            {
                Global.inventoryControl.craftAmount = count;
                Global.inventoryControl.craftTimer = activeBlueprint.itemCraftTime;
                Global.inventoryControl.craftBlueprint = activeBlueprint;
            }
            else
                tempText.setText(spaceText);

            craftSlider.maxValue = Global.inventoryControl.craftTimer;

            if (Global.inventoryControl.craftAmount > 0)
            {
                craftItemImage.transform.parent.gameObject.SetActive(true);
                craftItemImage.sprite = database.FetchItemByID(Global.inventoryControl.craftBlueprint.itemId).Sprite;
            }
            else
                craftItemImage.transform.parent.gameObject.SetActive(false);
        }
    }

    public void addCraftedItem()
    {
        for (int j = 0; j < Global.inventoryControl.craftBlueprint.requiredItems.Count; j++)
        {
            if (!Global.inventoryControl.CheckItemsInInventory(Global.inventoryControl.craftBlueprint.requiredItems[j].itemId, Global.inventoryControl.craftBlueprint.requiredItems[j].itemAmount))
            {
                Global.inventoryControl.craftAmount = 0;
                Global.inventoryControl.craftTimer = 0f;

                tempText.setText(itemsText);
                return;
            }
        }

        if (Global.inventoryControl.CheckPlaceForItemsInInventory(Global.inventoryControl.craftBlueprint.itemId, Global.inventoryControl.craftBlueprint.itemAmount))
        {
            for (int j = 0; j < Global.inventoryControl.craftBlueprint.requiredItems.Count; j++)
                Global.inventoryControl.RemoveItemsInInventory(Global.inventoryControl.craftBlueprint.requiredItems[j].itemId, Global.inventoryControl.craftBlueprint.requiredItems[j].itemAmount);

            Global.inventoryControl.AddItemsToInventory(Global.inventoryControl.craftBlueprint.itemId, Global.inventoryControl.craftBlueprint.itemAmount);

            Global.inventoryControl.craftTimer = Global.inventoryControl.craftBlueprint.itemCraftTime;
        }
        else
        {
            Global.inventoryControl.craftAmount = 0;
            Global.inventoryControl.craftTimer = 0f;

            tempText.setText(spaceText);
        }

        craftSlider.maxValue = Global.inventoryControl.craftTimer;

        if (Global.inventoryControl.craftAmount <= 0)
            craftItemImage.transform.parent.gameObject.SetActive(false);
    }

    void Update()
    {
        craftSlider.value = craftSlider.maxValue - Global.inventoryControl.craftTimer;

        if (Global.inventoryControl.craftAmount > 0)
            craftText.text = Global.inventoryControl.craftAmount.ToString();
        else
            craftText.text = "";
    }

    void CancelCraft()
    {
        Global.inventoryControl.craftAmount = 0;
        Global.inventoryControl.craftTimer = 0f;
        craftSlider.maxValue = 0;
        craftItemImage.transform.parent.gameObject.SetActive(false);
    }
}
