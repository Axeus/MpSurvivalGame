using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(Characteristics))]

public class MeleeWeapon : MonoBehaviour {

    [SerializeField]
    protected GameObject particles;

    [SerializeField]
    protected GameObject blood;

    [SerializeField]
    protected AudioClip swingClip;

    [SerializeField]
    protected MiningObj.ObjTypeEnum mineType;

    [SerializeField]
    protected float attackRange;

    protected bool isHits;
    protected bool isReturns;

    protected Characteristics characteristics;

    protected AudioSource audioSrc;
    protected Animator weaponAnimator;
    protected LayerMask layerMask;

    void Start () 
	{
        layerMask = 1 << LayerMask.NameToLayer("Character");
        layerMask = ~layerMask;
        audioSrc = GetComponent<AudioSource>();

		characteristics = GetComponentInParent<Characteristics>();
    
        if(Global.player != null)
        weaponAnimator = Global.player.GetChild(0).GetComponentInChildren<Animator>();
    }   
	
    protected GameObject Mine(MiningObj.ObjTypeEnum type, RaycastHit hit)
    {
        GameObject particles = null;
        if (hit.collider.GetComponentInParent<MiningObj>() != null)
        {
            MiningObj obj = hit.collider.GetComponentInParent<MiningObj>();
            particles = obj.particles;

            if (obj.currentHealth > 0)
            {
                if ((obj.objectType == type || obj.objectType == MiningObj.ObjTypeEnum.None))
                {
                    int damage = characteristics.power;
                    if (obj.objectType == MiningObj.ObjTypeEnum.None)
                        damage /= 2;

                    Global.inventoryControl.CmdMine(damage, obj.gameObject, Global.playerID);
                }
                else
                    Global.inventoryControl.CmdMine(1, obj.gameObject, Global.playerID);
            }     
        }
        return particles;
    }

	void Update () 
	{
        if (isHits && (weaponAnimator.GetCurrentAnimatorStateInfo(2).IsName("HorizontalSwing") || weaponAnimator.GetCurrentAnimatorStateInfo(2).IsName("VerticalSwing")) && weaponAnimator.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.99f)
        {
            isHits = false;
            isReturns = true;

            audioSrc.PlayOneShot(swingClip);

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            

            if (Physics.Raycast(ray, out hit, attackRange, layerMask))
            {
                if (hit.collider.tag == "Enemy" && hit.collider.gameObject.GetComponent<EnemyHealth>().currentHealth > 0)
                {
                    Global.inventoryControl.CmdDamage(hit.collider.gameObject, characteristics.power, transform.position, false);

                    weaponAnimator.SetInteger("AttackIndex", 1);

                    if (blood != null)
                        Instantiate(blood, hit.point, blood.transform.rotation);
                }
                else if (hit.collider.tag == "Player" && hit.collider.gameObject.GetComponentInParent<PlayerStats>().currentHealth > 0)
                {
                    Global.inventoryControl.CmdDamage(hit.collider.gameObject.GetComponentInParent<PlayerStats>().gameObject, characteristics.power, transform.position, true);

                    weaponAnimator.SetInteger("AttackIndex", 1);

                    if (blood != null)
                        Instantiate(blood, hit.point, blood.transform.rotation);
                }
                else
                {
                    GameObject sparksMine = Mine(mineType, hit);
                    weaponAnimator.SetInteger("AttackIndex", 2);

                    if (!hit.collider.isTrigger)
                    {
                        if (sparksMine != null)
                            Instantiate(sparksMine, hit.point, particles.transform.rotation);
                        else
                        if (particles != null)
                            Instantiate(particles, hit.point, particles.transform.rotation);
                    }
                }
            }
            else
            {
                weaponAnimator.SetInteger("AttackIndex", 1);
            }
        }
        else if (isReturns && (!weaponAnimator.GetCurrentAnimatorStateInfo(2).IsName("HorizontalSwing") || !weaponAnimator.GetCurrentAnimatorStateInfo(2).IsName("VerticalSwing")) && weaponAnimator.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.90f)
        {
            isReturns = false;
        }
		
		if(!Global.inventoryControl.IsInventoryOrContainersActive()  && Time.timeScale > 0 && !isHits && !isReturns)
		{
			if(Input.GetButton("Fire1"))
			{
                weaponAnimator.SetInteger("AttackIndex", 0);
                Global.inventoryControl.CmdAnimSetInteger(Global.player.gameObject, "AttackIndex", 0);
                weaponAnimator.SetTrigger("Attack");
                Global.inventoryControl.CmdAnimSetTrigger(Global.player.gameObject, "Attack");

                isHits = true;
			}
		}
    }
}
