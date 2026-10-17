using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bir oyun turunun durumu: gecen sure, oldurulen dusman, oyunun bitip
/// bitmedigi ve kayitli en iyi sure. Her sahne yuklenisinde sifirlanir.
/// </summary>
public static class GameSession
{
    private const string BestTimeKey = "BestTime";

    public static int Kills { get; private set; }
    public static int TotalEnemies { get; set; }

    /// <summary>Game Over ya da kazanma ekrani acildiysa true.</summary>
    public static bool Ended { get; private set; }

    private static float endElapsed;

    /// <summary>
    /// Bu turda gecen oyun suresi. Olceklenmis zaman oldugu icin duraklatma ve
    /// bilgi paneli sureye eklenmez.
    /// </summary>
    public static float Elapsed => Ended ? endElapsed : Time.timeSinceLevelLoad;

    public static bool HasBestTime => PlayerPrefs.HasKey(BestTimeKey);
    public static float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        Reset();
        // Domain reload kapaliysa abonelik ikiye katlanmasin.
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            Reset();
        }
    }

    private static void Reset()
    {
        Kills = 0;
        TotalEnemies = 0;
        Ended = false;
        endElapsed = 0f;
    }

    public static void RegisterKill()
    {
        Kills++;
    }

    public static void MarkEnded()
    {
        if (Ended)
        {
            return;
        }
        endElapsed = Time.timeSinceLevelLoad;
        Ended = true;
    }

    /// <summary>Sure en iyi sureden kisaysa kaydeder; yeni rekorsa true doner.</summary>
    public static bool TrySaveBestTime(float seconds)
    {
        if (HasBestTime && seconds >= BestTime)
        {
            return false;
        }

        PlayerPrefs.SetFloat(BestTimeKey, seconds);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>Saniyeyi "dd:ss" bicimine cevirir.</summary>
    public static string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }
}
