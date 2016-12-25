using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AgressiveEnemy : MobController {

    public float attackRate;   
    public int damage;
    public float attackDistance;

    private float attackTimer;

    public static float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    [ClientRpc]
    void RpcAnimSetTrigger(string val)
    {
        animator.SetTrigger(val);
    }

    [Command]
    void CmdAnimSetTrigger(string val)
    {
        RpcAnimSetTrigger(val);
    }

    [ClientRpc]
    void RpcAnimSetFloat(string val, float val2)
    {
        animator.SetFloat(val, val2);
    }

    [Command]
    void CmdAnimSetFloat(string val, float val2)
    {
        RpcAnimSetFloat(val, val2);
    }

    [ClientRpc]
    void RpcAnimSetBool(string val, bool val2)
    {
        animator.SetBool(val, val2);
    }

    [Command]
    void CmdAnimSetBool(string val, bool val2)
    {
        RpcAnimSetBool(val, val2);
    }

    void Start()
    {
        if(isServer)
        {
            InvokeRepeating("Upd", 0.5f, 0.5f);
        }
    }

    bool playerNear;

    void Upd()
    {
        if (isActive && isServer)
        {
            if (enemyHealth.currentHealth > 0)
            {
                if (enemyHealth.playerHealth == null || Distance(enemyHealth.player.position, thisTransform.position) > aggroDistance)
                {
                    playerNear = false;
                    enemyHealth.playerHealth = null;

                    Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aggroDistance);
                    int i = 0;
                    while (i < hitColliders.Length)
                    {
                        if (hitColliders[i].tag == "Player")
                        {
                            enemyHealth.playerHealth = hitColliders[i].GetComponentInParent<PlayerStats>();
                            if (!enemyHealth.playerHealth.isDead)
                            {
                                playerNear = true;
                                enemyHealth.player = hitColliders[i].transform;

                                break;
                            }
                        }
                        i++;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (isActive && isServer)
        {
            if (enemyHealth.currentHealth > 0)
            {
                Vector3 playerPos = thisTransform.position;
                if (playerNear && !enemyHealth.playerHealth.isDead)
                    playerPos = enemyHealth.player.position;
                else
                    if(enemyHealth.hurt)
                    playerPos = enemyHealth.hitPos;

                if ((playerNear && !enemyHealth.playerHealth.isDead) || enemyHealth.hurt)
                {
                    thisTransform.LookAt(new Vector3(playerPos.x, thisTransform.position.y, playerPos.z));

                    float distance = Distance(playerPos, thisTransform.position);

                    if (distance < attackDistance)
                    {
                        if (!stop)
                            CmdAnimSetBool("Walk", false);
                        stop = true;

                        if (attackTimer <= 0)
                        {
                            RaycastHit hit;

                            if (Physics.Raycast(thisTransform.position, thisTransform.forward, out hit, attackDistance))
                            {
                                if (hit.collider.tag == "Player")
                                {
                                    CmdAnimSetTrigger("Attack");
                                    attackTimer = attackRate;
                                    enemyHealth.playerHealth.TakeDamage(damage);
                                }
                            }
                        }
                    }
                    else
                    {
                        CmdAnimSetFloat("Run", 1f);

                        if (stop)
                            CmdAnimSetBool("Walk", true);
                        stop = false;
                        dest = thisTransform.forward * speed;
                    }
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    if (timer <= 0)
                    {
                        timer = Random.Range(0.5f, 1f) * 8f;

                        if (Random.value < 0.5f)
                        {
                            stop = true;
                            CmdAnimSetBool("Walk", false);
                            dest = Vector3.zero;
                        }
                        else
                        {
                            stop = false;
                            dest = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                            thisTransform.rotation = Quaternion.LookRotation(dest);
                            float speedF = Random.Range(0.1f, 1f);
                            dest = thisTransform.forward * speed * speedF;
                            CmdAnimSetFloat("Run", speedF);
                            CmdAnimSetBool("Walk", true);
                        }
                    }

                    timer -= Time.deltaTime;
                }
            }
        }
    }
}
