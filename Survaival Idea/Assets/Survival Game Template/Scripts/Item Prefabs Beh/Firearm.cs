using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(Characteristics))]

public class Firearm : MonoBehaviour
{
    public GameObject flash;
    [SerializeField]
    protected GameObject sparks;

    [SerializeField]
    protected AudioClip shotClip;

    [SerializeField]
    protected float attackRange;

    [SerializeField]
    protected GameObject blood;

    public string ammoItemID;
    public float aimFOV = 30f;
    public float aimFOVSpeed = 5f;

    protected bool isActive;

    protected Characteristics characteristics;

    protected AudioSource audioSrc;
    private Animator weaponAnimator;
    private Animator rifleAnimator;
    private float timer;
    private float length;
    private LayerMask layerMask;
    private bool aim;
    private float defaultFOV;
    private Vector3 aimPos;
    private Quaternion aimRot;
    private Vector3 defPos;
    private Quaternion defRot;
    private Camera cameraMain;
    private Camera cameraWeapon;
    private Transform handsTransform;


    void Start()
    {        
       layerMask = 1 << LayerMask.NameToLayer("Character");
        layerMask = ~layerMask;
        audioSrc = GetComponent<AudioSource>();

        weaponAnimator = Global.player.GetChild(0).GetComponentInChildren<Animator>();
        rifleAnimator = GetComponent<Animator>();

        defRot = weaponAnimator.transform.localRotation;
        defPos = weaponAnimator.transform.localPosition;

        aimRot = weaponAnimator.transform.localRotation * Quaternion.Euler(355.6f, 1.3f, 0f);
        aimPos = weaponAnimator.transform.localPosition + new Vector3(-0.16f, 0, 0);

        cameraMain = Camera.main;
        cameraWeapon = Camera.main.transform.GetChild(0).GetComponent<Camera>();
        handsTransform = weaponAnimator.transform;

        characteristics = GetComponentInParent<Characteristics>();

        foreach (AnimationClip clip in weaponAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "FirstPerson_TwoHandsRifle_Attack")
            {
                length = clip.length;
            }
        }

        defaultFOV = cameraMain.fieldOfView;
    }

    void OnDestroy()
    {
        if (characteristics != null)
        {
            cameraMain.fieldOfView = defaultFOV;
            cameraWeapon.fieldOfView = defaultFOV;
            handsTransform.localPosition = defPos;
            handsTransform.localRotation = defRot;
        }
    }

    void Update()
    {
        if (isActive)
        {
            timer += Time.deltaTime;
            if (timer > length)
            {
                timer = 0f;
                isActive = false;
            }
        }

        if (Input.GetButton("Fire2"))
        {
            aim = true;
        }
        else
            aim = false;

        if(aim)
        {
            float lerp = Mathf.Lerp(cameraMain.fieldOfView, aimFOV, aimFOVSpeed * Time.deltaTime);
            cameraMain.fieldOfView = lerp;
            cameraWeapon.fieldOfView = lerp;
            handsTransform.localPosition = Vector3.Lerp(handsTransform.localPosition, aimPos, aimFOVSpeed * Time.deltaTime);
            handsTransform.localRotation = Quaternion.Lerp(handsTransform.localRotation, aimRot, aimFOVSpeed * Time.deltaTime);
        }
        else
        {
            float lerp = Mathf.Lerp(cameraMain.fieldOfView, defaultFOV, aimFOVSpeed * Time.deltaTime);
            cameraMain.fieldOfView = lerp;
            cameraWeapon.fieldOfView = lerp;
            handsTransform.localPosition = Vector3.Lerp(handsTransform.localPosition, defPos, aimFOVSpeed * Time.deltaTime);
            handsTransform.localRotation = Quaternion.Lerp(handsTransform.localRotation, defRot, aimFOVSpeed * Time.deltaTime);
        }

        if (!Global.inventoryControl.IsInventoryOrContainersActive() && Time.timeScale > 0 && !isActive)
        {
            if (Input.GetButton("Fire1"))
            {
                if (Global.inventoryControl.CheckItemsInInventory(ammoItemID, 1))
                {
                    GameObject flashLoc = null;
                    if (flash != null)
                        flashLoc = Instantiate(flash,  flash.transform.position, flash.transform.rotation) as GameObject;

                    flashLoc.transform.SetParent(transform, false);

                    Global.inventoryControl.RemoveItemsInInventory(ammoItemID, 1);

                    rifleAnimator.SetTrigger("Attack");
                    weaponAnimator.SetTrigger("Attack");
                    Global.inventoryControl.CmdAnimSetTrigger(Global.player.gameObject, "Attack");

                    audioSrc.PlayOneShot(shotClip);

                    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, attackRange, layerMask))
                    {
                        if (hit.collider.tag == "Enemy" && hit.collider.gameObject.GetComponent<EnemyHealth>().currentHealth > 0)
                        {
                            Global.inventoryControl.CmdDamage(hit.collider.gameObject, characteristics.power, transform.position, false);
                            if (blood != null)
                                Instantiate(blood, hit.point, blood.transform.rotation);
                        }
                        else if (hit.collider.tag == "Player" && hit.collider.gameObject.GetComponentInParent<PlayerStats>().currentHealth > 0)
                        {
                            Global.inventoryControl.CmdDamage(hit.collider.gameObject.GetComponentInParent<PlayerStats>().gameObject, characteristics.power, transform.position, true);
                            if (blood != null)
                                Instantiate(blood, hit.point, blood.transform.rotation);
                        }
                        else
                        {
                            if (!hit.collider.isTrigger)
                            {
                                if (sparks != null)
                                    Instantiate(sparks, hit.point, sparks.transform.rotation);
                            }
                        }
                    }

                    isActive = true;
                }
            }
        }
    }
}

