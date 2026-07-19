using UnityEngine;

public class FireDamage : MonoBehaviour
{
    public int damageAmount = 10;
    public float damageInterval = 1f;

    private bool playerInFire = false;
    private float damageTimer = 0f;

    // Sahnede 11 FireDamage kopyasi var; her hasar tikinde sahne taramak yerine
    // referansi bir kez Start'ta al.
    private CharacterHealth playerHealth;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<CharacterHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning($"{name}: 'Player' etiketli obje veya CharacterHealth bulunamadi, FireDamage devre disi birakildi.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (playerInFire)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                playerHealth.TakeDamage(damageAmount);
                damageTimer = damageInterval;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInFire = true;
            damageTimer = 0f; // Hemen hasar vermesi için
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInFire = false;
        }
    }
}