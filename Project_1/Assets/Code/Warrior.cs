using UnityEngine;

public class Warrior : MonoBehaviour
{

    public Animator anim;
    public int maxhealth = 100;
    int currenthealth;

    WarriorAI warriorai;

    void Start()
    {
        currenthealth = maxhealth;
        warriorai = GetComponent<WarriorAI>();
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
        if (warriorai != null)
        {
            warriorai.followspeed = 0;
            warriorai.enabled = false;
        }
        Destroy(gameObject, 1.7f);
    }

}
