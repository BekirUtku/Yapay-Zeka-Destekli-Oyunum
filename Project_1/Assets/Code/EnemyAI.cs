using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public Vector2 pos1;
    public Vector2 pos2;
    public float leftrightspeed;
    private float oldPosition;

    public float distance;

    private Transform target;
    public float followspeed;

    private Animator anim;

    EnemyCombat enemycombat;

    [Tooltip("Raycast yalnizca bu katmanlari gorur. FightAI/WarriorAI ile ayni: Player.")]
    public LayerMask playerLayerMask; // Yalnızca bu layer'daki objeleri tarayacak

    void Start()
    {
        Physics2D.queriesStartInColliders = false;

        // Inspector'da atanmadiysa 'Player' katmanina dus ve uyar.
        if (playerLayerMask == 0)
        {
            playerLayerMask = LayerMask.GetMask("Player");
            Debug.LogWarning($"{name}: EnemyAI.playerLayerMask atanmamis, otomatik olarak 'Player' katmanina ayarlandi.", this);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning($"{name}: Sahnede 'Player' etiketli obje yok, EnemyAI devre disi birakildi.", this);
            enabled = false;
            return;
        }
        target = playerObj.transform;
        anim = GetComponent<Animator>();

        enemycombat = GetComponent<EnemyCombat>();
    }

    void Update()
    {
        EnemyAi();
    }
    void EnemyMove()
    {
        transform.position = Vector3.Lerp(pos1, pos2, Mathf.PingPong(Time.time * leftrightspeed, 1.0f));

        if (transform.position.x > oldPosition)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

        if (transform.position.x < oldPosition)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        oldPosition = transform.position.x;

    }
    void EnemyAi()
    {
        RaycastHit2D hitEnemy = Physics2D.Raycast(transform.position, -transform.right, distance, playerLayerMask);

        if (hitEnemy.collider != null)
        {
            Debug.DrawLine(transform.position, hitEnemy.point, Color.red);
            anim.SetBool("Attack", true);
            EnemyFollow();

            enemycombat.DamagePlayer();
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position - transform.right * distance, Color.green);
            anim.SetBool("Attack", false);
            EnemyMove();
        }
    }

    void EnemyFollow()
    {
        Vector3 targetPosition = new Vector3(target.position.x, gameObject.transform.position.y, transform.position.z);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, followspeed * Time.deltaTime);
    }
}
