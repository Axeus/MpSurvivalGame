using UnityEngine;
using System.Collections;

public class ThirdPerson : MonoBehaviour {

    public Transform itemsSpawner;
    public Transform itemsSpawnerLeft;
    PlayerStats stats;
    PlayerController controller;
    Animator anim;

    void OnAnimatorIK()
    {
        if (!stats.isDead)
        {
            anim.SetLookAtWeight(0.5f, 0.5f);
            anim.SetLookAtPosition(controller.pos);
        }
    }

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
        stats = GetComponentInParent<PlayerStats>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
