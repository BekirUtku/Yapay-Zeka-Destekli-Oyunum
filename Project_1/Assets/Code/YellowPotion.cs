using UnityEngine;

public class YellowPotion : MonoBehaviour
{
    public int extraDamage = 20; // Vuruþ gücüne eklenecek miktar

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCombat combat = other.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.attachDamage += extraDamage;
            }

            Destroy(gameObject); // iksir yok olsun
        }
    }
}