using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fight : MonoBehaviour
{

    public Animator anim;
    public int maxhealth = 100;
    int currenthealth;

    FýghtAI fýghtai;

    void Start()
    {
        currenthealth = maxhealth;
        fýghtai = GetComponent<FýghtAI>();
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
        fýghtai.followspeed = 0;
        Destroy(gameObject, 1.7f);
    }
}
