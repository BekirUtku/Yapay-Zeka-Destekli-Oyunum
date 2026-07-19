using UnityEngine;
using System.Collections;

public class PlayerInvisibility : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;
    private int originalLayer;

    private int invisibilityUses = 0; // Kullanım sayacı
    public int maxUses = 2; // En fazla kaç kere kullanılabilir

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        // H tuşuna basıldığında ve zaten görünmez değilken ve hak kaldıysa
        if (Input.GetKeyDown(KeyCode.H) && !isInvisible && invisibilityUses < maxUses)
        {
            StartCoroutine(BecomeInvisible());
        }
    }

    IEnumerator BecomeInvisible()
    {
        isInvisible = true;
        invisibilityUses++; // Her kullanımda sayaç artırılır

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
