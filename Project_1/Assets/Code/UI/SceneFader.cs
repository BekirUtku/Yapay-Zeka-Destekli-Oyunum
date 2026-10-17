using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sahneler arasi kararma gecisi. Oyun acilirken kendini olusturur ve sahne
/// degisse de kalir; sahnede obje gerektirmez.
/// Sahne yuklemek icin SceneManager yerine SceneFader.LoadScene kullan.
/// </summary>
public class SceneFader : MonoBehaviour
{
    private const float FadeDuration = 0.35f;

    private static SceneFader instance;

    private CanvasGroup group;
    private bool busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject host = new GameObject("[SceneFader]");
        host.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(host);
        instance = host.AddComponent<SceneFader>();
    }

    void Awake()
    {
        Canvas canvas = UITheme.CreateCanvas(transform, "Fader", 1000);
        group = canvas.gameObject.AddComponent<CanvasGroup>();

        Image black = UITheme.Panel("Black", canvas.transform, new Color(0.02f, 0.03f, 0.05f, 1f), 0);
        UITheme.Stretch(black.rectTransform);
        black.raycastTarget = true;

        // Oyun acilisinda siyahtan ac.
        group.alpha = 1f;
        StartCoroutine(Fade(1f, 0f));
    }

    public static void LoadScene(int buildIndex)
    {
        if (instance == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
            return;
        }
        instance.Begin(() => SceneManager.LoadScene(buildIndex));
    }

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            return;
        }
        instance.Begin(() => SceneManager.LoadScene(sceneName));
    }

    private void Begin(System.Action load)
    {
        // Ayni anda iki gecis baslamasin (butona iki kez tiklama).
        if (busy)
        {
            return;
        }
        StartCoroutine(FadeAndLoad(load));
    }

    private IEnumerator FadeAndLoad(System.Action load)
    {
        busy = true;
        yield return Fade(group.alpha, 1f);

        Time.timeScale = 1f;
        load();

        // Yeni sahnenin ilk karesi cizilsin, sonra ac.
        yield return null;
        yield return Fade(1f, 0f);
        busy = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        group.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / FadeDuration));
            yield return null;
        }

        group.alpha = to;
        group.blocksRaycasts = to > 0f;
    }
}
