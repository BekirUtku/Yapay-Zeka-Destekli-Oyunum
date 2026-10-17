using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kamera sarsintisi.
///
/// Cinemachine Impulse yerine dogrudan kamera transform'u kaydiriliyor:
/// CinemachineBrain kamerayi LateUpdate icinde yazar ve execution order'i 0'dir,
/// bu script 1000'de oldugu icin ondan SONRA calisir. Brain her karede pozisyonu
/// bastan yazdigi icin ofset birikmez.
///
/// Sahnede obje veya bilesen gerektirmez.
/// </summary>
[DefaultExecutionOrder(1000)]
public class ScreenShake : MonoBehaviour
{
    private static ScreenShake instance;

    [Header("Ayarlar")]
    [Tooltip("Trauma 1 iken uygulanacak en buyuk kayma (dunya birimi).")]
    public float maxOffset = 0.55f;

    [Tooltip("Titresim hizi.")]
    public float frequency = 26f;

    [Tooltip("Trauma'nin saniyede ne kadar sonecegi.")]
    public float decay = 3.2f;

    private Transform cam;
    private float trauma;
    private float seed;
    private bool warned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject host = new GameObject("[ScreenShake]");
        host.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(host);
        instance = host.AddComponent<ScreenShake>();
    }

    void Awake()
    {
        seed = Random.value * 1000f;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sahne degisince eski kamera yok olur.
        cam = null;
        trauma = 0f;
    }

    /// <param name="force">0-1 arasi siddet. Ust uste gelen vuruslarda birikir.</param>
    public static void Shake(float force = 0.35f)
    {
        if (instance != null)
        {
            instance.trauma = Mathf.Clamp01(instance.trauma + force);
        }
    }

    void LateUpdate()
    {
        if (trauma <= 0f)
        {
            return;
        }

        if (cam == null)
        {
            Camera main = Camera.main;
            if (main == null)
            {
                if (!warned)
                {
                    Debug.LogWarning("ScreenShake: MainCamera etiketli kamera bulunamadi, sarsinti devre disi.");
                    warned = true;
                }
                trauma = 0f;
                return;
            }
            cam = main.transform;
        }

        // Karesel sonum, sarsintinin sonunu daha yumusak yapar.
        float amount = trauma * trauma;

        // Hitstop sirasinda da titresin diye olceklenmemis zaman.
        float t = Time.unscaledTime * frequency;
        float offsetX = (Mathf.PerlinNoise(seed, t) - 0.5f) * 2f;
        float offsetY = (Mathf.PerlinNoise(seed + 137f, t) - 0.5f) * 2f;

        // Brain bu karede pozisyonu zaten yazdi; uzerine ekliyoruz, birikme olmaz.
        cam.position += new Vector3(offsetX, offsetY, 0f) * (maxOffset * amount);

        trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);
    }
}
