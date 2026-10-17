using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oyun ici gosterge: can karti (hasar izi ile), gorunmezlik ve guc cipleri,
/// sure, dusuk canda kirmizi kenar, iksir bildirimleri ve kurtarma ilerlemesi.
/// GameUIBootstrap tarafindan oyun sahnesinde otomatik olusturulur.
/// </summary>
public class GameHUD : MonoBehaviour
{
    private static GameHUD instance;

    [Tooltip("Can bu oranin altina dusunce ekran kenarlari kirmizi atar.")]
    public float lowHealthThreshold = 0.3f;

    private CharacterHealth health;
    private PlayerInvisibility invisibility;
    private PlayerCombat combat;
    private PlayerRescue rescue;

    private CanvasGroup hudGroup;
    private Image vignette;

    private TextMeshProUGUI healthValue;
    private RectTransform healthFill;
    private RectTransform healthGhost;
    private Image healthFillImage;
    private RectTransform healthCard;

    private Image invisChip;
    private TextMeshProUGUI invisText;
    private RectTransform powerChip;
    private TextMeshProUGUI powerText;
    private TextMeshProUGUI timerText;

    private CanvasGroup rescueGroup;
    private RectTransform rescueFill;

    private RectTransform toastRoot;

    private CanvasGroup introGroup;
    private RectTransform introCard;
    private float introTime;

    private int lastHealth = int.MinValue;
    private float shownFraction = 1f;
    private float ghostFraction = 1f;
    private float ghostHoldUntil;
    private float hitFlash;
    private float cardShake;

    private int lastUses = -1;
    private bool lastInvisible;
    private int lastPower = int.MinValue;
    private float powerPulse;
    private int lastSecond = -1;

    /// <summary>Sahnede HUD varsa true; yoksa eski sahne UI'si kullanilir.</summary>
    public static bool Exists => instance != null;

    /// <summary>Ekranin ustunde kisa bir bildirim gosterir (iksir alindi vb.).</summary>
    public static void Toast(string text, Color color)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ToastRoutine(text, color));
        }
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Start()
    {
        health = FindFirstObjectByType<CharacterHealth>();
        if (health == null)
        {
            Debug.LogWarning($"{name}: CharacterHealth bulunamadi, GameHUD devre disi birakildi.", this);
            enabled = false;
            return;
        }

        invisibility = health.GetComponent<PlayerInvisibility>();
        combat = health.GetComponent<PlayerCombat>();
        rescue = health.GetComponent<PlayerRescue>();

        // Eski can bari yerini yeni karta birakir. Bilesen calismaya devam eder,
        // yalnizca gorunmez.
        if (health.healthBar != null)
        {
            health.healthBar.gameObject.SetActive(false);
        }

        Build();
    }

    // ------------------------------------------------------------------ Kurulum

    private void Build()
    {
        Canvas canvas = UITheme.CreateCanvas(transform, "HUD", 10);
        hudGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        hudGroup.blocksRaycasts = false;
        hudGroup.interactable = false;

        // En altta: hasar/dusuk can vinyeti.
        vignette = UITheme.Panel("Vignette", canvas.transform, UITheme.WithAlpha(UITheme.Danger, 0f), 0);
        vignette.sprite = UITheme.VignetteSprite();
        UITheme.Stretch(vignette.rectTransform);

        BuildHealthCard(canvas.transform);
        BuildTimer(canvas.transform);
        BuildRescuePrompt(canvas.transform);
        BuildKeyHints(canvas.transform);
        BuildIntroBanner(canvas.transform);

        toastRoot = UITheme.Rect("Toasts", canvas.transform);
        UITheme.Place(toastRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(800f, 400f));
        UITheme.VStack(toastRoot.gameObject, 0, 10f, TextAnchor.UpperCenter, false);
    }

    private void BuildHealthCard(Transform parent)
    {
        Image card = UITheme.Panel("HealthCard", parent, UITheme.HudPanel, UITheme.RadiusLarge);
        healthCard = card.rectTransform;
        UITheme.Place(healthCard, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(380f, 0f));
        UITheme.VStack(card.gameObject, 18, 12f, TextAnchor.UpperLeft);
        UITheme.FitHeight(card.gameObject);

        RectTransform header = UITheme.Rect("Header", card.transform);
        UITheme.HStack(header.gameObject, 8f, TextAnchor.MiddleLeft, true);
        UITheme.Layout(header.gameObject, -1f, 28f);
        UITheme.Text("Label", header, "Can", 22f, UITheme.TextSecondary);
        healthValue = UITheme.Text("Value", header, "", 22f, UITheme.TextPrimary, true, TextAlignmentOptions.Right);

        // Iz: hasardan sonra beyaz serit bir sure kalip yavasca erir.
        RectTransform track = UITheme.Rect("HealthTrack", card.transform);
        UITheme.Layout(track.gameObject, -1f, 14f);
        Image trackBg = track.gameObject.AddComponent<Image>();
        trackBg.sprite = UITheme.RoundedSprite(7);
        trackBg.type = Image.Type.Sliced;
        trackBg.color = UITheme.Track;
        trackBg.raycastTarget = false;

        healthGhost = UITheme.Panel("Ghost", track, UITheme.Ghost, 7).rectTransform;
        UITheme.Stretch(healthGhost);
        healthFillImage = UITheme.Panel("Fill", track, UITheme.Accent, 7);
        healthFill = healthFillImage.rectTransform;
        UITheme.Stretch(healthFill);

        RectTransform chips = UITheme.Rect("Chips", card.transform);
        UITheme.HStack(chips.gameObject, 8f, TextAnchor.MiddleLeft, false);
        UITheme.Layout(chips.gameObject, -1f, 34f);

        invisChip = UITheme.Chip(chips, "Invisibility", out invisText);
        if (invisibility == null)
        {
            invisChip.gameObject.SetActive(false);
        }

        Image power = UITheme.Chip(chips, "Power", out powerText);
        powerChip = power.rectTransform;
        if (combat == null)
        {
            power.gameObject.SetActive(false);
        }
    }

    private void BuildTimer(Transform parent)
    {
        Image chip = UITheme.Panel("Timer", parent, UITheme.HudPanel, UITheme.RadiusMedium);
        UITheme.Place(chip.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -32f), new Vector2(150f, 56f));
        timerText = UITheme.Text("Text", chip.transform, "00:00", 30f, UITheme.TextPrimary, UITheme.Weight.ExtraBold, TextAlignmentOptions.Center);
        UITheme.Stretch(timerText.rectTransform);
    }

    private void BuildRescuePrompt(Transform parent)
    {
        Image card = UITheme.Panel("RescuePrompt", parent, UITheme.HudPanel, UITheme.RadiusLarge);
        UITheme.Place(card.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(440f, 0f));
        UITheme.VStack(card.gameObject, 18, 12f, TextAnchor.UpperCenter);
        UITheme.FitHeight(card.gameObject);

        rescueGroup = card.gameObject.AddComponent<CanvasGroup>();
        rescueGroup.alpha = 0f;

        RectTransform row = UITheme.Rect("Row", card.transform);
        UITheme.HStack(row.gameObject, 14f, TextAnchor.MiddleCenter, false);
        UITheme.Layout(row.gameObject, -1f, 40f);
        UITheme.KeyBadge(row, "F");
        UITheme.Text("Text", row, "Basılı tut — Şövalyeyi kurtar", 24f, UITheme.TextPrimary);

        rescueFill = UITheme.Bar(card.transform, "Progress", 8f, UITheme.Accent, out _);
        UITheme.SetFill(rescueFill, 0f);
    }

    private void BuildKeyHints(Transform parent)
    {
        Image panel = UITheme.Panel("KeyHints", parent, UITheme.HudPanel, UITheme.RadiusMedium);
        UITheme.Place(panel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(32f, 28f), Vector2.zero);
        HorizontalLayoutGroup h = UITheme.HStack(panel.gameObject, 10f, TextAnchor.MiddleLeft, false);
        h.padding = new RectOffset(10, 16, 8, 8);
        ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        UITheme.KeyBadge(panel.transform, "I", 30f);
        UITheme.Text("Guide", panel.transform, "Rehber", 18f, UITheme.TextSecondary);
        UITheme.Layout(UITheme.Rect("Gap", panel.transform).gameObject, 8f);
        UITheme.KeyBadge(panel.transform, "ESC", 30f);
        UITheme.Text("Menu", panel.transform, "Menü", 18f, UITheme.TextSecondary);
    }

    /// <summary>Oyun basinda gorevi tanitan, birkac saniye sonra kaybolan baslik.</summary>
    private void BuildIntroBanner(Transform parent)
    {
        Image card = UITheme.Panel("Intro", parent, UITheme.HudPanel, UITheme.RadiusLarge + 4);
        introCard = card.rectTransform;
        UITheme.Place(introCard, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 0f));
        UITheme.VStack(card.gameObject, 34, 6f, TextAnchor.UpperCenter);
        UITheme.FitHeight(card.gameObject);

        UITheme.Overline("Overline", card.transform, "GÖREV", UITheme.Accent, TextAlignmentOptions.Center);
        UITheme.Heading("Title", card.transform, "Kayıp şövalyeyi kurtar", 60f, UITheme.TextPrimary, TextAlignmentOptions.Center);
        UITheme.Text("Hint", card.transform,
            $"Sağa doğru ilerle   ·   <color={UITheme.AccentHex}>I</color> ile rehberi aç",
            22f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);

        introGroup = card.gameObject.AddComponent<CanvasGroup>();
        introGroup.alpha = 0f;
    }

    // ---------------------------------------------------------------- Guncelleme

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        UpdateHealth(dt);
        UpdateChips(dt);
        UpdateRescue(dt);
        UpdateTimer();
        UpdateIntro(dt);

        // Oyun bitince HUD sonup sonuc ekranina yer acar.
        float targetAlpha = GameSession.Ended ? 0f : 1f;
        hudGroup.alpha = Mathf.MoveTowards(hudGroup.alpha, targetAlpha, dt * 4f);
    }

    private void UpdateHealth(float dt)
    {
        int max = Mathf.Max(1, health.maxHealth);
        int current = Mathf.Clamp(health.currentHealth, 0, max);

        if (current != lastHealth)
        {
            if (lastHealth != int.MinValue && current < lastHealth)
            {
                ghostHoldUntil = Time.unscaledTime + 0.45f;
                hitFlash = 1f;
                cardShake = 1f;
            }
            lastHealth = current;
            healthValue.text = $"{current} / {max}";
        }

        float target = (float)current / max;
        shownFraction = Mathf.MoveTowards(shownFraction, target, dt * 2.5f);

        if (target >= ghostFraction)
        {
            // Iyilesirken iz, dolu kismi takip eder.
            ghostFraction = shownFraction;
        }
        else if (Time.unscaledTime >= ghostHoldUntil)
        {
            ghostFraction = Mathf.MoveTowards(ghostFraction, target, dt * 0.8f);
        }

        UITheme.SetFill(healthFill, shownFraction);
        UITheme.SetFill(healthGhost, Mathf.Max(ghostFraction, shownFraction));

        healthFillImage.color = target > 0.45f
            ? Color.Lerp(UITheme.Warning, UITheme.Accent, Mathf.InverseLerp(0.45f, 0.6f, target))
            : Color.Lerp(UITheme.Danger, UITheme.Warning, Mathf.InverseLerp(0.2f, 0.45f, target));

        // Hasar aninda kart kisa bir sure titrer.
        cardShake = Mathf.MoveTowards(cardShake, 0f, dt * 4f);
        float shakeX = Mathf.Sin(Time.unscaledTime * 70f) * 6f * cardShake * cardShake;
        healthCard.anchoredPosition = new Vector2(32f + shakeX, -32f);

        // Dusuk canda nabiz gibi atan kenar + her darbede kisa parlama.
        float low = 0f;
        if (current > 0 && target <= lowHealthThreshold)
        {
            float severity = 1f - target / lowHealthThreshold;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
            low = Mathf.Lerp(0.2f, 0.45f, severity) * Mathf.Lerp(0.7f, 1f, pulse);
        }
        hitFlash = Mathf.MoveTowards(hitFlash, 0f, dt * 3f);
        vignette.color = UITheme.WithAlpha(UITheme.Danger, Mathf.Max(low, hitFlash * 0.45f));
    }

    private void UpdateChips(float dt)
    {
        if (invisibility != null)
        {
            int uses = invisibility.RemainingUses;
            bool active = invisibility.IsInvisible;
            if (uses != lastUses || active != lastInvisible)
            {
                lastUses = uses;
                lastInvisible = active;

                invisText.text = active
                    ? $"<color={UITheme.AccentHex}>Görünmezsin</color>"
                    : $"<color={UITheme.AccentHex}><b>H</b></color>  Görünmezlik  <b>{uses}</b>";
                invisText.alpha = active || uses > 0 ? 1f : 0.4f;
                invisChip.color = active ? UITheme.WithAlpha(UITheme.Accent, 0.25f) : UITheme.Secondary;
            }
        }

        if (combat != null)
        {
            int power = combat.attackDamage;
            if (power != lastPower)
            {
                if (lastPower != int.MinValue && power > lastPower)
                {
                    powerPulse = 1f;
                }
                lastPower = power;
                powerText.text = $"<color={UITheme.WarningHex}>Güç</color>  <b>{power}</b>";
            }

            powerPulse = Mathf.MoveTowards(powerPulse, 0f, dt * 2f);
            powerChip.localScale = Vector3.one * (1f + 0.18f * powerPulse * powerPulse);
        }
    }

    private void UpdateRescue(float dt)
    {
        bool show = rescue != null && rescue.IsInRange && !GameSession.Ended;
        rescueGroup.alpha = Mathf.MoveTowards(rescueGroup.alpha, show ? 1f : 0f, dt * 6f);
        UITheme.SetFill(rescueFill, rescue != null ? rescue.HoldProgress : 0f);
    }

    private void UpdateIntro(float dt)
    {
        if (introGroup == null)
        {
            return;
        }

        // Rehber/duraklatma acikken zaman ilerlemez; banner kaldigi yerden devam eder.
        if (!Mathf.Approximately(Time.timeScale, 0f))
        {
            introTime += dt;
        }

        const float delay = 0.5f;
        const float fadeIn = 0.45f;
        const float hold = 3.2f;
        const float fadeOut = 0.6f;

        float t = introTime - delay;
        float alpha;
        if (t <= 0f)
        {
            alpha = 0f;
        }
        else if (t < fadeIn)
        {
            alpha = UIAnim.EaseOutCubic(t / fadeIn);
        }
        else if (t < fadeIn + hold)
        {
            alpha = 1f;
        }
        else
        {
            alpha = 1f - Mathf.Clamp01((t - fadeIn - hold) / fadeOut);
        }

        // Rehber ya da duraklatma menusu acikken banner arkada asili kalmasin.
        bool paused = Mathf.Approximately(Time.timeScale, 0f);
        introGroup.alpha = Mathf.MoveTowards(introGroup.alpha, paused ? 0f : alpha, dt * 8f);
        introCard.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, Mathf.Clamp01(t / fadeIn));

        if (t > fadeIn + hold + fadeOut)
        {
            Destroy(introCard.gameObject);
            introGroup = null;
        }
    }

    private void UpdateTimer()
    {
        int second = Mathf.FloorToInt(GameSession.Elapsed);
        if (second != lastSecond)
        {
            lastSecond = second;
            timerText.text = GameSession.FormatTime(second);
        }
    }

    // ----------------------------------------------------------------- Bildirim

    private IEnumerator ToastRoutine(string message, Color color)
    {
        Image chip = UITheme.Panel("Toast", toastRoot, UITheme.HudPanel, UITheme.RadiusMedium);
        HorizontalLayoutGroup h = UITheme.HStack(chip.gameObject, 0f, TextAnchor.MiddleCenter, false);
        h.padding = new RectOffset(24, 24, 10, 10);
        UITheme.Text("Text", chip.transform, message, 28f, color, true, TextAlignmentOptions.Center);

        CanvasGroup group = chip.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        RectTransform rt = chip.rectTransform;

        const float fadeIn = 0.18f;
        const float hold = 1.3f;
        const float fadeOut = 0.4f;

        float t = 0f;
        while (t < fadeIn + hold + fadeOut)
        {
            t += Time.unscaledDeltaTime;
            if (t < fadeIn)
            {
                float k = UIAnim.EaseOutCubic(t / fadeIn);
                group.alpha = k;
                rt.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, k);
            }
            else if (t > fadeIn + hold)
            {
                group.alpha = 1f - (t - fadeIn - hold) / fadeOut;
                rt.localScale = Vector3.one;
            }
            else
            {
                group.alpha = 1f;
                rt.localScale = Vector3.one;
            }
            yield return null;
        }

        Destroy(chip.gameObject);
    }
}
