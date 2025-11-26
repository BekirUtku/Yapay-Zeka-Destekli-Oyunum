using UnityEngine;
using System.Collections;

public class PlayerInvisibility : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;
    private int originalLayer;

    private int invisibilityUses = 0; // Kullaným sayacý
    public int maxUses = 2; // En fazla kaç kere kullanýlabilir

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        // H tuþuna basýldýðýnda ve zaten görünmez deðilken ve hak kaldýysa
        if (Input.GetKeyDown(KeyCode.H) && !isInvisible && invisibilityUses < maxUses)
        {
            StartCoroutine(BecomeInvisible());
        }
    }

    IEnumerator BecomeInvisible()
    {
        isInvisible = true;
        invisibilityUses++; // Her kullanýmda sayaç artýrýlýr

        // Görünmez yap
        spriteRenderer.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Invisible");

        yield return new WaitForSeconds(1f);

        // Görünür yap
        spriteRenderer.enabled = true;
        gameObject.layer = originalLayer;

        isInvisible = false;
    }
}
