using System.Collections;
using UnityEngine;

/// <summary>
/// Hasar alan objenin sprite'ini kisa sureligine renklendirir.
/// Sahnede elle eklemeye gerek yok: HitFlash.Play(gameObject) cagrisi
/// bileseni gerekiyorsa kendisi ekler.
/// </summary>
[DisallowMultipleComponent]
public class HitFlash : MonoBehaviour
{
    [Tooltip("Vurus aninda sprite'a uygulanacak renk. Sprite tinti carpma ile calisir, " +
             "bu yuzden yesil ve mavi bilesenleri dusuk tutmak kirmiziyi belirginlestirir.")]
    public Color flashColor = new Color(1f, 0.12f, 0.12f, 1f);

    [Tooltip("Renklendirmenin suresi (saniye, olceklenmemis zaman).")]
    public float flashDuration = 0.13f;

    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private Coroutine routine;

    /// <summary>Hedefte HitFlash yoksa ekler ve calistirir.</summary>
    public static void Play(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        HitFlash flash = target.GetComponent<HitFlash>();
        if (flash == null)
        {
            flash = target.AddComponent<HitFlash>();
        }

        flash.Flash();
    }

    void Awake()
    {
        Cache();
    }

    private void Cache()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }
    }

    public void Flash()
    {
        if (renderers == null)
        {
            Cache();
        }

        if (renderers.Length == 0 || !isActiveAndEnabled)
        {
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
            Restore();
        }

        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = flashColor;
            }
        }

        // Hitstop sirasinda da gorunsun diye olceklenmemis zaman.
        yield return new WaitForSecondsRealtime(flashDuration);

        Restore();
        routine = null;
    }

    private void Restore()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }
    }

    void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
            Restore();
        }
    }
}
