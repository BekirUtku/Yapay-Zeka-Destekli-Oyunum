using System.Collections;
using UnityEngine;

/// <summary>
/// Vurus aninda oyunu cok kisa sureligine yavaslatir; darbenin agirlik
/// hissini artirir. Sahnede obje gerektirmez, ilk sahne yuklendiginde
/// kendini olusturur.
/// </summary>
public class HitStop : MonoBehaviour
{
    private static HitStop instance;

    private Coroutine routine;
    private float appliedScale = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject host = new GameObject("[HitStop]");
        host.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(host);
        instance = host.AddComponent<HitStop>();
    }

    /// <param name="duration">Gercek zamanda ne kadar surecegi.</param>
    /// <param name="timeScale">Bu sure boyunca uygulanacak zaman olcegi.</param>
    public static void Play(float duration = 0.06f, float timeScale = 0.05f)
    {
        if (instance != null)
        {
            instance.Run(duration, timeScale);
        }
    }

    private void Run(float duration, float scale)
    {
        // Oyun zaten duraklatilmissa (bilgi paneli, Game Over) karisma.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(Routine(duration, scale));
    }

    private IEnumerator Routine(float duration, float scale)
    {
        appliedScale = scale;
        Time.timeScale = scale;

        yield return new WaitForSecondsRealtime(duration);

        // Bu arada baska bir sistem timeScale'i degistirdiyse (panel acildi,
        // oyun bitti) onun degerini ezme.
        if (Mathf.Approximately(Time.timeScale, appliedScale))
        {
            Time.timeScale = 1f;
        }

        appliedScale = -1f;
        routine = null;
    }
}
