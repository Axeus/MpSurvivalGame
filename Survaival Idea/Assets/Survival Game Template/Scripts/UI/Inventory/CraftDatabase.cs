using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftDatabase : MonoBehaviour {

	public List<Blueprint> craftDatabase = new List<Blueprint>();
}

[System.Serializable]
public class Blueprint
{
	public string itemId;
	public int itemAmount;
    public float itemCraftTime = 1f;
    public CraftPanel.subsections subsection;
    public CraftPanel.workstations workStation;
    public List<requiredItem> requiredItems = new List<requiredItem>();

    [HideInInspector]
    public string desc;

	[System.Serializable]
	public struct requiredItem
	{
		public string itemId;
		public int itemAmount;
	}
		
	public Blueprint()
	{
	}
}