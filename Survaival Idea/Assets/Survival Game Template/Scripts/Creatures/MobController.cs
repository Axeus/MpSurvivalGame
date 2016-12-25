using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(BoxCollider))]

public class MobController : NetworkBehaviour
{

    public float speed;
    public float aggroDistance;
    public float groundCheckDistance = 0.4f;

    [HideInInspector]
    public bool isActive = true;

    [SyncVar]
    protected Vector3 dest;
    [SyncVar]
    protected Vector3 prevDest;
    
    protected Transform thisTransform;

    protected bool isNear;
    protected EnemyHealth enemyHealth;
    

    protected Rigidbody rigid;
    protected Animator animator;
    protected float timer;
    [SyncVar]
    protected bool stop;
    private bool m_PreviouslyGrounded, m_IsGrounded;
    private CapsuleCollider m_Capsule;
    private Vector3 m_GroundContactNormal;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.freezeRotation = true;
        rigid.useGravity = false;

        m_Capsule = GetComponent<CapsuleCollider>();

        animator = GetComponent<Animator>();
        thisTransform = transform;
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            GroundCheck();

            if (enemyHealth.currentHealth > 0)
            {
                if (!stop)
                {
                    dest = Vector3.ProjectOnPlane(dest, m_GroundContactNormal);

                    Vector3 velocity = rigid.velocity;
                    Vector3 velocityChange = (dest - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -10f, 10f);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -10f, 10f);
                    velocityChange.y = 0;

                    rigid.AddForce(velocityChange, ForceMode.VelocityChange);
                }
                else if (m_IsGrounded)
                    rigid.Sleep();               
            }
            else if (m_IsGrounded)
            {
                isActive = false;
                rigid.isKinematic = true;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<BoxCollider>().enabled = false;
            }

            if (!m_IsGrounded)
                rigid.useGravity = true;
            else
                rigid.useGravity = false;

            prevDest = dest;
        }
    }

    private void GroundCheck()
    {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo,
                               ((m_Capsule.height / 2f) - m_Capsule.radius) + groundCheckDistance))
        {
            m_IsGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
    }
}