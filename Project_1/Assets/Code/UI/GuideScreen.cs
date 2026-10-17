using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Oyun ici rehber (I tusu). Bes sekme: Gorev, Kontroller, Iksirler, Dusmanlar,
/// Tehlikeler. Gorseller sahnedeki objelerin sprite'larindan, sayilar
/// betiklerdeki degerlerden okunur; oyun dengesi degisince rehber kendiliginden
/// guncel kalir. Acma/kapama ve duraklatma InfoPanelManager'dadir.
/// </summary>
public class GuideScreen : MonoBehaviour
{
    private static readonly string[] TabNames = { "Görev", "Kontroller", "İksirler", "Düşmanlar", "Tehlikeler" };

    private const float CardWidth = 1400f;
    private const float CardHeight = 860f;
    private const float Pad = 56f;
    private const float PagesTop = 258f;

    /// <summary>Kapat butonuna ya da karti disina tiklaninca cagrilir.</summary>
    public Action CloseRequested;

    public bool Visible { get; private set; }

    private CanvasGroup group;
    private RectTransform card;
    private RectTransform pagesRoot;

    private readonly List<UITabButton> tabs = new List<UITabButton>();
    private readonly List<RectTransform> pages = new List<RectTransform>();
    private readonly List<List<CanvasGroup>> pageItems = new List<List<CanvasGroup>>();

    private int current = -1;
    private Coroutine fadeRoutine;
    private Coroutine revealRoutine;

    void Awake()
    {
        Build();
    }

    // ------------------------------------------------------------------ Disari

    /// <summary>Rehberi acar. page &lt; 0 ise en son bakilan sekmede acilir.</summary>
    public void Show(int page)
    {
        Visible = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(UIAnim.Show(group, card, true, 0.24f));

        SelectPage(page >= 0 ? page : Mathf.Max(0, current), true);
    }

    public void Hide()
    {
        Visible = false;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(UIAnim.Show(group, card, false, 0.18f));
    }

    void Update()
    {
        if (!Visible)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Tab))
        {
            SelectPage((current + 1) % pages.Count, false);
        }
        else if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPage((current - 1 + pages.Count) % pages.Count, false);
        }

        for (int i = 0; i < pages.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectPage(i, false);
            }
        }
    }

    private void SelectPage(int index, bool force)
    {
        if (index == current && !force)
        {
            return;
        }

        current = index;
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].gameObject.SetActive(i == index);
            tabs[i].SetActive(i == index);
        }

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
        }
        revealRoutine = StartCoroutine(RevealPage(pageItems[index]));
    }

    /// <summary>Sayfadaki kartlar sirayla, hafif buyuyerek belirir.</summary>
    private IEnumerator RevealPage(List<CanvasGroup> items)
    {
        const float step = 0.05f;
        const float duration = 0.28f;

        foreach (CanvasGroup item in items)
        {
            item.alpha = 0f;
        }

        float total = duration + step * items.Count;
        float t = 0f;
        while (t < total)
        {
            t += Time.unscaledDeltaTime;
            for (int i = 0; i < items.Count; i++)
            {
                float k = UIAnim.EaseOutCubic(Mathf.Clamp01((t - i * step) / duration));
                items[i].alpha = k;
                items[i].transform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, k);
            }
            yield return null;
        }

        foreach (CanvasGroup item in items)
        {
            item.alpha = 1f;
            item.transform.localScale = Vector3.one;
        }
    }

    // ------------------------------------------------------------------ Kurulum

    private void Build()
    {
        Canvas canvas = UITheme.CreateCanvas(transform, "Guide", 55);

        // Karti disina tiklamak rehberi kapatir.
        Image backdrop = UITheme.Panel("Backdrop", canvas.transform, new Color(0.02f, 0.03f, 0.05f, 0.82f), 0);
        UITheme.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;
        Button backdropButton = backdrop.gameObject.AddComponent<Button>();
        backdropButton.transition = Selectable.Transition.None;
        backdropButton.navigation = new Navigation { mode = Navigation.Mode.None };
        backdropButton.onClick.AddListener(() => CloseRequested?.Invoke());
        group = backdrop.gameObject.AddComponent<CanvasGroup>();

        Image cardImage = UITheme.Panel("Card", backdrop.transform, UITheme.Card, UITheme.RadiusLarge + 6);
        cardImage.raycastTarget = true; // Kart icine tiklama arka plana gecmesin.
        card = cardImage.rectTransform;
        UITheme.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CardWidth, CardHeight));

        BuildHeader(card);
        BuildTabs(card);

        pagesRoot = UITheme.Rect("Pages", card);
        pagesRoot.anchorMin = Vector2.zero;
        pagesRoot.anchorMax = Vector2.one;
        pagesRoot.offsetMin = new Vector2(Pad, Pad);
        pagesRoot.offsetMax = new Vector2(-Pad, -PagesTop);

        BuildMissionPage();
        BuildControlsPage();
        BuildPotionsPage();
        BuildEnemiesPage();
        BuildHazardsPage();

        group.alpha = 0f;
        backdrop.gameObject.SetActive(false);
    }

    private void BuildHeader(Transform parent)
    {
        RectTransform header = UITheme.Rect("Header", parent);
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.anchoredPosition = new Vector2(0f, -48f);
        header.sizeDelta = new Vector2(-Pad * 2f, 100f);

        RectTransform titles = UITheme.Rect("Titles", header);
        titles.anchorMin = Vector2.zero;
        titles.anchorMax = new Vector2(0.62f, 1f);
        titles.offsetMin = Vector2.zero;
        titles.offsetMax = Vector2.zero;
        UITheme.VStack(titles.gameObject, 0, 4f, TextAnchor.MiddleLeft);
        UITheme.Overline("Overline", titles, "REHBER", UITheme.Accent);
        UITheme.Heading("Title", titles, "Hayatta kalma rehberi", 54f, UITheme.TextPrimary);

        RectTransform right = UITheme.Rect("Actions", header);
        right.anchorMin = new Vector2(0.4f, 0f);
        right.anchorMax = Vector2.one;
        right.offsetMin = Vector2.zero;
        right.offsetMax = Vector2.zero;
        UITheme.HStack(right.gameObject, 10f, TextAnchor.MiddleRight, false);

        UITheme.KeyBadge(right, "Q", 34f);
        UITheme.KeyBadge(right, "E", 34f);
        UITheme.Text("TabsHint", right, "Sekme", 20f, UITheme.TextSecondary);
        UITheme.Layout(UITheme.Rect("Gap", right).gameObject, 18f);
        UITheme.KeyBadge(right, "I", 34f);
        UITheme.Text("CloseHint", right, "Kapat", 20f, UITheme.TextSecondary);
        UITheme.Layout(UITheme.Rect("Gap", right).gameObject, 18f);

        Image close = UITheme.Panel("Close", right, UITheme.Secondary, UITheme.RadiusMedium);
        close.raycastTarget = true;
        UITheme.Layout(close.gameObject, 56f, 56f);
        Button closeButton = close.gameObject.AddComponent<Button>();
        closeButton.transition = Selectable.Transition.None;
        closeButton.navigation = new Navigation { mode = Navigation.Mode.None };
        closeButton.onClick.AddListener(() => CloseRequested?.Invoke());
        close.gameObject.AddComponent<UIButtonFx>().Init(close, UITheme.Secondary, UITheme.SecondaryHover);
        TextMeshProUGUI x = UITheme.Text("X", close.transform, "×", 38f, UITheme.TextPrimary, UITheme.Weight.SemiBold,
            TextAlignmentOptions.Center);
        UITheme.Stretch(x.rectTransform);
    }

    private void BuildTabs(Transform parent)
    {
        Image bar = UITheme.Panel("Tabs", parent, UITheme.Secondary, UITheme.RadiusMedium + 2);
        RectTransform rt = bar.rectTransform;
        UITheme.Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Pad, -168f), Vector2.zero);
        HorizontalLayoutGroup h = UITheme.HStack(bar.gameObject, 6f, TextAnchor.MiddleLeft, false);
        h.padding = new RectOffset(6, 6, 6, 6);
        ContentSizeFitter fitter = bar.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < TabNames.Length; i++)
        {
            int index = i;
            Image tab = UITheme.Panel(TabNames[i], bar.transform, UITheme.WithAlpha(Color.white, 0f), UITheme.RadiusSmall + 2);
            tab.raycastTarget = true;
            UITheme.Layout(tab.gameObject, 196f, 50f);

            TextMeshProUGUI label = UITheme.Text("Label", tab.transform, $"{TabNames[i]}", 22f, UITheme.TextSecondary,
                UITheme.Weight.SemiBold, TextAlignmentOptions.Center);
            UITheme.Stretch(label.rectTransform);

            UITabButton button = tab.gameObject.AddComponent<UITabButton>();
            button.Init(tab, label, () => SelectPage(index, false));
            tabs.Add(button);
        }
    }

    // ------------------------------------------------------------- Sayfalar

    private RectTransform NewPage(string name, out List<CanvasGroup> items)
    {
        RectTransform page = UITheme.Rect(name, pagesRoot);
        UITheme.Stretch(page);
        page.gameObject.SetActive(false);
        pages.Add(page);

        items = new List<CanvasGroup>();
        pageItems.Add(items);
        return page;
    }

    private static Image ItemCard(Transform parent, List<CanvasGroup> items, Color color)
    {
        Image cardImage = UITheme.Panel("Card", parent, color, UITheme.RadiusLarge);
        items.Add(cardImage.gameObject.AddComponent<CanvasGroup>());
        return cardImage;
    }

    private void BuildMissionPage()
    {
        RectTransform page = NewPage("Mission", out List<CanvasGroup> items);
        HorizontalLayoutGroup row = UITheme.HStack(page.gameObject, 36f, TextAnchor.UpperLeft, false);
        row.childForceExpandHeight = true;

        // Sol: kurtarilacak sovalye.
        Image art = ItemCard(page, items, UITheme.Secondary);
        UITheme.Layout(art.gameObject, 440f);
        UITheme.VStack(art.gameObject, 28, 18f, TextAnchor.MiddleCenter, false);
        RectTransform box = SpriteBox(art.transform, FindKnightSprite(), 330f, UITheme.Accent, "?");
        UITheme.Layout(box.gameObject, 330f, 330f);
        Image nameChip = UITheme.Chip(art.transform, "Name", out TextMeshProUGUI nameText, 22f);
        nameChip.color = UITheme.HudPanel;
        nameText.text = $"<color={UITheme.AccentHex}>Kayıp şövalye</color>";

        // Sag: hikaye ve adimlar.
        RectTransform info = UITheme.Rect("Info", page);
        UITheme.Layout(info.gameObject).flexibleWidth = 1f;
        UITheme.VStack(info.gameObject, 0, 14f, TextAnchor.UpperLeft);
        items.Add(info.gameObject.AddComponent<CanvasGroup>());

        UITheme.Overline("Overline", info, "GÖREVİN", UITheme.Accent);
        UITheme.Heading("Heading", info, "Kayıp şövalyeyi kurtar", 50f, UITheme.TextPrimary);
        UITheme.Text("Story", info,
            "Ovanın sonunda yaralı bir şövalye yardım bekliyor. Yolda iskeletler, Ölüm Getiren ve ağır zırhlı bir savaşçı var. " +
            "Tuzaklardan kaç, iksirleri akıllıca kullan ve ona ulaş.",
            23f, UITheme.TextSecondary);
        UITheme.Spacer(info, 6f);

        PlayerRescue rescue = FindFirstObjectByType<PlayerRescue>();
        float hold = rescue != null ? rescue.holdTime : 2f;

        Step(info, "1", "Sağa doğru ilerle", "Düşmanları alt et ya da görünmez olup yanlarından sıyrıl.");
        Step(info, "2", "Şövalyeye ulaş", "Haritanın sonunda seni bekliyor; yaklaşınca ekranda istem belirir.");
        Step(info, "3", "F'yi basılı tut", $"{hold:0.#} saniye basılı tutarak onu kurtar ve oyunu kazan.");

        UITheme.Spacer(info, 8f);
        RectTransform chips = UITheme.Rect("Chips", info);
        UITheme.HStack(chips.gameObject, 10f, TextAnchor.MiddleLeft, false);
        UITheme.Layout(chips.gameObject, -1f, 40f);

        int enemies = FindObjectsByType<EnemyHealthBase>(FindObjectsSortMode.None).Length;
        int potions = FindObjectsByType<HealthPotion>(FindObjectsSortMode.None).Length
                      + FindObjectsByType<PoisonPotion>(FindObjectsSortMode.None).Length
                      + FindObjectsByType<YellowPotion>(FindObjectsSortMode.None).Length;
        StatChip(chips, "Düşman", enemies.ToString());
        StatChip(chips, "İksir", potions.ToString());
        StatChip(chips, "En iyi süre", GameSession.HasBestTime ? GameSession.FormatTime(GameSession.BestTime) : "henüz yok");
    }

    private void BuildControlsPage()
    {
        RectTransform page = NewPage("Controls", out List<CanvasGroup> items);
        GridLayoutGroup grid = page.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(632f, 120f);
        grid.spacing = new Vector2(24f, 22f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        PlayerInvisibility invisibility = FindFirstObjectByType<PlayerInvisibility>();
        int uses = invisibility != null ? invisibility.maxUses : 2;

        ControlCard(page, items, new[] { "A", "D" }, "Hareket", "Sola ve sağa koş.");
        ControlCard(page, items, new[] { "Space" }, "Zıpla", "Havadayken bir kez daha zıplayabilirsin.");
        ControlCard(page, items, new[] { "E" }, "Saldır", "Dururken ya da koşarken vur. Saniyede bir kez.");
        ControlCard(page, items, new[] { "H" }, "Görünmezlik", $"Kısa süre düşmanlar seni göremez. {uses} hakkın var.");
        ControlCard(page, items, new[] { "F" }, "Kurtar", "Şövalyenin yanında basılı tut.");
        ControlCard(page, items, new[] { "I" }, "Rehber", "Bu ekranı aç ya da kapat.");
        ControlCard(page, items, new[] { "ESC" }, "Duraklat", "Devam et, yeniden başla ya da ana menüye dön.");
        ControlCard(page, items, null, "Ekranı tanı", "Sol üstte can, görünmezlik ve güç; sağ üstte süre.");
    }

    private void BuildPotionsPage()
    {
        RectTransform page = NewPage("Potions", out List<CanvasGroup> items);
        UITheme.VStack(page.gameObject, 0, 20f, TextAnchor.UpperLeft);

        RectTransform row = UITheme.Rect("Row", page);
        UITheme.HStack(row.gameObject, 24f, TextAnchor.UpperLeft, true).childForceExpandHeight = true;
        UITheme.Layout(row.gameObject, -1f, 456f);

        HealthPotion health = FindFirstObjectByType<HealthPotion>();
        PoisonPotion poison = FindFirstObjectByType<PoisonPotion>();
        YellowPotion power = FindFirstObjectByType<YellowPotion>();
        CharacterHealth player = FindFirstObjectByType<CharacterHealth>();
        int maxHealth = player != null ? player.maxHealth : 100;

        InfoCard(row, items, SpriteOf(health), UITheme.Danger, "Can iksiri", "Kırmızı şişe",
            $"+{(health != null ? health.healAmount : 30)} can", UITheme.Success,
            $"Canını yeniler, en fazla {maxHealth} cana kadar.",
            CountLine(FindObjectsByType<HealthPotion>(FindObjectsSortMode.None).Length));

        InfoCard(row, items, SpriteOf(poison), UITheme.Accent, "Zehir iksiri", "Turkuaz şişe",
            $"-{(poison != null ? poison.poisonDamage : 20)} can", UITheme.Danger,
            "Bir tuzak. Dokunduğun an canın azalır, ondan uzak dur.",
            CountLine(FindObjectsByType<PoisonPotion>(FindObjectsSortMode.None).Length));

        InfoCard(row, items, SpriteOf(power), UITheme.Warning, "Güç iksiri", "Sarı şişe",
            $"+{(power != null ? power.extraDamage : 20)} saldırı", UITheme.Warning,
            "Saldırı gücünü kalıcı olarak artırır.",
            CountLine(FindObjectsByType<YellowPotion>(FindObjectsSortMode.None).Length));

        TipBar(page, items, "Güç iksiri kalıcıdır; en zorlu rakibe ulaşmadan önce topla.");
    }

    private void BuildEnemiesPage()
    {
        RectTransform page = NewPage("Enemies", out List<CanvasGroup> items);
        HorizontalLayoutGroup row = UITheme.HStack(page.gameObject, 24f, TextAnchor.UpperLeft, true);
        row.childForceExpandHeight = true;

        // Her dusman turunden bir ornek, haritada karsilasilma sirasiyla.
        List<EnemyHealthBase> types = new List<EnemyHealthBase>();
        HashSet<Type> seen = new HashSet<Type>();
        EnemyHealthBase[] all = FindObjectsByType<EnemyHealthBase>(FindObjectsSortMode.None);
        Array.Sort(all, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        foreach (EnemyHealthBase e in all)
        {
            if (seen.Add(e.GetType()))
            {
                types.Add(e);
            }
        }

        if (types.Count == 0)
        {
            TipBar(page, items, "Bu bölümde düşman yok.");
            return;
        }

        float maxHp = 1f, maxDamage = 1f, maxSpeed = 0.01f;
        float minScore = float.MaxValue, maxScore = float.MinValue;
        foreach (EnemyHealthBase e in types)
        {
            EnemyCombatBase combat = e.GetComponent<EnemyCombatBase>();
            EnemyAIBase ai = e.GetComponent<EnemyAIBase>();
            maxHp = Mathf.Max(maxHp, e.maxhealth);
            maxDamage = Mathf.Max(maxDamage, combat != null ? combat.attackDamage : 0);
            maxSpeed = Mathf.Max(maxSpeed, ai != null ? ai.followspeed : 0f);
            float score = Score(e);
            minScore = Mathf.Min(minScore, score);
            maxScore = Mathf.Max(maxScore, score);
        }

        for (int i = 0; i < types.Count; i++)
        {
            EnemyHealthBase e = types[i];
            EnemyCombatBase combat = e.GetComponent<EnemyCombatBase>();
            EnemyAIBase ai = e.GetComponent<EnemyAIBase>();
            Describe(e, out string enemyName, out string description);

            float level = maxScore > minScore ? Mathf.InverseLerp(minScore, maxScore, Score(e)) : 0.5f;
            string tag = level < 0.34f ? "Kolay" : level < 0.67f ? "Orta" : "Zorlu";
            Color tagColor = level < 0.34f ? UITheme.Success : level < 0.67f ? UITheme.Warning : UITheme.Danger;

            Image cardImage = ItemCard(page, items, UITheme.Secondary);
            UITheme.VStack(cardImage.gameObject, 28, 12f, TextAnchor.UpperLeft);

            UITheme.Overline("Order", cardImage.transform, $"{i + 1}. KARŞILAŞMA", UITheme.TextSecondary).fontSize = 16f;
            SpriteBox(cardImage.transform, SpriteOf(e), 150f, tagColor, "?");

            RectTransform nameRow = UITheme.Rect("NameRow", cardImage.transform);
            UITheme.HStack(nameRow.gameObject, 12f, TextAnchor.MiddleLeft, false);
            UITheme.Layout(nameRow.gameObject, -1f, 44f);
            UITheme.Heading("Name", nameRow, enemyName, 32f, UITheme.TextPrimary);
            Pill(nameRow, tag, tagColor);

            int damage = combat != null ? combat.attackDamage : 0;
            float speed = ai != null ? ai.followspeed : 0f;
            StatRow(cardImage.transform, "Can", e.maxhealth / maxHp, e.maxhealth.ToString(), UITheme.Success);
            StatRow(cardImage.transform, "Hasar", damage / maxDamage, damage.ToString(), UITheme.Danger);
            StatRow(cardImage.transform, "Hız", speed / maxSpeed, speed.ToString("0.#"), UITheme.Accent);

            UITheme.Text("Description", cardImage.transform, description, 19f, UITheme.TextSecondary);
        }
    }

    private void BuildHazardsPage()
    {
        RectTransform page = NewPage("Hazards", out List<CanvasGroup> items);
        UITheme.VStack(page.gameObject, 0, 20f, TextAnchor.UpperLeft);

        RectTransform row = UITheme.Rect("Row", page);
        UITheme.HStack(row.gameObject, 24f, TextAnchor.UpperLeft, true).childForceExpandHeight = true;
        UITheme.Layout(row.gameObject, -1f, 456f);

        FireDamage fire = FindFirstObjectByType<FireDamage>();
        int fires = FindObjectsByType<FireDamage>(FindObjectsSortMode.None).Length;
        string fireEffect = fire != null
            ? $"-{fire.damageAmount} can / {fire.damageInterval:0.#} sn"
            : "Sürekli hasar";
        InfoCard(row, items, SpriteOf(fire), UITheme.Warning, "Ateş", "Alevler", fireEffect, UITheme.Danger,
            "İçinde durduğun sürece yanarsın. Hızla geç.", CountLine(fires));

        int tiles = FindObjectsByType<FallingTile>(FindObjectsSortMode.None).Length
                    + FindObjectsByType<FallingTile2>(FindObjectsSortMode.None).Length;
        InfoCard(row, items, FindFallingTileSprite(), UITheme.Accent, "Düşen platform", "Çürük zemin", "Durma, geç",
            UITheme.Warning, "Üstüne basınca bir an sonra çöker.", CountLine(tiles));

        InfoCard(row, items, null, UITheme.Danger, "Uçurum", "Haritanın dışı", "Oyun biter", UITheme.Danger,
            "Aşağı düşersen oyun biter. Çift zıplamayı akıllıca kullan.", "");

        TipBar(page, items, "Görünmezlik ateşe karşı korumaz; yalnızca düşmanlar seni göremez.");
    }

    // ---------------------------------------------------------- Parcalar

    private RectTransform SpriteBox(Transform parent, Sprite sprite, float height, Color glowColor, string fallback)
    {
        RectTransform box = UITheme.Rect("Art", parent);
        UITheme.Layout(box.gameObject, -1f, height);

        Image glow = UITheme.Panel("Glow", box, UITheme.WithAlpha(glowColor, 0.22f), 0);
        glow.sprite = UITheme.GlowSprite();
        UITheme.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(height * 1.3f, height * 1.3f));

        if (sprite != null)
        {
            Image art = UITheme.Panel("Sprite", box, Color.white, 0);
            art.sprite = sprite;
            art.preserveAspect = true;
            UITheme.Stretch(art.rectTransform, 6f);
        }
        else
        {
            TextMeshProUGUI icon = UITheme.Heading("Icon", box, fallback, height * 0.55f, glowColor, TextAlignmentOptions.Center);
            UITheme.Stretch(icon.rectTransform);
        }
        return box;
    }

    private void InfoCard(Transform parent, List<CanvasGroup> items, Sprite sprite, Color glow, string title,
        string subtitle, string effect, Color effectColor, string description, string footer)
    {
        Image cardImage = ItemCard(parent, items, UITheme.Secondary);
        UITheme.VStack(cardImage.gameObject, 28, 10f, TextAnchor.UpperCenter);

        SpriteBox(cardImage.transform, sprite, 140f, glow, "!");
        UITheme.Heading("Title", cardImage.transform, title, 32f, UITheme.TextPrimary, TextAlignmentOptions.Center);
        UITheme.Text("Subtitle", cardImage.transform, subtitle, 19f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);

        Image pill = UITheme.Panel("Effect", cardImage.transform, UITheme.WithAlpha(effectColor, 0.16f), UITheme.RadiusMedium);
        UITheme.Layout(pill.gameObject, -1f, 46f);
        TextMeshProUGUI effectText = UITheme.Text("Text", pill.transform, effect, 24f, effectColor, UITheme.Weight.ExtraBold,
            TextAlignmentOptions.Center);
        UITheme.Stretch(effectText.rectTransform);

        UITheme.Text("Description", cardImage.transform, description, 20f, UITheme.TextPrimary, false, TextAlignmentOptions.Center);
        if (!string.IsNullOrEmpty(footer))
        {
            UITheme.Text("Footer", cardImage.transform, footer, 18f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);
        }
    }

    private void ControlCard(Transform parent, List<CanvasGroup> items, string[] keys, string title, string description)
    {
        bool isInfo = keys == null;
        Image cardImage = ItemCard(parent, items, isInfo ? UITheme.WithAlpha(UITheme.Accent, 0.1f) : UITheme.Secondary);
        HorizontalLayoutGroup h = UITheme.HStack(cardImage.gameObject, 22f, TextAnchor.MiddleLeft, false);
        h.padding = new RectOffset(24, 24, 16, 16);

        RectTransform keyArea = UITheme.Rect("Keys", cardImage.transform);
        UITheme.HStack(keyArea.gameObject, 8f, TextAnchor.MiddleLeft, false);
        UITheme.Layout(keyArea.gameObject, 150f, 52f);
        if (isInfo)
        {
            Image badge = UITheme.Panel("Hud", keyArea, UITheme.Track, UITheme.RadiusSmall);
            UITheme.Layout(badge.gameObject, 76f, 52f);
            TextMeshProUGUI t = UITheme.Text("Text", badge.transform, "HUD", 22f, UITheme.Accent, UITheme.Weight.ExtraBold,
                TextAlignmentOptions.Center);
            UITheme.Stretch(t.rectTransform);
        }
        else
        {
            foreach (string key in keys)
            {
                UITheme.KeyBadge(keyArea, key, 52f);
            }
        }

        RectTransform text = UITheme.Rect("Text", cardImage.transform);
        UITheme.Layout(text.gameObject).flexibleWidth = 1f;
        UITheme.VStack(text.gameObject, 0, 4f, TextAnchor.MiddleLeft);
        UITheme.Text("Title", text, title, 26f, UITheme.TextPrimary, UITheme.Weight.SemiBold);
        UITheme.Text("Description", text, description, 19f, UITheme.TextSecondary);
    }

    private static void Step(Transform parent, string number, string title, string description)
    {
        RectTransform row = UITheme.Rect("Step " + number, parent);
        UITheme.HStack(row.gameObject, 18f, TextAnchor.UpperLeft, false);

        Image circle = UITheme.Panel("Number", row, UITheme.WithAlpha(UITheme.Accent, 0.18f), 22);
        UITheme.Layout(circle.gameObject, 44f, 44f);
        TextMeshProUGUI n = UITheme.Text("Text", circle.transform, number, 22f, UITheme.Accent, UITheme.Weight.ExtraBold,
            TextAlignmentOptions.Center);
        UITheme.Stretch(n.rectTransform);

        RectTransform text = UITheme.Rect("Text", row);
        UITheme.Layout(text.gameObject).flexibleWidth = 1f;
        UITheme.VStack(text.gameObject, 0, 2f, TextAnchor.UpperLeft);
        UITheme.Text("Title", text, title, 24f, UITheme.TextPrimary, UITheme.Weight.SemiBold);
        UITheme.Text("Description", text, description, 19f, UITheme.TextSecondary);
    }

    private static void StatChip(Transform parent, string label, string value)
    {
        UITheme.Chip(parent, label, out TextMeshProUGUI text, 20f);
        text.text = $"<color={UITheme.SecondaryHex}>{label}</color>   <b>{value}</b>";
    }

    private static void StatRow(Transform parent, string label, float fraction, string value, Color color)
    {
        RectTransform row = UITheme.Rect(label, parent);
        UITheme.HStack(row.gameObject, 12f, TextAnchor.MiddleLeft, false);
        UITheme.Layout(row.gameObject, -1f, 26f);

        TextMeshProUGUI name = UITheme.Text("Label", row, label, 18f, UITheme.TextSecondary);
        UITheme.Layout(name.gameObject, 70f);

        RectTransform fill = UITheme.Bar(row, "Bar", 8f, color, out _);
        fill.parent.GetComponent<LayoutElement>().flexibleWidth = 1f;
        UITheme.SetFill(fill, Mathf.Max(0.06f, fraction));

        TextMeshProUGUI number = UITheme.Text("Value", row, value, 18f, UITheme.TextPrimary, UITheme.Weight.SemiBold,
            TextAlignmentOptions.Right);
        UITheme.Layout(number.gameObject, 48f);
    }

    private static void Pill(Transform parent, string text, Color color)
    {
        Image pill = UITheme.Panel("Pill", parent, UITheme.WithAlpha(color, 0.16f), UITheme.RadiusSmall);
        HorizontalLayoutGroup h = UITheme.HStack(pill.gameObject, 0f, TextAnchor.MiddleCenter, false);
        h.padding = new RectOffset(12, 12, 4, 4);
        UITheme.Text("Text", pill.transform, text, 18f, color, UITheme.Weight.SemiBold);
    }

    private static void TipBar(Transform parent, List<CanvasGroup> items, string tip)
    {
        Image bar = ItemCard(parent, items, UITheme.WithAlpha(UITheme.Accent, 0.1f));
        UITheme.Layout(bar.gameObject, -1f, 70f);
        HorizontalLayoutGroup h = UITheme.HStack(bar.gameObject, 16f, TextAnchor.MiddleLeft, false);
        h.padding = new RectOffset(26, 26, 0, 0);
        UITheme.Text("Label", bar.transform, "İPUCU", 18f, UITheme.Accent, UITheme.Weight.ExtraBold).characterSpacing = 6f;
        UITheme.Text("Text", bar.transform, tip, 21f, UITheme.TextPrimary);
    }

    private static string CountLine(int count)
    {
        return count > 0 ? $"Haritada {count} tane" : "";
    }

    // ------------------------------------------------------------ Veri

    private static Sprite SpriteOf(Component component)
    {
        if (component == null)
        {
            return null;
        }
        SpriteRenderer sr = component.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }

    private static Sprite FindKnightSprite()
    {
        GameObject knight = null;
        try
        {
            knight = GameObject.FindGameObjectWithTag("FinalNPC");
        }
        catch (UnityException)
        {
            // Etiket tanimli degilse.
        }

        if (knight == null)
        {
            PlayerRescue rescue = FindFirstObjectByType<PlayerRescue>();
            if (rescue != null && rescue.rescueTarget != null)
            {
                knight = rescue.rescueTarget.gameObject;
            }
        }
        return knight != null ? SpriteOf(knight.transform) : null;
    }

    /// <summary>Dusen platform bir Tilemap; ilk dolu karonun sprite'ini alir.</summary>
    private static Sprite FindFallingTileSprite()
    {
        MonoBehaviour tile = FindFirstObjectByType<FallingTile>();
        if (tile == null)
        {
            tile = FindFirstObjectByType<FallingTile2>();
        }
        if (tile == null)
        {
            return null;
        }

        Tilemap map = tile.GetComponent<Tilemap>();
        if (map == null)
        {
            return SpriteOf(tile);
        }

        foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
        {
            Sprite sprite = map.GetSprite(cell);
            if (sprite != null)
            {
                return sprite;
            }
        }
        return null;
    }

    private static float Score(EnemyHealthBase e)
    {
        EnemyCombatBase combat = e.GetComponent<EnemyCombatBase>();
        return e.maxhealth * (combat != null ? combat.attackDamage : 1);
    }

    private static void Describe(EnemyHealthBase e, out string enemyName, out string description)
    {
        if (e is Enemy)
        {
            enemyName = "İskelet";
            description = "Yavaş ama inatçı. Arkasına geçersen bir süre sonra seni kaybeder.";
        }
        else if (e is Fight)
        {
            enemyName = "Ölüm Getiren";
            description = "Seni görünce hızla üstüne gelir. Görünmez olup yanından sıyrıl.";
        }
        else if (e is Warrior)
        {
            enemyName = "Savaşçı";
            description = "En dayanıklı rakip. Güç iksirini almadan karşısına çıkma.";
        }
        else
        {
            enemyName = e.GetType().Name;
            description = "Seni görünce takip eder ve yaklaşınca vurur.";
        }
    }
}
