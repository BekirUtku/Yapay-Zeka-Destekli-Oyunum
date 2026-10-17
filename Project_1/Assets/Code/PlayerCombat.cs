using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCombat : MonoBehaviour
{
    // Eski adlar "atach/attach" yazim hatasi iceriyordu; sahnedeki degerler korunur.
    [FormerlySerializedAs("atachPoint")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    [FormerlySerializedAs("attachRange")]
    public float attackRange = 0.5f;
    [FormerlySerializedAs("attachDamage")]
    public int attackDamage = 40;


    [Header("Vurus hissi")]
    public Color popupColor = new Color(1f, 0.85f, 0.3f);
    public float hitStopDuration = 0.06f;
    public float shakeForce = 0.35f;

    private readonly List<Collider2D> hitBuffer = new List<Collider2D>();
    private ContactFilter2D hitFilter;

    public void DamageEnemy()
    {
        if (attackPoint == null)
        {
            return;
        }

        hitFilter.useLayerMask = true;
        hitFilter.layerMask = enemyLayers;
        hitFilter.useTriggers = Physics2D.queriesHitTriggers;

        Physics2D.OverlapCircle(attackPoint.position, attackRange, hitFilter, hitBuffer);

        bool hitAnything = false;

        foreach (Collider2D enemy in hitBuffer)
        {
            // Enemy, Fight ve Warrior hepsi EnemyHealthBase'ten turer.
            EnemyHealthBase target = enemy.GetComponent<EnemyHealthBase>();
            if (target == null || target.IsDead)
            {
                continue;
            }

            target.TakeDamage(attackDamage);
            hitAnything = true;
            DamagePopup.Spawn(enemy.bounds.center, attackDamage, popupColor);
        }

        // Isabet varsa darbeyi hissettir.
        if (hitAnything)
        {
            HitStop.Play(hitStopDuration);
            ScreenShake.Shake(shakeForce);
        }
    }

    private void OnDrawGizmosSelected()
    {

        if (attackPoint ==null)
        {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
