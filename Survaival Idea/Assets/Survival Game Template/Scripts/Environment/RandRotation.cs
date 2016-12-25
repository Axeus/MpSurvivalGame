using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RandRotation : NetworkBehaviour
{

    void Start()
    {
        if (!isServer)
        {
            Vector3 euler = transform.eulerAngles;
            euler.y = Random.Range(0f, 360f);
            transform.eulerAngles = euler;            
        }
    }
}
