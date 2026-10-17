using UnityEngine;

/// <summary>
/// Dusmanlarin ortak devriye/takip mantigi. EnemyAI, FightAI ve WarriorAI
/// bundan turer.
///
/// Devriye, pos1 ile pos2 arasinda bulundugu yerden yururek ilerler. Eskiden
/// konum Time.time ile hesaplandigi icin takipten donen dusman devriye
/// cizgisine isinlaniyordu.
/// </summary>
public abstract class EnemyAIBase : MonoBehaviour
{
    public Vector2 pos1;
    public Vector2 pos2;

    [Tooltip("Devriye hizi: saniyede pos1-pos2 mesafesinin kati kadar yol alinir " +
             "(eski PingPong davranisiyla ayni hiz).")]
    public float leftrightspeed;

    [Tooltip("Oyuncuyu gorebilecegi en uzak mesafe.")]
    public float distance;

    public float followspeed;

    [Tooltip("Oyuncu gorus isininin disina ciktiktan sonra kac saniye daha takip edilecegi. " +
             "Oyuncu dusmanin ustunden atlayip arkasina gecince hemen kaybedilmesin diye.")]
    public float loseSightDelay = 1f;

    [Tooltip("Raycast yalnizca bu katmanlari gorur: Player.")]
    public LayerMask playerLayerMask; // Yalnızca bu layer'daki objeleri tarayacak

    private Transform target;
    private Animator anim;
    private EnemyCombatBase combat;

    private Vector2 patrolTarget;
    private float lastSeenTime = float.NegativeInfinity;

    protected virtual void Start()
    {
        // Inspector'da atanmadiysa 'Player' katmanina dus ve uyar.
        if (playerLayerMask == 0)
        {
            playerLayerMask = LayerMask.GetMask("Player");
            Debug.LogWarning($"{name}: {GetType().Name}.playerLayerMask atanmamis, otomatik olarak 'Player' katmanina ayarlandi.", this);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning($"{name}: Sahnede 'Player' etiketli obje yok, {GetType().Name} devre disi birakildi.", this);
            enabled = false;
            return;
        }
        target = playerObj.transform;
        anim = GetComponent<Animator>();
        combat = GetComponent<EnemyCombatBase>();

        // Eski davranis da pos2'ye dogru yurumekle basliyordu.
        patrolTarget = pos2;
    }

    void Update()
    {
        // Oyun duraklatildiginda (bilgi paneli, Game Over, kazanma ekrani)
        // girdi islenmesin ve fizik sorgulari bosa calismasin.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        Think();
    }

    void Think()
    {
        // Gorus isini dusmanin baktigi yone gider (sprite sola bakarken -right).
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.right, distance, playerLayerMask);

        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            lastSeenTime = Time.time;
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position - transform.right * distance, Color.green);
        }

        bool chasing = IsTargetDetectable() && Time.time - lastSeenTime <= loseSightDelay;

        anim.SetBool("Attack", chasing);

        if (chasing)
        {
            Follow();
            if (combat != null)
            {
                combat.DamagePlayer();
            }
        }
        else
        {
            // Gorunmez olan oyuncu hafizadan da silinsin.
            lastSeenTime = float.NegativeInfinity;
            Patrol();
        }
    }

    /// <summary>Oyuncu gorunmezlik gibi bir nedenle katman degistirdiyse takip edilmez.</summary>
    bool IsTargetDetectable()
    {
        return (playerLayerMask.value & (1 << target.gameObject.layer)) != 0;
    }

    void Patrol()
    {
        float patrolSpeed = Vector2.Distance(pos1, pos2) * leftrightspeed;
        Vector2 current = transform.position;
        Vector2 next = Vector2.MoveTowards(current, patrolTarget, patrolSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        Face(patrolTarget.x - current.x);

        if ((next - patrolTarget).sqrMagnitude < 0.0001f)
        {
            patrolTarget = patrolTarget == pos2 ? pos1 : pos2;
        }
    }

    void Follow()
    {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, transform.position.z);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, followspeed * Time.deltaTime);

        // Oyuncu arkaya gecse de ona don; aksi halde gorus isini kaybeder.
        Face(target.position.x - transform.position.x);
    }

    /// <summary>dx &gt; 0 saga, dx &lt; 0 sola bakar; cok kucuk farklarda yon degismez.</summary>
    void Face(float dx)
    {
        if (dx > 0.05f)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else if (dx < -0.05f)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
