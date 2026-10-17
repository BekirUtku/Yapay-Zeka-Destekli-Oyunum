using UnityEngine;

public class PoisonPotion : MonoBehaviour
{
    public int poisonDamage = 20;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterHealth playerHealth = collision.GetComponent<CharacterHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(poisonDamage);
                GameHUD.Toast($"Zehir  -{poisonDamage}", UITheme.Danger);
            }

            Destroy(gameObject); // Zehir iksiri yok olsun
        }
    }
}
