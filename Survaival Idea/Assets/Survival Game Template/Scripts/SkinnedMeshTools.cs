using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public static class SkinnedMeshTools
{
    // Return the list of all the new skinned mesh renderer added to root. Set recursively obj as inactive.
    public static List<GameObject> AddSkinnedMeshTo(GameObject obj, Transform root) { return AddSkinnedMeshTo(obj, root, true); }
    // Return the list of all the new skinned mesh renderer added to root. Set recursively obj as inactive if hideFromObj is true.
    public static List<GameObject> AddSkinnedMeshTo(GameObject obj, Transform root, bool hideFromObj)
    {
        List<GameObject> result = new List<GameObject>();

        // Here, boneObj must be instatiated and active (at least the one with the renderer),
        // or else GetComponentsInChildren won't work.
        SkinnedMeshRenderer[] BonedObjects = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in BonedObjects)
            result.Add(ProcessBonedObject(smr, root));

        if (hideFromObj)
            obj.SetActiveRecursively(false);

        return result;
    }

    private static GameObject ProcessBonedObject(SkinnedMeshRenderer ThisRenderer, Transform root)
    {
        // Create the SubObject
        GameObject newObject = new GameObject(ThisRenderer.gameObject.name);
        newObject.transform.SetParent(root, false);

        // Add the renderer
        SkinnedMeshRenderer NewRenderer = newObject.AddComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;

        // Assemble Bone Structure	
        Transform[] MyBones = new Transform[ThisRenderer.bones.Length];

        // As clips are using bones by their names, we find them that way.
        for (int i = 0; i < ThisRenderer.bones.Length; i++)
            MyBones[i] = FindChildByName(ThisRenderer.bones[i].name, root);

        // Assemble Renderer	
        NewRenderer.bones = MyBones;
        NewRenderer.sharedMesh = ThisRenderer.sharedMesh;
        NewRenderer.sharedMaterials = ThisRenderer.sharedMaterials;
        NewRenderer.gameObject.layer = ThisRenderer.gameObject.layer;
        NewRenderer.updateWhenOffscreen = true;
        newObject.AddComponent<NetworkIdentity>();

        

        return newObject;
    }

    // Recursive search of the child by name.
    private static Transform FindChildByName(string ThisName, Transform ThisGObj)
    {
        Transform ReturnObj;

        // If the name match, we're return it
        if (ThisGObj.name == ThisName)
            return ThisGObj.transform;

        // Else, we go continue the search horizontaly and verticaly
        foreach (Transform child in ThisGObj)
        {
            ReturnObj = FindChildByName(ThisName, child);

            if (ReturnObj != null)
                return ReturnObj;
        }

        return null;
    }
}