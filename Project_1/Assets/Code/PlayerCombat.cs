using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform atachPoint;
    public LayerMask enemyLayers;

    public float attachRange = 0.5f;
    public int attachDamage = 40;


    public void DamageEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atachPoint.position, attachRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Fight fightScript = enemy.GetComponent<Fight>();
            if (fightScript != null)
            {
                fightScript.TakeDamage(attachDamage);
            }

            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attachDamage);
            }

            Warrior warriorScript = enemy.GetComponent<Warrior>();
            if (warriorScript != null)
            {
                warriorScript.TakeDamage(attachDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {

        if (atachPoint ==null)
        {
            return;
        }
        Gizmos.DrawWireSphere(atachPoint.position, attachRange);
    }

}
