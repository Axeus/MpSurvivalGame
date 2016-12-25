using UnityEngine;
using System.Collections;

public class MoonControl : MonoBehaviour {

    public float distance = 1000f;
    public float scale = 15f;

	// Use this for initialization
	void Start () {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, distance);
        transform.localScale = new Vector3(scale, scale, scale);
	}

}
