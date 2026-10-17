using System.Collections;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    //Can
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    //Düşma Hareketi
    // Dusman vuruslari arasindaki bekleme. Yalnizca TakeEnemyHit'e uygulanir;
    // ates ve zehir gibi kaynaklar kendi zamanlamasini kullanir.
    public bool enemyattack;
    public float enemytimer;

    [Tooltip("Iki dusman vurusu arasinda gecmesi gereken en az sure (saniye).")]
    public float enemyHitCooldown = 1.5f;

    public Animator anim;

    [Header("Vurus hissi")]
    [Tooltip("Oyuncu hasar aldiginda uygulanacak kamera sarsintisi siddeti.")]
    public float hitShakeForce = 0.5f;

    // Ölüm bir kez işlensin; birden fazla DelayedGameOver coroutine'i başlamasın.
    private bool isDead = false;

    private PlayerController playerController;

    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;

        // Oyun basinda ilk dusman vurusu hemen islenebilsin.
        enemyattack = true;
        enemytimer = enemyHitCooldown;

        healthBar.SetMaxHealth(maxHealth);
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void EnemyAttackSpacing()
    {
        if (enemyattack)
        {
            return;
        }

        enemytimer -= Time.deltaTime;
        if (enemytimer <= 0f)
        {
            enemyattack = true;
            enemytimer = enemyHitCooldown;
        }
    }

    /// <summary>
    /// Dusman yakin dovus vurusu. Bekleme suresi dolmadiysa yok sayilir;
    /// dusmanlar menzildeyken her karede cagirdigi icin bu gerekli.
    /// </summary>
    public void TakeEnemyHit(int damage)
    {
        if (isDead || !enemyattack) return;

        enemyattack = false;
        TakeDamage(damage);
    }

    /// <summary>Bekleme suresine bakmadan hasar verir (ates, zehir).</summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger("isHurt");

        HitFlash.Play(gameObject);
        ScreenShake.Shake(hitShakeForce);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;

            // Olum animasyonu oynarken oyuncu yurumesin/saldirmasin.
            if (playerController != null)
            {
                playerController.LockControls();
            }

            // Önce animasyonu başlat
            anim.SetBool("isDead", true);

            // GameOver panelini gecikmeli aç
            StartCoroutine(DelayedGameOver());
        }

        healthBar.SetHealth(currentHealth);
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
        // Olceklenmis zaman: bilgi paneli acikken sayac durur. Gercek zaman
        // kullanilsaydi Game Over panel acikken gelir, panel kapaninca
        // timeScale 1'e donup oyun Game Over ekraninin arkasinda devam ederdi.
        yield return new WaitForSeconds(1.5f);

        // Player scriptindeki Die() fonksiyonunu çağır
        if (playerController != null)
        {
            playerController.Die();
        }
    }
}
