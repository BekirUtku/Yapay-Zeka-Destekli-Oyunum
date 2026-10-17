using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Ana menu: hareketli arka plan, oyun basligi, Oyna / Nasil oynanir / Cik
/// butonlari, en iyi sure ve kontrol listesi penceresi. Sahnedeki eski menu
/// canvas'ini gizleyip yerine gecer; butonlar yine MenuScript'i cagirir.
/// GameUIBootstrap tarafindan menu sahnesinde otomatik olusturulur.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    /// <summary>Ana menude gorunen oyun adi.</summary>
    public const string GameTitle = "Kayıp Şövalye";
    public const string Tagline = "2D PLATFORM MACERASI";

    private const int ParticleCount = 46;

    private MenuScript menu;

    private RectTransform particleRoot;
    private RectTransform[] particles;
    private Image[] particleImages;
    private Vector4[] particleData; // x: hiz, y: salinim fazi, z: boyut, w: parlaklik fazi
    private RectTransform glowA;
    private RectTransform glowB;

    private Button playButton;
    private CanvasGroup helpGroup;
    private RectTransform helpCard;
    private Button helpCloseButton;
    private Coroutine helpRoutine;
    private bool helpOpen;

    void Start()
    {
        menu = FindFirstObjectByType<MenuScript>();
        HideLegacyCanvases();
        Build();
        Select(playButton);
    }

    /// <summary>Bu sahnedeki eski menu canvas'larini kapatir (kalici fader'a dokunmaz).</summary>
    private void HideLegacyCanvases()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (c.isRootCanvas && c.gameObject.scene == gameObject.scene && !c.transform.IsChildOf(transform))
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    // ------------------------------------------------------------------ Kurulum

    private void Build()
    {
        Canvas canvas = UITheme.CreateCanvas(transform, "MainMenu", 5);
        BuildBackground(canvas.transform);
        BuildContent(canvas.transform);
        BuildFooter(canvas.transform);
        BuildHelp(canvas.transform);
    }

    private void BuildBackground(Transform parent)
    {
        Image bg = UITheme.Panel("Background", parent, new Color(0.043f, 0.067f, 0.09f, 1f), 0);
        UITheme.Stretch(bg.rectTransform);

        glowA = Glow(parent, "GlowA", UITheme.WithAlpha(UITheme.Accent, 0.16f), new Vector2(0.78f, 0.62f), 1500f);
        glowB = Glow(parent, "GlowB", new Color(0.23f, 0.43f, 0.65f, 0.14f), new Vector2(0.12f, 0.1f), 1300f);

        particleRoot = UITheme.Rect("Particles", parent);
        UITheme.Stretch(particleRoot);

        particles = new RectTransform[ParticleCount];
        particleImages = new Image[ParticleCount];
        particleData = new Vector4[ParticleCount];

        Sprite dot = UITheme.RoundedSprite(4);
        for (int i = 0; i < ParticleCount; i++)
        {
            Image img = UITheme.Panel("P", particleRoot, UITheme.Accent, 0);
            img.sprite = dot;

            float size = Random.Range(3f, 8f);
            RectTransform rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(Random.Range(0f, 1920f), Random.Range(0f, 1080f));

            particles[i] = rt;
            particleImages[i] = img;
            particleData[i] = new Vector4(Random.Range(12f, 40f), Random.Range(0f, 6.28f), size, Random.Range(0f, 6.28f));
        }

        // Alt kenarda yumusak koyulasma; metin okunurlugu icin.
        Image shade = UITheme.Panel("Shade", parent, new Color(0f, 0f, 0f, 0.55f), 0);
        shade.sprite = UITheme.VignetteSprite();
        UITheme.Stretch(shade.rectTransform);
    }

    private RectTransform Glow(Transform parent, string name, Color color, Vector2 anchor, float size)
    {
        Image glow = UITheme.Panel(name, parent, color, 0);
        glow.sprite = UITheme.GlowSprite();
        UITheme.Place(glow.rectTransform, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
        return glow.rectTransform;
    }

    private void BuildContent(Transform parent)
    {
        RectTransform column = UITheme.Rect("Content", parent);
        UITheme.Place(column, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(170f, 10f), new Vector2(960f, 0f));
        UITheme.VStack(column.gameObject, 0, 14f, TextAnchor.MiddleLeft, false);
        UITheme.FitHeight(column.gameObject);

        TextMeshProUGUI tagline = UITheme.Overline("Tagline", column, Tagline, UITheme.Accent);
        tagline.fontSize = 22f;
        UITheme.Layout(tagline.gameObject, 960f);

        TextMeshProUGUI title = UITheme.Heading("Title", column, GameTitle, 120f, UITheme.TextPrimary);
        title.characterSpacing = -2f;
        UITheme.Layout(title.gameObject, 960f);

        TextMeshProUGUI subtitle = UITheme.Text("Subtitle", column,
            "Tuzaklarla dolu ovayı geç, düşmanları alt et ve kayıp şövalyeyi kurtar.",
            26f, UITheme.TextSecondary);
        UITheme.Layout(subtitle.gameObject, 560f);

        UITheme.Spacer(column, 26f);

        RectTransform buttons = UITheme.Rect("Buttons", column);
        UITheme.VStack(buttons.gameObject, 0, 12f, TextAnchor.UpperLeft);
        UITheme.Layout(buttons.gameObject, 380f);

        playButton = UITheme.Button(buttons, "Oyna", true, Play);
        UITheme.Button(buttons, "Nasıl oynanır", false, OpenHelp);
        UITheme.Button(buttons, "Çık", false, Quit);

        if (GameSession.HasBestTime)
        {
            UITheme.Spacer(column, 10f);
            Image chip = UITheme.Chip(column, "BestTime", out TextMeshProUGUI chipText);
            chipText.text = $"<color={UITheme.SecondaryHex}>En iyi süre</color>   <b>{GameSession.FormatTime(GameSession.BestTime)}</b>";
            chip.color = UITheme.HudPanel;
        }
    }

    private void BuildFooter(Transform parent)
    {
        TextMeshProUGUI hint = UITheme.Text("Hint", parent,
            $"Oyun içinde <color={UITheme.AccentHex}>I</color> ile rehberi, <color={UITheme.AccentHex}>ESC</color> ile duraklatma menüsünü açabilirsin.",
            20f, UITheme.TextSecondary);
        UITheme.Place(hint.rectTransform, Vector2.zero, Vector2.zero, new Vector2(170f, 56f), new Vector2(1100f, 30f));

        TextMeshProUGUI version = UITheme.Text("Version", parent, "v" + Application.version, 20f, UITheme.TextSecondary,
            false, TextAlignmentOptions.Right);
        UITheme.Place(version.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-56f, 56f), new Vector2(200f, 30f));
    }

    private void BuildHelp(Transform parent)
    {
        Image backdrop = UITheme.Panel("HelpBackdrop", parent, UITheme.Backdrop, 0);
        UITheme.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;
        helpGroup = backdrop.gameObject.AddComponent<CanvasGroup>();

        Image cardImage = UITheme.Panel("HelpCard", backdrop.transform, UITheme.Card, UITheme.RadiusLarge);
        helpCard = cardImage.rectTransform;
        UITheme.Place(helpCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 0f));
        UITheme.VStack(cardImage.gameObject, 40, 14f, TextAnchor.UpperLeft);
        UITheme.FitHeight(cardImage.gameObject);

        UITheme.Heading("Title", helpCard, "Nasıl oynanır", 48f, UITheme.TextPrimary);
        UITheme.Text("Subtitle", helpCard, "Şövalyeye ulaşıp F'yi basılı tutarak onu kurtar.", 22f, UITheme.TextSecondary);
        UITheme.Spacer(helpCard, 6f);

        ControlRow(helpCard, new[] { "A", "D" }, "Sola / sağa hareket");
        ControlRow(helpCard, new[] { "Space" }, "Zıpla — havadayken bir kez daha");
        ControlRow(helpCard, new[] { "E" }, "Saldır");
        ControlRow(helpCard, new[] { "H" }, "Kısa süre görünmez ol — sınırlı hak");
        ControlRow(helpCard, new[] { "F" }, "Basılı tut — şövalyeyi kurtar");
        ControlRow(helpCard, new[] { "I" }, "Oyun rehberi");
        ControlRow(helpCard, new[] { "ESC" }, "Duraklat");

        UITheme.Spacer(helpCard, 10f);
        helpCloseButton = UITheme.Button(helpCard, "Kapat", true, CloseHelp);
        // Pencere acikken ok tuslari arkadaki menu butonlarina gecmesin.
        helpCloseButton.navigation = new Navigation { mode = Navigation.Mode.None };

        helpGroup.alpha = 0f;
        backdrop.gameObject.SetActive(false);
    }

    private void ControlRow(Transform parent, string[] keys, string description)
    {
        RectTransform row = UITheme.Rect("Row", parent);
        UITheme.HStack(row.gameObject, 16f, TextAnchor.MiddleLeft, false);
        UITheme.Layout(row.gameObject, -1f, 40f);

        RectTransform keyArea = UITheme.Rect("Keys", row);
        UITheme.HStack(keyArea.gameObject, 6f, TextAnchor.MiddleLeft, false);
        UITheme.Layout(keyArea.gameObject, 160f, 40f);
        foreach (string key in keys)
        {
            UITheme.KeyBadge(keyArea, key);
        }

        UITheme.Flex(UITheme.Text("Text", row, description, 24f, UITheme.TextPrimary).gameObject);
    }

    // ---------------------------------------------------------------- Davranis

    void Update()
    {
        AnimateBackground();

        if (helpOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseHelp();
        }
    }

    private void AnimateBackground()
    {
        float t = Time.unscaledTime;
        float dt = Time.unscaledDeltaTime;

        glowA.anchoredPosition = new Vector2(Mathf.Sin(t * 0.13f) * 90f, Mathf.Cos(t * 0.11f) * 60f);
        glowB.anchoredPosition = new Vector2(Mathf.Cos(t * 0.09f) * 80f, Mathf.Sin(t * 0.12f) * 50f);

        Rect area = particleRoot.rect;
        for (int i = 0; i < ParticleCount; i++)
        {
            Vector4 d = particleData[i];
            Vector2 p = particles[i].anchoredPosition;

            // Yavasca yukari suzulur, hafifce saga sola salinir, ustten cikinca alttan doner.
            p.y += d.x * dt;
            p.x += Mathf.Sin(t * 0.6f + d.y) * 8f * dt;
            if (p.y > area.height + 10f)
            {
                p.y = -10f;
                p.x = Random.Range(0f, area.width);
            }
            particles[i].anchoredPosition = p;

            float twinkle = 0.5f + 0.5f * Mathf.Sin(t * 1.4f + d.w);
            particleImages[i].color = UITheme.WithAlpha(UITheme.Accent, Mathf.Lerp(0.08f, 0.45f, twinkle) * (d.z / 8f));
        }
    }

    private void Play()
    {
        if (menu != null)
        {
            menu.PlayButton();
        }
        else
        {
            SceneFader.LoadScene(1);
        }
    }

    private void Quit()
    {
        if (menu != null)
        {
            menu.QuitButton();
        }
        else
        {
            Application.Quit();
        }
    }

    private void OpenHelp()
    {
        helpOpen = true;
        SetHelpVisible(true);
        Select(helpCloseButton);
    }

    private void CloseHelp()
    {
        helpOpen = false;
        SetHelpVisible(false);
        Select(playButton);
    }

    private void SetHelpVisible(bool visible)
    {
        if (helpRoutine != null)
        {
            StopCoroutine(helpRoutine);
        }
        helpRoutine = StartCoroutine(UIAnim.Show(helpGroup, helpCard, visible));
    }

    private static void Select(Button button)
    {
        if (EventSystem.current != null && button != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
