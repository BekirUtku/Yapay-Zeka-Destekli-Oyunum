using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public Animator anim;
    public int maxhealth = 100;
    int currenthealth;

    EnemyAI enemyai;

    void Start()
    {
        currenthealth = maxhealth;
        enemyai = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int damage )
    {
        currenthealth -= damage;
        anim.SetTrigger("hurt");

        if (currenthealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        anim.SetBool("isDead", true);

        // Olurken hareketi durdur (Fight/Warrior ile ayni davranis).
        if (enemyai != null)
        {
            enemyai.followspeed = 0;
            enemyai.enabled = false;
        }

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;      
        Destroy(gameObject, 2f);
    }
}
