using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform atachPoint;
    public LayerMask enemyLayers;

    public float attachRange = 0.5f;
    public int attachDamage = 40;


    private readonly List<Collider2D> hitBuffer = new List<Collider2D>();
    private ContactFilter2D hitFilter;

    public void DamageEnemy()
    {
        if (atachPoint == null)
        {
            return;
        }

        hitFilter.useLayerMask = true;
        hitFilter.layerMask = enemyLayers;
        hitFilter.useTriggers = Physics2D.queriesHitTriggers;

        Physics2D.OverlapCircle(atachPoint.position, attachRange, hitFilter, hitBuffer);

        foreach (Collider2D enemy in hitBuffer)
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
