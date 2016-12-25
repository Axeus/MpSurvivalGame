using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

	private Item item;
	private string data;

	void Start()
	{
	}

	void Update()
	{
		if(gameObject.activeSelf)
            transform.position = Input.mousePosition;
	}

	public void Activate(Item item)
	{
		this.item = item;
		ConstructDataString();

        gameObject.SetActive(true);
	}

	public void Deactivate()
	{
        gameObject.SetActive(false);
	}

	public string GetDataString(Item item)
	{
		this.item = item;
		return ConstructDataString();
	}

	public string ConstructDataString()
	{
		data = "<color=#ffffff><b>" + item.Title + "</b></color>\n\n";
		data += "<color=#C4C4C4>" + item.Description + "</color>";
        if (item.Power > 0)
        {
            if(item.ItemType == Item.ItemTypeEnum.Consumable)
                data += "\n\n<color=#FAF200>Restore: " + item.Power.ToString() + "</color>";
            else
                data += "\n\n<color=#FAF200>Damage: " + item.Power.ToString() + "</color>";
        }
		if(item.Defence > 0) data += "\n\n<color=#FAF200>Defence: " + item.Defence.ToString() + "</color>";
        if (item.Placeable) data += "\n\n<color=#dfdfdf>Can be placed</color>";


        transform.position = Input.mousePosition;
        transform.GetChild(0).GetComponent<Text>().text = data;

		return data;
	}
}
