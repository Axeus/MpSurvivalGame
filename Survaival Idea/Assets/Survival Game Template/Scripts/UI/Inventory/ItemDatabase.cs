using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System;

public class ItemDatabase : MonoBehaviour {

    public int defaultMaxStack = 64;
    public bool showDefaultItemPrefab;

    public List<Item> database = new List<Item>();
    
    private JsonData itemData;

    void Awake()
	{
        TextAsset ItemsFile = Resources.Load<TextAsset>("Items");

        try
        {
            database = JsonMapper.ToObject<List<Item>>(ItemsFile.text);
        }
        catch (Exception exception)
        {           
            Debug.LogError("Please check the file Items.json for errors");
            Debug.LogError(exception.Message);
        }

        for (int i = 0; i < database.Count; i++)
			database[i].setProperties(defaultMaxStack, showDefaultItemPrefab);
	}

	public Item FetchItemByID(string id)
	{
		for(int i =0;i<database.Count;i++)
		{
			if(database[i].ID == id)
				return database[i];
		}

		return null;
	}
}

public class Item
{
	public bool isNull;

	public string ID { get; set; }
	public string Title { get; set; }
    public string Description { get; set; }
    public int Power { get; set; }
	public int Defence { get; set; }	
	public bool Stackable { get; set; }
	public string Slug { get; set; }
    public string PlaceableSlug { get; set; }
    public Sprite Sprite { get; set; }
	public int MaxStack { get; set; }
	public GameObject itemPrefab;
	public GameObject itemPlaceablePrefab;
	public ItemTypeEnum ItemType { get; set; }
	public bool Placeable { get; set; }

	public enum ItemTypeEnum {
		Torso,
		Head,
		Legs,
		Feet,
		Gloves,
		Weapon,
		Consumable,
		Other
	}

    public void setProperties(int defaultMaxStack, bool showDefaultItemPrefab)
    {        
        if (Slug == null)
            Slug = ID;

        if (PlaceableSlug == null)
            PlaceableSlug = ID;

        Sprite = Resources.Load<Sprite>("Sprites/Items/" + Slug);
        if (Sprite == null)
            Sprite = Resources.Load<Sprite>("Sprites/Items/Empty");

        itemPrefab = Resources.Load<GameObject>("Item Prefabs/" + Slug);

        if(showDefaultItemPrefab && itemPrefab == null)
            itemPrefab = Resources.Load<GameObject>("Item Prefabs/Empty");

        if (Placeable)
        {
            itemPlaceablePrefab = Resources.Load<GameObject>("Item Placeable Prefabs/" + PlaceableSlug);
            if (itemPlaceablePrefab == null)
                Placeable = false;
        }

        if (MaxStack == 0)
        {
            if (Stackable)
                MaxStack = defaultMaxStack;
            else
                MaxStack = 1;
        }

        isNull = false;     
	}

	public Item()
	{
		isNull = true;
	}
}