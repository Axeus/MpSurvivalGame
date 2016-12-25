using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bow : MonoBehaviour {

	public GameObject arrowPrefab;
	public GameObject arrowOnBow;
    public Transform arrowSpawner;
    public string arrowItemID;
    public float arrowPhysicsStrength;

	public AudioClip bowClip;

    private Renderer arrowOnBowRenderer;

    private AudioSource bowAudio;
    private float length;

    private bool isActive;
    private float timer;

    private Characteristics characteristics;
    private Animator weaponAnimator;
    private Animator bowAnimator;

    void Start () 
	{
		bowAudio = GetComponent <AudioSource> ();
		characteristics = GetComponent<Characteristics>();

		arrowOnBowRenderer = arrowOnBow.GetComponent<Renderer>();

        weaponAnimator = Global.player.GetChild(0).GetComponentInChildren<Animator>();
        bowAnimator = GetComponent<Animator>();

        foreach (AnimationClip clip in bowAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Tension")
            {
                length = clip.length;
            }
        }
    }

	void Update () 
	{
		if(isActive)
		{
			timer += Time.deltaTime;
			if(timer > length) timer = length;
		}
		if(!Global.inventoryControl.IsInventoryOrContainersActive() && Time.timeScale > 0)
		{
			if(Input.GetButtonDown("Fire1") && !isActive)
			{
				if(Global.inventoryControl.CheckItemsInInventory(arrowItemID, 1))
				{
                    Global.inventoryControl.RemoveItemsInInventory(arrowItemID, 1);

                    bowAnimator.SetTrigger("Attack");
                    weaponAnimator.SetTrigger("Attack");
                    Global.inventoryControl.CmdAnimSetTrigger(Global.player.gameObject, "Attack");

                    arrowOnBowRenderer.enabled = true;
					isActive = true;
				}
			}
			if(Input.GetButtonUp("Fire1") && isActive)
			{
                bowAnimator.SetTrigger("AttackBow");
                weaponAnimator.SetTrigger("AttackBow");
                Global.inventoryControl.CmdAnimSetTrigger(Global.player.gameObject, "AttackBow");

                Global.inventoryControl.CmdSpawnArrow(Global.player.gameObject, arrowSpawner.transform.position, arrowSpawner.transform.rotation, timer, length, characteristics.power, arrowPhysicsStrength);

                timer = 0;
				arrowOnBowRenderer.enabled = false;
				isActive = false;

                bowAudio.PlayOneShot(bowClip);
            }
		}
	}
}
