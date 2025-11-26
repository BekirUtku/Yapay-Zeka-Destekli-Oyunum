using UnityEngine;

public class FireDamage : MonoBehaviour
{
    public int damageAmount = 10;
    public float damageInterval = 1f;

    private bool playerInFire = false;
    private float damageTimer = 0f;

    void Update()
    {
        if (playerInFire)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    CharacterHealth health = player.GetComponent<CharacterHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(damageAmount);
                        damageTimer = damageInterval;
                    }
                }
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