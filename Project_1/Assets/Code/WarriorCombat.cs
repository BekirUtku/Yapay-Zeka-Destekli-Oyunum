using System.Collections.Generic;
using UnityEngine;

public class WarriorCombat : MonoBehaviour
{

    public Transform enemyAttackPoint;
    public LayerMask playerLayers;

    public float enemyattachRange = 0.5f;
    public int enemyattachDamage = 40;


    // Yeniden kullanilan tampon - DamagePlayer her karede cagrilabildigi icin
    // OverlapCircleAll'in her seferinde dizi ayirmasini onler.
    private readonly List<Collider2D> hitBuffer = new List<Collider2D>();
    private ContactFilter2D hitFilter;

    public void DamagePlayer()
    {
        if (enemyAttackPoint == null)
        {
            return;
        }

        hitFilter.useLayerMask = true;
        hitFilter.layerMask = playerLayers;
        hitFilter.useTriggers = Physics2D.queriesHitTriggers;

        Physics2D.OverlapCircle(enemyAttackPoint.position, enemyattachRange, hitFilter, hitBuffer);

        for (int i = 0; i < hitBuffer.Count; i++)
        {
            CharacterHealth health = hitBuffer[i].GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.TakeDamage(enemyattachDamage);
            }
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
