using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fight : MonoBehaviour
{

    public Animator anim;
    public int maxhealth = 100;
    int currenthealth;

    FightAI fightai;

    void Start()
    {
        currenthealth = maxhealth;
        fightai = GetComponent<FightAI>();
    }

    public void TakeDamage(int damage)
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

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        // Olurken hareketi durdur (Enemy ile ayni davranis).
        if (fightai != null)
        {
            fightai.followspeed = 0;
            fightai.enabled = false;
        }
        Destroy(gameObject, 1.7f);
    }
}
