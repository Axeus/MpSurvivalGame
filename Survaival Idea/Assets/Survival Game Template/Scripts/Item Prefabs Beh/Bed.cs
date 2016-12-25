using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameTooltip))]

public class Bed : MonoBehaviour {

    private string tooltipText = "Press 'E' to set the spawn point";  

    void Start () {
        string tooltip = GetComponent<GameTooltip>().tooltip;
        if (tooltip == "")
            GetComponent<GameTooltip>().tooltip = tooltipText;
    }
}
