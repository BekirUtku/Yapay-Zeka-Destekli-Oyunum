using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC ile acilan duraklatma menusu: Devam et, Yeniden basla, Rehber, Ana menu.
/// Fare ve klavye (ok tuslari + Enter) ile kullanilabilir.
/// GameUIBootstrap tarafindan oyun sahnesinde otomatik olusturulur.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public KeyCode pauseKey = KeyCode.Escape;

    public static bool IsOpen { get; private set; }

    private CharacterHealth health;
    private InfoPanelManager info;

    private CanvasGroup group;
    private RectTransform card;
    private TextMeshProUGUI timeText;
    private Button firstButton;
    private Coroutine fadeRoutine;

    private bool open;
    private bool showingGuide;

    void Start()
    {
        health = FindFirstObjectByType<CharacterHealth>();
        info = FindFirstObjectByType<InfoPanelManager>();
        Build();
    }

    void OnDestroy()
    {
        IsOpen = false;
    }

    private void Build()
    {
        Canvas canvas = UITheme.CreateCanvas(transform, "PauseMenu", 50);

        Image backdrop = UITheme.Panel("Backdrop", canvas.transform, UITheme.Backdrop, 0);
        UITheme.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true; // Arkadaki oyuna tiklanmasin.
        group = backdrop.gameObject.AddComponent<CanvasGroup>();

        Image cardImage = UITheme.Panel("Card", backdrop.transform, UITheme.Card, UITheme.RadiusLarge);
        card = cardImage.rectTransform;
        UITheme.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 0f));
        UITheme.VStack(cardImage.gameObject, 36, 12f, TextAnchor.UpperCenter);
        UITheme.FitHeight(cardImage.gameObject);

        UITheme.Text("Title", card, "Duraklatıldı", 44f, UITheme.TextPrimary, true, TextAlignmentOptions.Center);
        timeText = UITheme.Text("Time", card, "", 22f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);
        UITheme.Spacer(card, 12f);

        firstButton = UITheme.Button(card, "Devam et", true, Resume);
        UITheme.Button(card, "Yeniden başla", false, Restart);
        if (info != null)
        {
            UITheme.Button(card, "Rehber", false, OpenGuide);
        }
        UITheme.Button(card, "Ana menü", false, GoToMainMenu);

        UITheme.Spacer(card, 4f);
        UITheme.Text("Hint", card, "ESC ile oyuna dön", 18f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);

        group.alpha = 0f;
        backdrop.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(pauseKey) || showingGuide)
        {
            return;
        }

        // Bilgi paneli ayni tusla kapaniyor; ayni karede menu de acilmasin.
        if (info != null && (info.IsOpen || info.LastClosedFrame == Time.frameCount))
        {
            return;
        }

        if (open)
        {
            Resume();
        }
        else
        {
            TryPause();
        }
    }

#if !UNITY_EDITOR
    // Oyun penceresi odagi kaybedince (alt+tab) otomatik duraklat.
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !open && !showingGuide)
        {
            TryPause();
        }
    }
#endif

    private void TryPause()
    {
        if (GameSession.Ended || (health != null && health.IsDead))
        {
            return;
        }

        // Baska bir sey (bilgi paneli vb.) oyunu zaten durdurmus.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        open = true;
        IsOpen = true;
        Time.timeScale = 0f;
        timeText.text = $"Süre  {GameSession.FormatTime(GameSession.Elapsed)}";
        SetVisible(true);
    }

    public void Resume()
    {
        if (!open)
        {
            return;
        }

        open = false;
        IsOpen = false;
        Time.timeScale = 1f;
        SetVisible(false);
    }

    private void OpenGuide()
    {
        showingGuide = true;
        SetVisible(false);

        // Oyun duraklatilmis kalir; rehber kapaninca menuye geri donulur.
        info.OpenFromPause(() =>
        {
            showingGuide = false;
            SetVisible(true);
        });
    }

    private void Restart()
    {
        SceneFader.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMainMenu()
    {
        SceneFader.LoadScene("Menu");
    }

    private void SetVisible(bool visible)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(UIAnim.Show(group, card, visible));

        if (EventSystem.current != null)
        {
            // Gizli bir buton secili kalirsa Space/Enter onu tetikleyebilir.
            EventSystem.current.SetSelectedGameObject(visible ? firstButton.gameObject : null);
        }
    }
}
