using UnityEngine;

public class FallingTile2 : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool triggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!triggered && collision.collider.CompareTag("Player"))
        {
            triggered = true;
            Invoke("DropTile", 0.5f);
        }
    }

    void DropTile()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;

        Destroy(gameObject, 1f);
    }
}