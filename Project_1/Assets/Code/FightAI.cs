using UnityEngine;

public class FightAI : MonoBehaviour
{
    public Vector2 pos1;
    public Vector2 pos2;
    public float leftrightspeed;
    private float oldPosition;

    public float distance;

    private Transform target;
    public float followspeed;

    private Animator anim;

    FightCombat fightcombat;

    public LayerMask playerLayerMask; // Yalnızca bu layer'daki objeleri tarayacak

    void Start()
    {
        Physics2D.queriesStartInColliders = false;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning($"{name}: Sahnede 'Player' etiketli obje yok, FightAI devre disi birakildi.", this);
            enabled = false;
            return;
        }
        target = playerObj.transform;
        anim = GetComponent<Animator>();

        fightcombat = GetComponent<FightCombat>();
    }


    void Update()
    {
        // Oyun duraklatildiginda (bilgi paneli, Game Over, kazanma ekrani)
        // girdi islenmesin ve fizik sorgulari bosa calismasin.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        FightAi();
    }

    void FightMove()
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

    void FightAi()
    {
        // Sadece belirli layer'ları tarayan raycast
        RaycastHit2D hitEnemy = Physics2D.Raycast(transform.position, -transform.right, distance, playerLayerMask);

        if (hitEnemy.collider != null)
        {
            Debug.DrawLine(transform.position, hitEnemy.point, Color.red);
            anim.SetBool("Attack", true);
            FightFollow();

            fightcombat.DamagePlayer();
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position - transform.right * distance, Color.green);
            anim.SetBool("Attack", false);
            FightMove();

        }

    }

    void FightFollow()
    {
        Vector3 targetPosition = new Vector3(target.position.x, gameObject.transform.position.y, transform.position.z);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, followspeed * Time.deltaTime);
    }
}
