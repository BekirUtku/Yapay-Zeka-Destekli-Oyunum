using System.Collections;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    //Can
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    //Düşma Hareketi
    public bool enemyattack;
    public float enemytimer;

    public Animator anim;

    // Ölüm bir kez işlensin; birden fazla DelayedGameOver coroutine'i başlamasın.
    private bool isDead = false;


    void Start()
    {
        currentHealth = maxHealth;
        enemytimer = 1.5f;

        healthBar.SetMaxHealth(maxHealth);
        anim = GetComponent<Animator>();

    }

    void EnemyAttackSpacing()
    {
        if (enemyattack == false)
        {
            enemytimer -= Time.deltaTime;
        }
        if (enemytimer < 0)
        {
            enemytimer = 0f;
        }
        if (enemytimer == 0f)
        {
            enemyattack = true;
            enemytimer = 1.5f;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (enemyattack)
        {
            currentHealth -= damage;
            enemyattack = false;
            anim.SetTrigger("isHurt");
        }

        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;

            // Önce animasyonu başlat
            anim.SetBool("isDead", true);

            // GameOver panelini gecikmeli aç
            StartCoroutine(DelayedGameOver());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // max sağlığı aşmasın
        healthBar.SetHealth(currentHealth); // health bar’ı güncelle
    }

    void Update()
    {
        EnemyAttackSpacing();

#if UNITY_EDITOR
        // Test amaçlı hasar tuşu - yalnızca Editor'de derlenir, build'e girmez.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TakeDamage(20);
        }
#endif
    }
    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        // Player scriptindeki Die() fonksiyonunu çağır
        GetComponent<PlayerController>().Die();
    }
}
