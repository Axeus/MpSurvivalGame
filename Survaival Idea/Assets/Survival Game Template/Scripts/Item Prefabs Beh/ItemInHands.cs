using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ItemInHands : NetworkBehaviour
{
    public int AnimationIndex;
    public bool rightHand = true;

    [HideInInspector]
    [SyncVar]
    public GameObject obj;
    [HideInInspector]
    [SyncVar]
    public Quaternion rot;
    [HideInInspector]
    [SyncVar]
    public Quaternion localRot;
    [HideInInspector]
    [SyncVar]
    public Vector3 localPos;
    // Use this for initialization
    public static void ChangeLayersRecursively(Transform thisTransform)
    {
        thisTransform.gameObject.layer = LayerMask.NameToLayer("Character");
        foreach (Transform child in thisTransform)
        {
            ChangeLayersRecursively(child.transform);
        }
    }

    void Start()
    {
        if (!isServer && transform.parent == null)
        {
            foreach (Behaviour childCompnent in gameObject.GetComponentsInChildren<Behaviour>())
                childCompnent.enabled = false;

            transform.rotation = rot;
            if(rightHand)
                transform.parent = obj.GetComponentInChildren<ThirdPerson>().itemsSpawner;
            else
                transform.parent = obj.GetComponentInChildren<ThirdPerson>().itemsSpawnerLeft;

            if (GetComponentInChildren<Bow>() != null)
                GetComponentInChildren<Bow>().arrowOnBow.GetComponent<Renderer>().enabled = true;

            transform.localPosition = localPos;
            transform.localRotation = localRot;

            ChangeLayersRecursively(transform);
        }
    }
}
