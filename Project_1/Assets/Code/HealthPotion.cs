using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int healAmount = 30;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterHealth playerHealth = collision.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                GameHUD.Toast($"+{healAmount} Can", UITheme.Success);
            }

            Destroy(gameObject); 
        }
    }
}