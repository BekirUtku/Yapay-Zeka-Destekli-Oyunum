using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Her sahne yuklendiginde uygun arayuzu kurar; sahnelere elle obje eklemek gerekmez.
/// Oyuncu olan sahne: HUD + duraklatma menusu + oyun sonu ekrani.
/// MenuScript olan sahne: ana menu.
/// </summary>
public static class GameUIBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // Domain reload kapaliysa abonelik ikiye katlanmasin.
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
        {
            return;
        }

        EnsureEventSystem();

        if (Object.FindFirstObjectByType<PlayerController>() != null)
        {
            GameSession.TotalEnemies = Object.FindObjectsByType<EnemyHealthBase>(FindObjectsSortMode.None).Length;

            GameObject root = new GameObject("[GameUI]");
            root.AddComponent<GameHUD>();
            root.AddComponent<PauseMenu>();
            root.AddComponent<GameEndScreen>();
        }
        else if (Object.FindFirstObjectByType<MenuScript>() != null)
        {
            new GameObject("[MainMenuUI]").AddComponent<MainMenuUI>();
        }
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        // Proje eski Input Manager'i kullaniyor (Both modu), standart modul yeterli.
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
