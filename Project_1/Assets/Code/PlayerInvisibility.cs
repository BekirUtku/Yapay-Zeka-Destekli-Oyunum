using UnityEngine;
using System.Collections;

public class PlayerInvisibility : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;
    private int originalLayer;

    private int invisibilityUses = 0; // Kullanım sayacı
    public int maxUses = 2; // En fazla kaç kere kullanılabilir

    /// <summary>Kalan kullanim hakki (HUD gosterir).</summary>
    public int RemainingUses => Mathf.Max(0, maxUses - invisibilityUses);

    public bool IsInvisible => isInvisible;

    private CharacterHealth health;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
        health = GetComponent<CharacterHealth>();
    }

    void Update()
    {
        // Duraklatilmisken (bilgi paneli vb.) veya olurken hak harcanmasin.
        if (Mathf.Approximately(Time.timeScale, 0f) || (health != null && health.IsDead))
        {
            return;
        }

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
