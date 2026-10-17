using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Dusmanlarin ortak yakin dovus saldirisi. EnemyCombat, FightCombat ve
/// WarriorCombat bundan turer.
/// </summary>
public abstract class EnemyCombatBase : MonoBehaviour
{
    public Transform enemyAttackPoint;
    public LayerMask playerLayers;

    // Eski adlar "attach" yazim hatasi iceriyordu; sahnedeki degerler korunur.
    [FormerlySerializedAs("enemyattachRange")]
    public float attackRange = 0.5f;
    [FormerlySerializedAs("enemyattachDamage")]
    public int attackDamage = 40;

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

        Physics2D.OverlapCircle(enemyAttackPoint.position, attackRange, hitFilter, hitBuffer);

        for (int i = 0; i < hitBuffer.Count; i++)
        {
            CharacterHealth health = hitBuffer[i].GetComponent<CharacterHealth>();
            if (health != null)
            {
                // Dusman vuruslari CharacterHealth'teki bekleme suresine tabidir.
                health.TakeEnemyHit(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyAttackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(enemyAttackPoint.position, attackRange);
    }
}
