using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Arrow : NetworkBehaviour {

	[SerializeField] private AudioClip hitSound;

    [SerializeField]
    private GameObject blood;

    private LayerMask layerMask;

	private bool impacted = false;
	private bool isActive = true;
	private Rigidbody arrowRigidbody;
	
	private AudioSource audioSource;
    private Transform thisTransform;

    [SyncVar]
	private int damage;
    private LayerMask layers;
    [SyncVar]
    [HideInInspector]
    public Vector3 posBow;
	
	public void setDamage(int dmg)
	{
		damage = dmg;
	}

	void Start() 
	{ 
		arrowRigidbody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
        thisTransform = transform;

		layers = 1 << 0;
	}	

	void FixedUpdate() 
	{
		if(isActive)
		{
			if(arrowRigidbody.velocity != Vector3.zero)
				arrowRigidbody.rotation = Quaternion.LookRotation(arrowRigidbody.velocity);

            Vector3 fwd = thisTransform.TransformDirection(Vector3.forward);

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(thisTransform.position, fwd * 1, out hit, 1f, layers))
            {
                Impact(hit.point, hit.normal, hit.collider.gameObject);
            }
            
		}
	}

    /*
	void OnCollisionEnter(Collision collision) 
	{
		if(isActive && collide)
		{
			if(impacted == true) 
			{
				return;
			}

			///Impact(collision.contacts[0].point, collision.contacts[0].normal, collision.gameObject);
			//impacted = true;
		}
	}
    */

	void Impact(Vector3 pos, Vector3 normal, GameObject hitObject) 
	{
        GetComponent<CapsuleCollider>().isTrigger = true;
        arrowRigidbody.isKinematic = true;
		isActive = false;

        thisTransform.position = pos;

		if(hitObject.tag == "Enemy")
		{
            gameObject.GetComponent<Renderer>().enabled = false;

            if (isServer)
            {
                Global.inventoryControl.CmdDamage(hitObject, damage, posBow, false);
                Destroy(gameObject);
                if (blood != null)
                {
                    GameObject bloodGO = Instantiate(blood, pos, blood.transform.rotation) as GameObject;

                    NetworkServer.Spawn(bloodGO);
                }
            }
		}
        else if (hitObject.tag == "Player")
        {
            gameObject.GetComponent<Renderer>().enabled = false;

            if (isServer)
            {
                Global.inventoryControl.CmdDamage(hitObject.gameObject.GetComponentInParent<PlayerStats>().gameObject, damage, posBow, true);
                Destroy(gameObject);
                if (blood != null)
                {
                    GameObject bloodGO = Instantiate(blood, pos, blood.transform.rotation) as GameObject;

                    NetworkServer.Spawn(bloodGO);
                }
            }
        }
        else
		{
			audioSource.clip = hitSound;
			audioSource.Play();
            if (isServer)
            {
                Destroy(gameObject, 20);
            }
		}
	}
}
