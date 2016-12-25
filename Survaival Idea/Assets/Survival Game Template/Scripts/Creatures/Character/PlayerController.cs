using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Networking;




[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    //public Transform bone;

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

	private InventoryControl inventoryControl;
    private LayerMask layers;
    private LayerMask buildLayers;

    private float delta;
    private Text gameTooltipText;
    private TempText tempText;
    private float timer;
    private PlayerStats playerStats;
    public Vector3 spawnPoint { get; private set; }
    private Animator weaponAnimator;
    Transform sun;

    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_AudioSource = GetComponent<AudioSource>();
        inventoryControl = GetComponent<InventoryControl>();

        playerStats = GetComponent<PlayerStats>();
        spawnPoint = transform.position;

        layers = (1 << 0 | 1 << LayerMask.NameToLayer("Arrow"));
        buildLayers = 1 << 0;

        if (isLocalPlayer)
        {
            weaponAnimator = transform.GetChild(0).GetComponentInChildren<Animator>();
            transform.GetChild(0).gameObject.SetActive(true);

            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;

            m_MouseLook.Init(transform, m_Camera.transform);

            gameTooltipText = GameObject.Find("GameTooltipText").transform.GetChild(0).GetComponent<Text>();
            tempText = GameObject.Find("TempText").transform.GetChild(0).GetComponent<TempText>();
            sun = GameObject.Find("SunLight").transform;            
        }
        else
        {
            foreach (Transform child in transform.GetChild(1))
            {
                child.gameObject.SetActive(true);

                foreach (Transform child2 in child)
                    child2.gameObject.SetActive(true);
            }
        }
    }

	public static void ChangeLayersRecursively(Transform thisTransform)
	{
        thisTransform.gameObject.layer = 0;
		foreach(Transform child in thisTransform)
		{
			ChangeLayersRecursively(child.transform);
		}
	}

    [Command] 
    void CmdSpawnBuild(string prefabID, Vector3 pos, Quaternion quat)
    {
        GameObject itemPlaceablePrefab = Instantiate(Global.inventoryControl.database.FetchItemByID(prefabID).itemPlaceablePrefab, pos, quat) as GameObject;
        itemPlaceablePrefab.GetComponentInChildren<Rigidbody>().isKinematic = true;
        itemPlaceablePrefab.GetComponentInChildren<BuildCollisionProperties>().active = true;
        itemPlaceablePrefab.GetComponentInChildren<BuildCollisionProperties>().direction = quat;
        itemPlaceablePrefab.GetComponentInChildren<Collider>().isTrigger = false;

        if (itemPlaceablePrefab.GetComponentInChildren<MeshCollider>() != null)
            itemPlaceablePrefab.GetComponentInChildren<MeshCollider>().convex = false;

        foreach (Transform child in itemPlaceablePrefab.transform)
        {
            child.gameObject.SetActive(true);
        }

        NetworkServer.Spawn(itemPlaceablePrefab);     
    }

    void Build(Ray ray, RaycastHit hitInfo)
    {
        if (inventoryControl.quickInv.items[inventoryControl.quickInv.slotSelected].itemPlaceablePrefab != null
                   && inventoryControl.quickInv.itemPlaceableGhost.activeSelf)
        {
            if (Physics.Raycast(ray, out hitInfo, inventoryControl.buildRayDistance, layers))
            {
                Vector3 pos = hitInfo.point;
                pos += new Vector3(0, 0.01f, 0);

                if (inventoryControl.quickInv.itemPlaceableGhostCheck.collide == 0)
                {
                    bool place = true;

                    if (hitInfo.collider.GetComponent<BuildCollisionProperties>() != null && !hitInfo.collider.GetComponent<BuildCollisionProperties>().canObjectsBePlacedOnThis)
                        place = false;

                    if (place)
                    {
                        delta = 0;
                        

                        CmdSpawnBuild(inventoryControl.quickInv.items[inventoryControl.quickInv.slotSelected].ID, inventoryControl.quickInv.itemPlaceableGhost.transform.position, inventoryControl.quickInv.itemPlaceableGhost.transform.rotation);

                        int slot = inventoryControl.quickInv.slotSelected;

                        ItemBehavior data = inventoryControl.quickInv.slots[slot].transform.GetChild(0).GetComponent<ItemBehavior>();

                        data.amount -= 1;

                        if (data.amount <= 0)
                        {
                            inventoryControl.quickInv.items[slot] = new Item();

                            inventoryControl.quickInv.SelectSlot(slot, true);

                            GameObject t = inventoryControl.quickInv.slots[slot].transform.GetChild(0).gameObject;

                            t.transform.SetParent(null);
                            Destroy(t);
                        }
                        else
                            data.transform.GetComponentInChildren<Text>().text = data.amount.ToString();
                    }
                }
            }
        }
    }

    void GhostBuild(Ray ray, RaycastHit hitInfo)
    {
        if (inventoryControl.quickInv.items[inventoryControl.quickInv.slotSelected].itemPlaceablePrefab != null)
        {
            if (Physics.Raycast(ray, out hitInfo, inventoryControl.buildRayDistance, buildLayers))
            {
                if (!inventoryControl.quickInv.itemPlaceableGhost.activeSelf)
                    inventoryControl.quickInv.itemPlaceableGhost.SetActive(true);

                Vector3 pos = hitInfo.point;
                pos += new Vector3(0, 0.03f, 0);

                if (inventoryControl.quickInv.itemPlaceableGhostCheck.collide > 0)
                    inventoryControl.ghostMaterial.color = new Color(1, 0, 0, 0.5f);
                else
                    inventoryControl.ghostMaterial.color = new Color(0, 1, 0, 0.5f);

                Transform trans = inventoryControl.quickInv.itemPlaceableGhost.transform;

                Vector3 euler = trans.eulerAngles;
                euler.y = delta;
                trans.eulerAngles = euler;

                trans.position = pos;
            }

            else if (inventoryControl.quickInv.itemPlaceableGhost.activeSelf)
            {
                inventoryControl.ghostMaterial.color = new Color(1, 0, 0, 0.5f);
                Transform trans = inventoryControl.quickInv.itemPlaceableGhost.transform;

                Vector3 euler = trans.eulerAngles;
                euler.y = delta;
                trans.eulerAngles = euler;
                trans.position = ray.GetPoint(inventoryControl.buildRayDistance);
            }
        }
    }

    [Command]
    void CmdRemoveBag(GameObject bag)
    {
        if(bag != null)
            bag.GetComponentInChildren<ItemBag>().RemoveBag();
    }

    [ClientRpc]
    void RpcRotateDoor(GameObject door)
    {
        door.GetComponent<Door>().Rotate();
    }

    [Command]
    void CmdRotateDoor(GameObject door)
    {
        RpcRotateDoor(door);
    }

    [Command]
    void CmdSetContainerPlayer(GameObject container, GameObject player)
    {
        container.GetComponent<Container>().player = player;
    }

    [Command]
    void CmdSetPos(GameObject player, Vector3 position)
    {
        player.GetComponent<PlayerController>().pos = position;
    }


    [SyncVar]
    public Vector3 pos;

    private void Update()
    {
        if (isLocalPlayer)
        {
            sun.position = transform.position;

            //bone.rotation = Camera.main.transform.rotation;

            gameTooltipText.text = "";

            if (!playerStats.isDead && !inventoryControl.menuCanvas.enabled)
            {
                Ray rayTooltip = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hitTooltip = new RaycastHit();
                pos = rayTooltip.GetPoint(1);
                CmdSetPos(Global.player.gameObject, rayTooltip.GetPoint(1));

                if (!inventoryControl.IsInventoryOrContainersActive() && Physics.Raycast(rayTooltip, out hitTooltip, 3f, layers))
                {
                    GameTooltip gameTooltip = hitTooltip.collider.gameObject.GetComponent<GameTooltip>();
                    if (gameTooltip != null)
                    {
                        gameTooltipText.text = gameTooltip.tooltip;
                    }
                }

                GhostBuild(rayTooltip, hitTooltip);

                if (Input.GetButtonDown("Use"))
                {                  
                    if (inventoryControl.container != null)
                    {
                        inventoryControl.container.CloseContainer();
                        inventoryControl.container = null;
                        inventoryControl.SetInventoryVisible(false);
                    }
                    else
                    {
                        if (Physics.Raycast(rayTooltip, out hitTooltip, 3f, layers))
                        {
                            if (hitTooltip.collider.gameObject.GetComponent<Container>() != null)
                            {
                                Container container = hitTooltip.collider.gameObject.GetComponent<Container>();

                                if (container.player == null)
                                {
                                    CmdSetContainerPlayer(container.gameObject, gameObject);

                                    container.OpenContainer();
                                    inventoryControl.container = container;
                                    inventoryControl.SetInventoryVisible(true);
                                }
                                else
                                    tempText.setText("Container occupied!");
                            }
                            else if (hitTooltip.collider.gameObject.GetComponent<WorkStation>() != null)
                            {
                                WorkStation workStation = hitTooltip.collider.gameObject.GetComponent<WorkStation>();

                                if (workStation.activeStation)
                                    inventoryControl.SetInventoryVisible(false);
                                else
                                    inventoryControl.SetInventoryVisible(true, workStation);
                            }
                            else if (hitTooltip.collider.gameObject.GetComponent<ItemBag>() != null)
                            {
                                ItemBag bag = hitTooltip.collider.gameObject.GetComponent<ItemBag>();
                                bag.AddToInventory(inventoryControl);
                                GameObject bagGO = bag.GetComponentInParent<NetworkIdentity>().gameObject;
                                bagGO.SetActive(false);
                                CmdRemoveBag(bagGO);
                            }
                            else if (hitTooltip.collider.gameObject.GetComponent<Bed>() != null)
                            {
                                Bed bed = hitTooltip.collider.gameObject.GetComponent<Bed>();

                                spawnPoint = bed.transform.position;
                                tempText.setText("New spawn point set!");
                            }
                            else if (hitTooltip.collider.transform.parent != null && hitTooltip.collider.transform.parent.GetComponent<Door>() != null)
                            {
                                Door door = hitTooltip.collider.transform.parent.GetComponent<Door>();
                                CmdRotateDoor(door.gameObject);                                
                            }
                        }
                    }
                }

                if (Input.GetButtonUp("Fire1"))
                {
                    if (!inventoryControl.IsInventoryOrContainersActive())
                        Build(rayTooltip, hitTooltip);
                }

                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                    delta += Input.GetAxis("Mouse ScrollWheel") * 50f;

                if (!inventoryControl.IsInventoryOrContainersActive())
                {
                    RotateView();
                }

                // the jump state needs to read here to make sure it is not missed
                if (!m_Jump)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }
               

                if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
                {
                    StartCoroutine(m_JumpBob.DoBobCycle());
                    PlayLandingSound();
                    m_MoveDir.y = 0f;
                    m_Jumping = false;
                    inventoryControl.CmdAnimSetBool(Global.player.gameObject, "IsLanding", true);
                }
                if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
                {
                    m_MoveDir.y = 0f;
                }
                

                m_PreviouslyGrounded = m_CharacterController.isGrounded;
            }
        }
    }

    private void PlayLandingSound()
    {
        CmdPlaySound(2);
        m_NextStep = m_StepCycle + .5f;
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            float speed = 0;

            if (!playerStats.isDead)
            {
                GetInput(out speed);

                if (m_Input.x != 0 || m_Input.y != 0)
                {
                    if (!m_IsWalking)
                    {
                        if (!weaponAnimator.GetBool("Walk"))
                        {
                            inventoryControl.CmdAnimSetBool(Global.player.gameObject, "Walk", true);
                            weaponAnimator.SetBool("Walk", true);
                        }
                        if (weaponAnimator.GetFloat("Speed") < 1)
                        {
                            weaponAnimator.SetFloat("Speed", 1);
                            inventoryControl.CmdAnimSetFloat(Global.player.gameObject, "Speed", 1);
                        }

                        playerStats.Tired(10f * playerStats.staminaSpeed);
                    }
                    else
                    {
                        if (!weaponAnimator.GetBool("Walk"))
                        {
                            inventoryControl.CmdAnimSetBool(Global.player.gameObject, "Walk", true);
                            weaponAnimator.SetBool("Walk", true);
                        }
                        if (weaponAnimator.GetFloat("Speed") > 0)
                        {
                            weaponAnimator.SetFloat("Speed", 0);
                            inventoryControl.CmdAnimSetFloat(Global.player.gameObject, "Speed", 0);
                        }

                        playerStats.AddStamina(playerStats.staminaSpeed);
                    }
                }
                else
                {
                    if (weaponAnimator.GetBool("Walk"))
                    {
                        inventoryControl.CmdAnimSetBool(Global.player.gameObject, "Walk", false);
                        weaponAnimator.SetBool("Walk", false);
                    }
                    playerStats.AddStamina(playerStats.staminaSpeed * 5f);
                }
            }

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                                m_CharacterController.height / 2f);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                    inventoryControl.CmdAnimSetTrigger(Global.player.gameObject, "Jump");
                    inventoryControl.CmdAnimSetBool(Global.player.gameObject, "IsLanding", false);
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);
        }
    }

    private void PlayJumpSound()
    {
        CmdPlaySound(1);
    }

    [Command]
    void CmdPlaySound(int type)
    {
        RpcPlaySound(type);
    }

    [ClientRpc]
    void RpcPlaySound(int type)
    {
        switch (type)
        {
            case 1:
                m_AudioSource.PlayOneShot(m_JumpSound);
                break;
            case 2:
                m_AudioSource.PlayOneShot(m_LandSound);
                break;
            case 3:
                m_AudioSource.PlayOneShot(m_FootstepSounds[Random.Range(1, m_FootstepSounds.Length)]);
                break;
            default:
                break;
        }
        
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                            Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        CmdPlaySound(3);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                    (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
		if(playerStats.stamina > 1)
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
		else
			speed = m_WalkSpeed;
			
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }

    private void RotateView()
    {
        m_MouseLook.LookRotation (transform, m_Camera.transform);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
		if(body!= null && body.tag != "Enemy")
		{
	        //dont move the rigidbody if the character is on top of it
	        if (m_CollisionFlags == CollisionFlags.Below)
	        {
	            return;
	        }

	        if (body == null || body.isKinematic)
	        {
	            return;
	        }
	        body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
		}
    }
}

