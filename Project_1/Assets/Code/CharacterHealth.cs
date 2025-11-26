using System.Collections;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    //Can
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    //Düþma Hareketi
    public bool enemyattack;
    public float enemytimer;

    public Animator anim;


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

    void CharacterDamage()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            enemyattack = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (enemyattack)
        {
            currentHealth -= 20;
            enemyattack = false;
            anim.SetTrigger("isHurt");
        }

        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            // Önce animasyonu baþlat
            anim.SetBool("isDead", true);

            // GameOver panelini gecikmeli aç
            StartCoroutine(DelayedGameOver());
        }
    }

    void Die()
    {
        anim.SetBool("isDead", true);
        Destroy(gameObject, 2f);
    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // max saðlýðý aþmasýn
        healthBar.SetHealth(currentHealth); // health bar’ý güncelle
    }

    void Update()
    {
        EnemyAttackSpacing();
        CharacterDamage();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            TakeDamage(20);
        }
    }
    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        // Player scriptindeki Die() fonksiyonunu çaðýr
        GetComponent<NewMonoBehaviourScript>().Die();
    }
}
