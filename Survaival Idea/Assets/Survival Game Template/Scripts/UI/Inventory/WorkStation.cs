using UnityEngine;

using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(WorkStation))]
public class WorkstationsEditor : Editor
{
    public SerializedProperty tileTypeMask;

    void OnEnable()
    {
        tileTypeMask = serializedObject.FindProperty("type");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        tileTypeMask.intValue = (int)((CraftPanel.workstations)EditorGUILayout.EnumMaskField("type", (CraftPanel.workstations)tileTypeMask.intValue));

        serializedObject.ApplyModifiedProperties();
    }
}

#endif

[RequireComponent(typeof(GameTooltip))]
[RequireComponent(typeof(Collider))]

public class WorkStation : MonoBehaviour {
    
    public CraftPanel.workstations type = 0;

    public bool activeStation;

    private string tooltipText = "Press 'E' to use";

    void Start()
    {
        string tooltip = GetComponent<GameTooltip>().tooltip;
        if (tooltip == "")
            GetComponent<GameTooltip>().tooltip = tooltipText;
    }

    void Update()
    {
        if (activeStation)
        {
            if (Vector3.Distance(Global.player.transform.position, transform.position) > 4f)
            {
                if (Global.inventoryControl.IsInventoryOrContainersActive())
                    Global.inventoryControl.SetInventoryVisible(false);
            }
        }
    }

}

