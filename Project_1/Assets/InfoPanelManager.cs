using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Oyun ici bilgi/rehber ekrani. Icerik calisma zamaninda TMP zengin metin olarak
/// uretilir, panel acilirken oyun duraklatilir, fade + scale ile animasyonlanir.
/// Sahnede ek obje gerektirmez; CanvasGroup yoksa otomatik eklenir.
/// </summary>
public class InfoPanelManager : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Acilip kapanacak panel objesi.")]
    public GameObject infoPanel;

    [Tooltip("Bos birakilirsa infoPanel altindaki ilk TextMeshProUGUI kullanilir.")]
    public TextMeshProUGUI infoText;

    [Header("Tuslar")]
    public KeyCode toggleKey = KeyCode.I;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Davranis")]
    [Tooltip("Panel acikken Time.timeScale = 0 yapilir.")]
    public bool pauseWhileOpen = true;

    [Header("Yerlesim")]
    [Tooltip("Metnin panel kenarlarina uzakligi (piksel).")]
    public float padding = 64f;

    [Header("Animasyon")]
    [Range(0f, 1f)] public float fadeDuration = 0.18f;
    [Range(0.5f, 1f)] public float startScale = 0.92f;

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private Coroutine activeAnimation;
    private bool isOpen;
    private float previousTimeScale = 1f;

    // ---- Renk paleti ----
    private const string ColTitle = "#FFC94A";
    private const string ColSection = "#6FC7FF";
    private const string ColKey = "#FFC94A";
    private const string ColBody = "#E6E9EF";
    private const string ColMuted = "#98A2B3";
    private const string ColRed = "#FF6B6B";
    private const string ColGreen = "#7BE38B";
    private const string ColYellow = "#FFD84A";

    void Start()
    {
        if (infoPanel == null)
        {
            Debug.LogWarning($"{name}: InfoPanelManager.infoPanel atanmamis, script devre disi birakildi.", this);
            enabled = false;
            return;
        }

        if (infoText == null)
        {
            // Panel kapali olabilecegi icin includeInactive = true.
            infoText = infoPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        panelRect = infoPanel.GetComponent<RectTransform>();

        canvasGroup = infoPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = infoPanel.AddComponent<CanvasGroup>();
        }

        if (infoText != null)
        {
            ApplyLayout();
            infoText.richText = true;
            infoText.text = BuildContent();
        }
        else
        {
            Debug.LogWarning($"{name}: infoPanel altinda TextMeshProUGUI bulunamadi, metin yazilamadi.", this);
        }

        // Baslangicta kapali.
        isOpen = false;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        infoPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                TryOpen();
            }
        }
        else if (isOpen && Input.GetKeyDown(closeKey))
        {
            Close();
        }
    }

    private void TryOpen()
    {
        // Oyun baska bir sey tarafindan zaten durdurulmussa (Game Over / Kazandiniz)
        // paneli acma; aksi halde kapanista timeScale'i yanlis geri yuklerdik.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        isOpen = true;
        previousTimeScale = Time.timeScale;

        infoPanel.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        if (pauseWhileOpen)
        {
            Time.timeScale = 0f;
        }

        StartAnimation(Animate(0f, 1f, startScale, 1f, false));
    }

    private void Close()
    {
        isOpen = false;
        canvasGroup.blocksRaycasts = false;

        if (pauseWhileOpen)
        {
            Time.timeScale = previousTimeScale;
        }

        StartAnimation(Animate(canvasGroup.alpha, 0f, 1f, startScale, true));
    }

    private void StartAnimation(IEnumerator routine)
    {
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
        }
        activeAnimation = StartCoroutine(routine);
    }

    private IEnumerator Animate(float fromAlpha, float toAlpha, float fromScale, float toScale, bool disableAtEnd)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // Oyun duraklatildigi icin olceklenmemis zaman kullanilmali.
            elapsed += Time.unscaledDeltaTime;
            float k = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
            float eased = 1f - (1f - k) * (1f - k); // ease-out quad

            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);

            if (panelRect != null)
            {
                panelRect.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, eased);
            }

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one * toScale;
        }

        if (disableAtEnd)
        {
            infoPanel.SetActive(false);
        }

        activeAnimation = null;
    }

    /// <summary>
    /// Metin alanini panele oturtur. Sahnedeki RectTransform 200x50 boyutunda,
    /// merkeze sabitlenmis ve negatif margin'lerle disari tasirilmis durumdaydi;
    /// burada panele tam yayilan, esit bosluklu bir alana cevriliyor.
    /// Sahne dosyasi degismez, ayar yalnizca calisma zamaninda uygulanir.
    /// </summary>
    private void ApplyLayout()
    {
        RectTransform tr = infoText.rectTransform;

        tr.localScale = Vector3.one;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.pivot = new Vector2(0.5f, 0.5f);
        tr.offsetMin = new Vector2(padding, padding);    // sol, alt
        tr.offsetMax = new Vector2(-padding, -padding);  // sag, ust

        infoText.margin = Vector4.zero;
        infoText.alignment = TextAlignmentOptions.TopLeft;
    }

    /// <summary>Panelin zengin metin icerigini olusturur.</summary>
    private string BuildContent()
    {
        StringBuilder sb = new StringBuilder(1600);

        // ---- Baslik ----
        sb.AppendLine($"<align=center><size=42><b><color={ColTitle}>OYUN REHBERİ</color></b></size>");
        sb.AppendLine($"<size=18><color={ColMuted}>I veya ESC ile kapat   •   Rehber açıkken oyun duraklar</color></size></align>");
        sb.AppendLine("<size=14> </size>");

        // ---- Kontroller ----
        sb.AppendLine(Section("KONTROLLER"));
        sb.AppendLine(Row("A", "D", "Sola / sağa hareket"));
        sb.AppendLine(Row("SPACE", null, "Zıpla — havadayken bir kez daha zıplayabilirsin"));
        sb.AppendLine(Row("E", null, "Saldır — dururken ve koşarken farklı animasyon"));
        sb.AppendLine(Row("H", null, "Görünmez ol — kısa süreli, sınırlı hakkın var"));
        sb.AppendLine(Row("F", null, "Basılı tut — son NPC'yi kurtar ve oyunu kazan"));
        sb.AppendLine("<size=14> </size>");

        // ---- Iksirler ----
        sb.AppendLine(Section("İKSİRLER"));
        sb.AppendLine(Potion(ColRed, "Kırmızı — Can İksiri", "Canını doldurur."));
        sb.AppendLine(Potion(ColGreen, "Yeşil — Zehir İksiri", "Yanlışlıkla alırsan canını azaltır. Uzak dur."));
        sb.AppendLine(Potion(ColYellow, "Sarı — Güç İksiri", "Saldırı gücünü kalıcı olarak artırır."));
        sb.AppendLine("<size=14> </size>");

        // ---- Tehlikeler ----
        sb.AppendLine(Section("DİKKAT ET"));
        sb.AppendLine(Bullet("Düşmanlar seni görünce takip eder ve yaklaşınca vurur."));
        sb.AppendLine(Bullet("Bazı platformlar üzerine basınca kısa süre sonra düşer."));
        sb.AppendLine(Bullet("Ateş, içinde durduğun sürece düzenli hasar verir."));
        sb.AppendLine(Bullet("Haritadan aşağı düşersen oyun biter."));

        return sb.ToString();
    }

    private string Section(string title)
    {
        return $"<size=26><b><color={ColSection}>|  {title}</color></b></size>";
    }

    private string Row(string key1, string key2, string description)
    {
        string keys = Key(key1);
        if (!string.IsNullOrEmpty(key2))
        {
            keys += $"<color={ColMuted}> / </color>{Key(key2)}";
        }
        return $"<size=20>   {keys}<color={ColMuted}>   —   </color><color={ColBody}>{description}</color></size>";
    }

    private string Key(string k)
    {
        return $"<b><color={ColKey}>[{k}]</color></b>";
    }

    private string Potion(string color, string title, string description)
    {
        return $"<size=20>   <b><color={color}>●  {title}</color></b>\n"
             + $"          <color={ColBody}>{description}</color></size>";
    }

    private string Bullet(string text)
    {
        return $"<size=20>   <color={ColSection}>›</color>   <color={ColBody}>{text}</color></size>";
    }
}
