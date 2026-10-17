using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// "Modern minimal" arayuz temasi: renkler, olculer ve kodla UI olusturma
/// yardimcilari. Tum ekranlar (HUD, duraklatma, oyun sonu, ana menu) bu
/// sinifi kullanir; gorunumu degistirmek icin buradaki degerleri duzenlemek yeterli.
/// Yuvarlak kose ve vinyet gibi gorseller calisma zamaninda uretilir, asset gerekmez.
/// </summary>
public static class UITheme
{
    // ---- Renkler ----
    public static readonly Color TextPrimary = Hex(0xF2F5F8);
    public static readonly Color TextSecondary = Hex(0x8A96A3);
    public static readonly Color Accent = Hex(0x4FD1C5);
    public static readonly Color AccentHover = Hex(0x7FE3D9);
    public static readonly Color OnAccent = Hex(0x0B2B28);
    public static readonly Color Danger = Hex(0xFF6B6B);
    public static readonly Color Success = Hex(0x7BE38B);
    public static readonly Color Warning = Hex(0xFFD84A);

    // Oyun dunyasi acik renkli oldugu icin HUD panelleri koyu ve yari saydam.
    public static readonly Color HudPanel = new Color(0.06f, 0.08f, 0.11f, 0.66f);
    public static readonly Color Card = new Color(0.09f, 0.11f, 0.15f, 0.97f);
    public static readonly Color Backdrop = new Color(0.02f, 0.03f, 0.05f, 0.66f);
    public static readonly Color Secondary = new Color(1f, 1f, 1f, 0.08f);
    public static readonly Color SecondaryHover = new Color(1f, 1f, 1f, 0.16f);
    public static readonly Color Track = new Color(1f, 1f, 1f, 0.12f);
    public static readonly Color Ghost = new Color(1f, 1f, 1f, 0.55f);

    // Zengin metin icin ayni renklerin hex karsiliklari.
    public const string AccentHex = "#4FD1C5";
    public const string WarningHex = "#FFD84A";
    public const string SecondaryHex = "#8A96A3";

    // ---- Olculer ----
    public const int RadiusLarge = 18;
    public const int RadiusMedium = 12;
    public const int RadiusSmall = 8;
    public const float ButtonHeight = 60f;

    /// <summary>Tum kodla olusturulan canvas'larin referans cozunurlugu.</summary>
    public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    // ---- Yazi tipi (Exo 2, OFL lisansli; Assets/Resources/Fonts) ----

    public enum Weight
    {
        Regular,
        SemiBold,
        ExtraBold
    }

    private static TMP_FontAsset fontRegular;
    private static TMP_FontAsset fontSemiBold;
    private static TMP_FontAsset fontExtraBold;

    /// <summary>Govde metni yazi tipi.</summary>
    public static TMP_FontAsset Font => GetFont(Weight.Regular);

    /// <summary>
    /// Istenen agirlikta Exo 2. TTF'ler Resources'ta; TMP font varligi calisma
    /// aninda dinamik olarak uretilir, editorde Font Asset Creator gerekmez.
    /// Dosya bulunamazsa TMP'nin varsayilan yazi tipine donulur.
    /// </summary>
    public static TMP_FontAsset GetFont(Weight weight)
    {
        if (fontRegular == null || fontSemiBold == null || fontExtraBold == null)
        {
            LoadFonts();
        }

        switch (weight)
        {
            case Weight.SemiBold: return fontSemiBold;
            case Weight.ExtraBold: return fontExtraBold;
            default: return fontRegular;
        }
    }

    private static void LoadFonts()
    {
        TMP_FontAsset fallback = TMP_Settings.defaultFontAsset;

        fontRegular = CreateFont("Exo2-Regular", fallback);
        fontSemiBold = CreateFont("Exo2-SemiBold", fallback);
        fontExtraBold = CreateFont("Exo2-ExtraBold", fallback);

        // Zengin metindeki <b>, sahte kalinlastirma yerine gercek kalin dosyayi kullansin.
        SetBoldTypeface(fontRegular, fontSemiBold);
        SetBoldTypeface(fontSemiBold, fontExtraBold);
    }

    private static TMP_FontAsset CreateFont(string resourceName, TMP_FontAsset fallback)
    {
        Font source = Resources.Load<Font>("Fonts/" + resourceName);
        if (source == null)
        {
            Debug.LogWarning($"UITheme: Resources/Fonts/{resourceName} bulunamadi, varsayilan yazi tipi kullaniliyor.");
            return fallback;
        }

        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(source, 90, 9,
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
        if (asset == null)
        {
            return fallback;
        }

        asset.name = resourceName + " (Runtime)";
        if (fallback != null && fallback != asset)
        {
            // Exo 2'de olmayan karakterler (ok isaretleri vb.) LiberationSans'tan gelsin.
            asset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
        }
        return asset;
    }

    private static void SetBoldTypeface(TMP_FontAsset regular, TMP_FontAsset bold)
    {
        if (regular == null || bold == null || regular == bold)
        {
            return;
        }

        TMP_FontWeightPair[] table = regular.fontWeightTable;
        if (table != null && table.Length > 7)
        {
            table[7].regularTypeface = bold;
        }
    }

    private static Color Hex(int rgb, float a = 1f)
    {
        return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, a);
    }

    public static Color WithAlpha(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }

    // ---- Uretilen gorseller ----

    private static readonly Dictionary<int, Sprite> roundedCache = new Dictionary<int, Sprite>();
    private static Sprite vignetteSprite;
    private static Sprite glowSprite;

    /// <summary>9-slice icin kenarlari ayarlanmis, verilen yaricapli yuvarlak dikdortgen.</summary>
    public static Sprite RoundedSprite(int radius)
    {
        if (roundedCache.TryGetValue(radius, out Sprite cached) && cached != null)
        {
            return cached;
        }

        int size = radius * 2 + 2;
        Texture2D tex = NewTexture(size, size);
        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Kenara olan isaretli mesafe; 1 piksellik yumusatma ile kenar tirtiksiz olur.
                float px = x + 0.5f;
                float py = y + 0.5f;
                float qx = Mathf.Clamp(px, radius, size - radius);
                float qy = Mathf.Clamp(py, radius, size - radius);
                float d = Mathf.Sqrt((px - qx) * (px - qx) + (py - qy) * (py - qy));
                byte a = (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.name = "Rounded" + radius;
        roundedCache[radius] = sprite;
        return sprite;
    }

    /// <summary>Ortasi saydam, kenarlara dogru koyulasan beyaz vinyet. Renk Image.color ile verilir.</summary>
    public static Sprite VignetteSprite()
    {
        if (vignetteSprite != null)
        {
            return vignetteSprite;
        }

        vignetteSprite = RadialSprite(128, 0.55f, 1.3f, true);
        vignetteSprite.name = "Vignette";
        return vignetteSprite;
    }

    /// <summary>Ortasi parlak, kenarlara dogru sonen yumusak isik lekesi.</summary>
    public static Sprite GlowSprite()
    {
        if (glowSprite != null)
        {
            return glowSprite;
        }

        glowSprite = RadialSprite(128, 0f, 1f, false);
        glowSprite.name = "Glow";
        return glowSprite;
    }

    private static Sprite RadialSprite(int size, float inner, float outer, bool edgesOpaque)
    {
        Texture2D tex = NewTexture(size, size);
        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2f - 1f;
                float v = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(u * u + v * v);
                float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inner, outer, d));
                float a = edgesOpaque ? k : 1f - k;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Texture2D NewTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.hideFlags = HideFlags.DontSave;
        return tex;
    }

    // ---- Olusturma yardimcilari ----

    /// <summary>1920x1080'e gore olceklenen, ekran ustu bir canvas.</summary>
    public static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static RectTransform Rect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    /// <summary>Ebeveyni tamamen kaplar, istenirse kenarlardan iceri girer.</summary>
    public static void Stretch(RectTransform rt, float inset = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    /// <summary>Tek bir capa noktasina gore konum ve boyut verir.</summary>
    public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    /// <summary>radius 0 ise duz dikdortgen, aksi halde yuvarlak koseli panel.</summary>
    public static Image Panel(string name, Transform parent, Color color, int radius)
    {
        RectTransform rt = Rect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        if (radius > 0)
        {
            img.sprite = RoundedSprite(radius);
            img.type = Image.Type.Sliced;
        }
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    /// <summary>bold = true yari kalin (SemiBold) dosyayi kullanir.</summary>
    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
        bool bold = false, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        return Text(name, parent, text, size, color, bold ? Weight.SemiBold : Weight.Regular, alignment);
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
        Weight weight, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        RectTransform rt = Rect(name, parent);
        TextMeshProUGUI label = rt.gameObject.AddComponent<TextMeshProUGUI>();

        TMP_FontAsset font = GetFont(weight);
        if (font != null)
        {
            label.font = font;
        }
        label.text = text;
        label.fontSize = size;
        label.color = color;
        // Agirlik dosyadan geliyor; FontStyles.Bold sahte kalinlastirma yapardi.
        label.fontStyle = FontStyles.Normal;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Overflow;
        label.richText = true;
        label.raycastTarget = false;
        return label;
    }

    /// <summary>Ekran basliklari: en kalin agirlik, hafif sikistirilmis harf araligi.</summary>
    public static TextMeshProUGUI Heading(string name, Transform parent, string text, float size, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        TextMeshProUGUI label = Text(name, parent, text, size, color, Weight.ExtraBold, alignment);
        label.characterSpacing = -1f;
        return label;
    }

    /// <summary>Basligin ustundeki kucuk, aralikli buyuk harfli etiket.</summary>
    public static TextMeshProUGUI Overline(string name, Transform parent, string text, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        TextMeshProUGUI label = Text(name, parent, text, 20f, color, Weight.SemiBold, alignment);
        label.characterSpacing = 9f;
        return label;
    }

    /// <summary>Cocuklarini dikey dizen, yuksekligini icerige gore ayarlayabilen yerlesim.</summary>
    public static VerticalLayoutGroup VStack(GameObject go, int padding, float spacing, TextAnchor alignment,
        bool expandWidth = true)
    {
        VerticalLayoutGroup v = go.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(padding, padding, padding, padding);
        v.spacing = spacing;
        v.childAlignment = alignment;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = expandWidth;
        v.childForceExpandHeight = false;
        return v;
    }

    public static HorizontalLayoutGroup HStack(GameObject go, float spacing, TextAnchor alignment, bool expandWidth)
    {
        HorizontalLayoutGroup h = go.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = alignment;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = expandWidth;
        h.childForceExpandHeight = false;
        return h;
    }

    /// <summary>Yuksekligi icerige gore otomatik ayarlar (kartlar icin).</summary>
    public static void FitHeight(GameObject go)
    {
        ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public static LayoutElement Layout(GameObject go, float preferredWidth = -1f, float preferredHeight = -1f)
    {
        LayoutElement le = go.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = go.AddComponent<LayoutElement>();
        }
        le.preferredWidth = preferredWidth;
        le.preferredHeight = preferredHeight;
        // Sabit olculer yan yana dizilimde sikistirilmasin.
        if (preferredWidth > 0f)
        {
            le.minWidth = preferredWidth;
        }
        if (preferredHeight > 0f)
        {
            le.minHeight = preferredHeight;
        }
        return le;
    }

    /// <summary>
    /// Yan yana dizilimde (HStack) kalan genisligi paylasir. Icindeki metnin tek
    /// satirlik uzunlugunu istemez; boylece uzun metin tasmak yerine satira bolunur
    /// ve ayni agirliktaki kardesleriyle esit genislik alir.
    /// </summary>
    public static LayoutElement Flex(GameObject go, float weight = 1f)
    {
        LayoutElement le = go.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = go.AddComponent<LayoutElement>();
        }
        le.minWidth = 0f;
        le.preferredWidth = 0f;
        le.flexibleWidth = weight;
        return le;
    }

    /// <summary>Bos dikey bosluk.</summary>
    public static void Spacer(Transform parent, float height)
    {
        Layout(Rect("Spacer", parent).gameObject, -1f, height);
    }

    /// <summary>
    /// Tema butonu. primary = vurgu renkli ana eylem; ekran basina bir tane olmali.
    /// Hover/secili/basili animasyonlari UIButtonFx'te.
    /// </summary>
    public static Button Button(Transform parent, string label, bool primary, UnityAction onClick)
    {
        Color normal = primary ? Accent : Secondary;
        Color hover = primary ? AccentHover : SecondaryHover;

        Image bg = Panel(label, parent, normal, RadiusMedium);
        bg.raycastTarget = true;
        Layout(bg.gameObject, -1f, ButtonHeight);

        Button button = bg.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = bg;
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        TextMeshProUGUI text = Text("Label", bg.transform, label, 26f, primary ? OnAccent : TextPrimary, true,
            TextAlignmentOptions.Center);
        Stretch(text.rectTransform);

        bg.gameObject.AddComponent<UIButtonFx>().Init(bg, normal, hover);
        return button;
    }

    /// <summary>Kucuk bilgi etiketi (cip). Metin zengin metin olabilir.</summary>
    public static Image Chip(Transform parent, string name, out TextMeshProUGUI text, float fontSize = 20f)
    {
        Image chip = Panel(name, parent, Secondary, RadiusSmall);
        HorizontalLayoutGroup h = HStack(chip.gameObject, 0f, TextAnchor.MiddleCenter, false);
        h.padding = new RectOffset(12, 12, 5, 5);
        text = Text("Text", chip.transform, "", fontSize, TextPrimary);
        return chip;
    }

    /// <summary>Klavye tusu rozeti: "[F]" yerine kutu icinde harf.</summary>
    public static Image KeyBadge(Transform parent, string key, float height = 40f)
    {
        float fontSize = height * 0.5f;
        Image badge = Panel("Key " + key, parent, Accent, RadiusSmall);
        Layout(badge.gameObject, Mathf.Max(height, height * 0.4f + key.Length * fontSize * 0.68f), height);
        TextMeshProUGUI t = Text("Text", badge.transform, key, fontSize, OnAccent, Weight.ExtraBold, TextAlignmentOptions.Center);
        Stretch(t.rectTransform);
        return badge;
    }

    /// <summary>Ilerleme cubugunun dolu kismini 0-1 arasinda ayarlar.</summary>
    public static void SetFill(RectTransform fill, float fraction)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
    }

    /// <summary>Yuvarlak izli bir ilerleme cubugu; dolu kismin RectTransform'unu dondurur.</summary>
    public static RectTransform Bar(Transform parent, string name, float height, Color fillColor, out Image fillImage)
    {
        int radius = Mathf.Max(1, Mathf.RoundToInt(height * 0.5f));
        Image track = Panel(name, parent, Track, radius);
        Layout(track.gameObject, -1f, height);

        fillImage = Panel("Fill", track.transform, fillColor, radius);
        RectTransform fill = fillImage.rectTransform;
        Stretch(fill);
        return fill;
    }
}
