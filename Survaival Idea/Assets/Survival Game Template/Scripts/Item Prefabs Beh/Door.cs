using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Door : NetworkBehaviour {   
    public float rotationSpeed = 5f;
    public GameObject door;

    [SyncVar]
    public bool rotated;
    [SyncVar]
    public Quaternion targetRotation;

    private Quaternion rot;
    private BoxCollider doorCollider;
    private Transform thisTransform;
    private BuildCollisionProperties parentProperties;

    void Start () {
        thisTransform = door.transform;
        rot = thisTransform.rotation;

        if (targetRotation != Quaternion.identity)
        {
            thisTransform.rotation = targetRotation;
        }
        else
        {
            targetRotation = rot;
        }

        doorCollider = door.GetComponent<BoxCollider>();

        parentProperties = GetComponentInParent<BuildCollisionProperties>();
    }

    public void Rotate()
    {
        if (isServer)
        {
            int degrees = 0;

            if (!rotated)
            {
                degrees = -90;
                rotated = true;
            }
            else
                rotated = false;

            targetRotation = rot;
            targetRotation *= Quaternion.Euler(0, degrees, 0);
        }

        doorCollider.isTrigger = true;
    }

    void Update()
    {
        if (parentProperties == null || parentProperties.active)
        {
            if (thisTransform.rotation != targetRotation)
            {
                thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else if (doorCollider.isTrigger)
                doorCollider.isTrigger = false;
        }
    }
}
