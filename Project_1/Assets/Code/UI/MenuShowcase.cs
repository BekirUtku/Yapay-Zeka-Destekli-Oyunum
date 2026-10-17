using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana menunun sag tarafindaki hareketli sahne: yuzen bir adanin ustunde
/// bekleyen ve arada kilic savuran kahraman, karsisinda ikinci adada bir
/// iskelet, arkada daglar, isik halkasi ve fareye gore derinlik (parallax).
/// Sprite'lar Resources/UI/MenuShowcase.asset'ten gelir; kareler koddan oynatilir.
/// </summary>
public class MenuShowcase : MonoBehaviour
{
    // Kahraman ve ana adanin piksel olcegi (kanvas birimi / sprite pikseli).
    private const float HeroScale = 8f;
    private const float EnemyScale = 4.5f;
    private const float EnemyIslandScale = 5f;
    private const float Surface = -150f;
    private const int ImpactFrame = 4;

    private static readonly Vector2 RootPosition = new Vector2(-500f, -30f);
    private static readonly Vector2 MainPosition = new Vector2(-40f, 0f);
    private static readonly Vector2 EnemyPosition = new Vector2(330f, 120f);
    private static readonly Vector2 GlowPosition = new Vector2(-40f, 30f);

    private MenuShowcaseData data;

    private RectTransform root;
    private CanvasGroup rootGroup;
    private RectTransform mountainsLayer;
    private RectTransform glowLayer;
    private RectTransform mainGroup;
    private RectTransform enemyGroup;

    private Image hero;
    private RectTransform heroRt;
    private Vector2 heroBase;
    private Image enemy;
    private RectTransform enemyRt;
    private Vector2 enemyBase;
    private Color enemyTint = new Color(0.72f, 0.78f, 0.84f, 1f);
    private RectTransform ring;
    private Image shockRing;

    private float startTime;
    private Vector2 parallax;

    private float heroTimer;
    private bool attacking;
    private bool impactDone;
    private float nextAttack;
    private float enemyTimer;
    private float shock;
    private float hitFlash;

    /// <summary>Vitrini verilen canvas'a kurar. Veri bulunamazsa null doner.</summary>
    public static MenuShowcase Create(Transform canvas)
    {
        MenuShowcaseData data = Resources.Load<MenuShowcaseData>("UI/MenuShowcase");
        if (data == null || data.heroIdle == null || data.heroIdle.Length == 0 || data.heroIdle[0] == null)
        {
            Debug.LogWarning("MenuShowcase: Resources/UI/MenuShowcase bulunamadi ya da bos, vitrin gosterilmiyor.");
            return null;
        }

        RectTransform rt = UITheme.Rect("Showcase", canvas);
        MenuShowcase showcase = rt.gameObject.AddComponent<MenuShowcase>();
        showcase.data = data;
        showcase.Build(rt);
        return showcase;
    }

    // ------------------------------------------------------------------ Kurulum

    private void Build(RectTransform rt)
    {
        root = rt;
        UITheme.Place(root, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), RootPosition, new Vector2(1000f, 1000f));
        rootGroup = root.gameObject.AddComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.blocksRaycasts = false;
        rootGroup.interactable = false;

        BuildMountains();

        glowLayer = UITheme.Rect("Glow", root);
        UITheme.Place(glowLayer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), GlowPosition, Vector2.zero);
        Image glow = UITheme.Panel("Light", glowLayer, UITheme.WithAlpha(UITheme.Accent, 0.32f), 0);
        glow.sprite = UITheme.GlowSprite();
        UITheme.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1050f, 1050f));

        Image ringImage = UITheme.Panel("Ring", glowLayer, UITheme.WithAlpha(UITheme.Accent, 0.22f), 0);
        ringImage.sprite = UITheme.RingSprite();
        ring = ringImage.rectTransform;
        UITheme.Place(ring, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(620f, 620f));

        shockRing = UITheme.Panel("Shock", glowLayer, UITheme.WithAlpha(UITheme.Accent, 0f), 0);
        shockRing.sprite = UITheme.RingSprite();
        UITheme.Place(shockRing.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(620f, 620f));

        BuildEnemy();
        BuildMain();

        startTime = Time.unscaledTime;
        nextAttack = 2.2f;
    }

    /// <summary>Kenarlari yumusak maskelenmis iki dag katmani.</summary>
    private void BuildMountains()
    {
        if (data.mountains == null || data.mountains.Length == 0)
        {
            return;
        }

        RectTransform mask = UITheme.Rect("Mountains", root);
        UITheme.Place(mask, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, -40f), new Vector2(1150f, 760f));
        RectMask2D rectMask = mask.gameObject.AddComponent<RectMask2D>();
        rectMask.softness = new Vector2Int(240, 220);

        mountainsLayer = UITheme.Rect("Layer", mask);
        UITheme.Stretch(mountainsLayer);

        Color[] tints =
        {
            new Color(0.10f, 0.30f, 0.32f, 0.55f),
            new Color(0.06f, 0.19f, 0.21f, 0.9f)
        };
        float[] offsets = { -140f, 160f };

        for (int i = 0; i < data.mountains.Length && i < tints.Length; i++)
        {
            Image layer = Pixel(mountainsLayer, "Mountain " + i, data.mountains[i], 4.2f - i * 0.2f, tints[i]);
            layer.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            layer.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            layer.rectTransform.pivot = new Vector2(0.5f, 0f);
            layer.rectTransform.anchoredPosition = new Vector2(offsets[i], -60f);
        }
    }

    private void BuildMain()
    {
        mainGroup = UITheme.Rect("Main", root);
        UITheme.Place(mainGroup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), MainPosition, Vector2.zero);

        // Ada: 4 karo genislik, 2 satir.
        float tile = 16f * HeroScale;
        Island(mainGroup, 4, HeroScale, new Vector2(0f, Surface));

        if (data.tree != null)
        {
            Image tree = Pixel(mainGroup, "Tree", data.tree, 6f, new Color(0.62f, 0.72f, 0.72f, 1f));
            PlaceBottom(tree.rectTransform, new Vector2(-tile * 1.45f, Surface - 6f));
        }

        if (data.plants != null)
        {
            float[] xs = { -tile * 1.1f, tile * 0.75f, tile * 1.35f };
            for (int i = 0; i < data.plants.Length && i < xs.Length; i++)
            {
                if (data.plants[i] == null)
                {
                    continue;
                }
                Image plant = Pixel(mainGroup, "Plant " + i, data.plants[i], HeroScale * 0.75f, Color.white);
                PlaceBottom(plant.rectTransform, new Vector2(xs[i], Surface - 10f));
            }
        }

        if (data.stone != null)
        {
            Image stone = Pixel(mainGroup, "Stone", data.stone, 5f, Color.white);
            PlaceBottom(stone.rectTransform, new Vector2(tile * 1.55f, Surface - 14f));
        }

        Image shadow = UITheme.Panel("Shadow", mainGroup, new Color(0f, 0f, 0f, 0.5f), 0);
        shadow.sprite = UITheme.GlowSprite();
        UITheme.Place(shadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, Surface + 4f), new Vector2(300f, 46f));

        hero = Pixel(mainGroup, "Hero", data.heroIdle[0], HeroScale, Color.white);
        heroRt = hero.rectTransform;
        // Kahraman saga, rakibine donuk.
        heroBase = AnchorFeet(heroRt, data.heroIdle[0], HeroScale, new Vector2(0f, Surface), !data.heroFacesRight);
    }

    private void BuildEnemy()
    {
        enemyGroup = UITheme.Rect("Enemy", root);
        UITheme.Place(enemyGroup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), EnemyPosition, Vector2.zero);

        Island(enemyGroup, 2, EnemyIslandScale, Vector2.zero);

        if (data.enemyIdle == null || data.enemyIdle.Length == 0 || data.enemyIdle[0] == null)
        {
            return;
        }

        Image shadow = UITheme.Panel("Shadow", enemyGroup, new Color(0f, 0f, 0f, 0.45f), 0);
        shadow.sprite = UITheme.GlowSprite();
        UITheme.Place(shadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(170f, 30f));

        enemy = Pixel(enemyGroup, "Skeleton", data.enemyIdle[0], EnemyScale, enemyTint);
        enemyRt = enemy.rectTransform;
        // Iskelet sola, kahramana donuk.
        enemyBase = AnchorFeet(enemyRt, data.enemyIdle[0], EnemyScale, Vector2.zero, data.enemyFacesRight);
    }

    /// <summary>Ust ve alt satirdan olusan, kenarlari yuvarlak karolu ada.</summary>
    private void Island(RectTransform parent, int width, float scale, Vector2 surface)
    {
        if (data.islandTop == null || data.islandTop.Length < 3)
        {
            return;
        }

        float tile = 16f * scale;
        float left = surface.x - width * tile * 0.5f + tile * 0.5f;
        Sprite[][] rows = { data.islandTop, data.islandBottom };

        for (int r = 0; r < rows.Length; r++)
        {
            Sprite[] row = rows[r];
            if (row == null || row.Length < 3)
            {
                continue;
            }

            for (int c = 0; c < width; c++)
            {
                Sprite sprite = c == 0 ? row[0] : c == width - 1 ? row[2] : row[1];
                Image img = Pixel(parent, $"Tile {r}-{c}", sprite, scale, Color.white);
                UITheme.Place(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(left + c * tile, surface.y - tile * (r + 0.5f)), img.rectTransform.sizeDelta);
            }
        }
    }

    // ------------------------------------------------------------- Guncelleme

    void Update()
    {
        float t = Time.unscaledTime - startTime;
        float dt = Time.unscaledDeltaTime;

        // Giris: sagdan kayarak belirir.
        float enter = UIAnim.EaseOutCubic(Mathf.Clamp01((t - 0.15f) / 0.9f));
        rootGroup.alpha = enter;
        root.anchoredPosition = RootPosition + new Vector2((1f - enter) * 160f, 0f);

        // Fare konumuna gore katmanlar farkli hizda kayar.
        Vector2 mouse = Input.mousePosition;
        Vector2 target = new Vector2(
            Mathf.Clamp(mouse.x / Mathf.Max(1, Screen.width) * 2f - 1f, -1f, 1f),
            Mathf.Clamp(mouse.y / Mathf.Max(1, Screen.height) * 2f - 1f, -1f, 1f));
        parallax = Vector2.Lerp(parallax, target, 1f - Mathf.Exp(-4f * dt));

        if (mountainsLayer != null)
        {
            mountainsLayer.anchoredPosition = -parallax * 12f;
        }
        glowLayer.anchoredPosition = GlowPosition - parallax * 8f;
        mainGroup.anchoredPosition = MainPosition + new Vector2(0f, Mathf.Sin(t * 1.2f) * 10f) - parallax * 24f;
        enemyGroup.anchoredPosition = EnemyPosition + new Vector2(0f, Mathf.Sin(t * 1.0f + 1.3f) * 8f) - parallax * 16f;

        AnimateHero(t, dt);
        AnimateEnemy(dt);

        // Halka yavasca nefes alir; darbe aninda ikinci halka disari dalgalanir.
        ring.localScale = Vector3.one * (1f + Mathf.Sin(t * 1.4f) * 0.03f);
        shock = Mathf.MoveTowards(shock, 0f, dt * 1.8f);
        shockRing.color = UITheme.WithAlpha(UITheme.Accent, shock * 0.75f);
        shockRing.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.35f, 0.85f, shock);
    }

    private void AnimateHero(float t, float dt)
    {
        heroTimer += dt;

        if (!attacking)
        {
            hero.sprite = Frame(data.heroIdle, heroTimer, data.heroFrameRate);

            bool canAttack = data.heroAttack != null && data.heroAttack.Length > 0;
            if (canAttack && t >= nextAttack)
            {
                attacking = true;
                impactDone = false;
                heroTimer = 0f;
            }
        }
        else
        {
            int frame = Mathf.FloorToInt(heroTimer * data.attackFrameRate);
            if (frame >= data.heroAttack.Length)
            {
                attacking = false;
                heroTimer = 0f;
                nextAttack = t + Random.Range(2.6f, 4.4f);
                hero.sprite = data.heroIdle[0];
            }
            else
            {
                hero.sprite = data.heroAttack[frame];
                if (!impactDone && frame >= ImpactFrame)
                {
                    impactDone = true;
                    shock = 1f;
                    hitFlash = 1f;
                }
            }
        }

        // Darbe aninda hafifce one (saga, rakibe dogru) atilir.
        heroRt.anchoredPosition = heroBase + new Vector2(16f * shock * shock, 0f);
    }

    private void AnimateEnemy(float dt)
    {
        if (enemy == null)
        {
            return;
        }

        enemyTimer += dt;
        enemy.sprite = Frame(data.enemyIdle, enemyTimer, data.enemyFrameRate);

        // Darbe aninda kirmizi yanip geri itilir.
        hitFlash = Mathf.MoveTowards(hitFlash, 0f, dt * 3.5f);
        enemy.color = Color.Lerp(enemyTint, UITheme.Danger, hitFlash);
        enemyRt.anchoredPosition = enemyBase + new Vector2(10f * hitFlash, 0f);
    }

    // ---------------------------------------------------------------- Yardimci

    private static Sprite Frame(Sprite[] frames, float time, float rate)
    {
        int index = Mathf.FloorToInt(time * rate) % frames.Length;
        return frames[index] != null ? frames[index] : frames[0];
    }

    /// <summary>Sprite'i piksel olcegiyle (keskin) gosteren Image.</summary>
    private static Image Pixel(Transform parent, string name, Sprite sprite, float scale, Color color)
    {
        Image img = UITheme.Panel(name, parent, color, 0);
        img.sprite = sprite;
        if (sprite != null)
        {
            img.rectTransform.sizeDelta = sprite.rect.size * scale;
        }
        return img;
    }

    private static void PlaceBottom(RectTransform rt, Vector2 position)
    {
        Vector2 size = rt.sizeDelta;
        UITheme.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), position, size);
    }

    /// <summary>
    /// Ilk karenin gorunur kismina gore ayaklari zemine, govdeyi x merkezine
    /// hizalar. Sonraki karelerde ayni ofset kullanilir; animasyon titremez.
    /// flip: sprite yatayda aynalanir (ofset de aynalanir).
    /// </summary>
    private static Vector2 AnchorFeet(RectTransform rt, Sprite first, float scale, Vector2 ground, bool flip)
    {
        Rect visible = UITheme.VisibleRect(first);
        float bottomPx = visible.yMin;
        float centerPx = visible.center.x;

        float offsetX = (first.rect.width * 0.5f - centerPx) * scale * (flip ? -1f : 1f);
        Vector2 position = ground + new Vector2(offsetX, -bottomPx * scale);
        UITheme.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), position, first.rect.size * scale);
        rt.localScale = new Vector3(flip ? -1f : 1f, 1f, 1f);
        return position;
    }
}
