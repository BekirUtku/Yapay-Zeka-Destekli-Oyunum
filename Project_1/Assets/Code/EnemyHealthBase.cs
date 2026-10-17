using UnityEngine;

/// <summary>
/// Dusmanlarin ortak can/olum mantigi. Enemy, Fight ve Warrior bundan turer;
/// sahnedeki betik baglantilari bozulmasin diye o siniflar ayri dosyalarda kalir.
/// </summary>
public abstract class EnemyHealthBase : MonoBehaviour
{
    public Animator anim;
    public int maxhealth = 100;
    int currenthealth;
    bool isDead;

    EnemyAIBase ai;

    /// <summary>Olum animasyonundan sonra objenin yok edilecegi sure.</summary>
    protected virtual float DestroyDelay => 1.7f;

    public bool IsDead => isDead;

    protected virtual void Start()
    {
        currenthealth = maxhealth;
        ai = GetComponent<EnemyAIBase>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currenthealth -= damage;
        anim.SetTrigger("hurt");
        HitFlash.Play(gameObject);

        if (currenthealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        GameSession.RegisterKill();
        anim.SetBool("isDead", true);

        // Olurken hareketi durdur.
        if (ai != null)
        {
            ai.followspeed = 0;
            ai.enabled = false;
        }

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, DestroyDelay);
    }
}
