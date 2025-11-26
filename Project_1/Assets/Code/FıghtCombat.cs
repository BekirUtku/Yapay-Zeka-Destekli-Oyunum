using UnityEngine;

public class FıghtCombat : MonoBehaviour
{

    public Transform enemyAttackPoint;
    public LayerMask playerLayers;

    public float enemyattachRange = 0.5f;
    public int enemyattachDamage = 40;


    public void DamagePlayer()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(enemyAttackPoint.position, enemyattachRange, playerLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<CharacterHealth>().TakeDamage(enemyattachDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {

        if (enemyAttackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(enemyAttackPoint.position, enemyattachRange);
    }

}
