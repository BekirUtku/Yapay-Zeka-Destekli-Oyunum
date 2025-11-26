using UnityEngine;

public class FıghtAI : MonoBehaviour
{
    public Vector2 pos1;
    public Vector2 pos2;
    public float leftrightspeed;
    private float oldPosition;

    public float distance;

    private Transform target;
    public float followspeed;

    private Animator anim;

    FıghtCombat fıghtcombat;

    public LayerMask playerLayerMask; // Yalnızca bu layer'daki objeleri tarayacak

    void Start()
    {
        Physics2D.queriesStartInColliders = false;
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();

        fıghtcombat = GetComponent<FıghtCombat>();
    }


    void Update()
    {
        FıghtAi();
    }

    void FıghtMove()
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

    void FıghtAi()
    {
        // Sadece belirli layer'ları tarayan raycast
        RaycastHit2D hitEnemy = Physics2D.Raycast(transform.position, -transform.right, distance, playerLayerMask);

        if (hitEnemy.collider != null)
        {
            Debug.DrawLine(transform.position, hitEnemy.point, Color.red);
            anim.SetBool("Attack", true);
            FıghtFollow();

            fıghtcombat.DamagePlayer();
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position - transform.right * distance, Color.green);
            anim.SetBool("Attack", false);
            FıghtMove();

        }

    }

    void FıghtFollow()
    {
        Vector3 targetPosition = new Vector3(target.position.x, gameObject.transform.position.y, target.position.x);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, followspeed * Time.deltaTime);
    }
}
