using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Kaybetme ve kazanma ekrani: baslik, sure / dusman / en iyi sure istatistikleri
/// ve Yeniden dene / Ana menu butonlari. Sahnedeki eski GameOverPanel ve WinPanel
/// yerine kullanilir; bu ekran yoksa onlara geri donulur.
/// GameUIBootstrap tarafindan oyun sahnesinde otomatik olusturulur.
/// </summary>
public class GameEndScreen : MonoBehaviour
{
    private static GameEndScreen instance;

    private CanvasGroup group;
    private RectTransform card;
    private TextMeshProUGUI overline;
    private TextMeshProUGUI title;
    private TextMeshProUGUI subtitle;
    private TextMeshProUGUI timeValue;
    private TextMeshProUGUI killsValue;
    private TextMeshProUGUI bestValue;
    private GameObject recordBadge;
    private Button primaryButton;
    private TextMeshProUGUI primaryLabel;

    private bool shown;

    /// <summary>Kaybetme ekranini acar. Ekran yoksa false doner.</summary>
    public static bool ShowDefeat()
    {
        return instance != null && instance.Show(false);
    }

    /// <summary>Kazanma ekranini acar. Ekran yoksa false doner.</summary>
    public static bool ShowVictory()
    {
        return instance != null && instance.Show(true);
    }

    void Awake()
    {
        instance = this;
        Build();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Build()
    {
        Canvas canvas = UITheme.CreateCanvas(transform, "GameEnd", 60);

        Image backdrop = UITheme.Panel("Backdrop", canvas.transform, UITheme.Backdrop, 0);
        UITheme.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;
        group = backdrop.gameObject.AddComponent<CanvasGroup>();

        Image cardImage = UITheme.Panel("Card", backdrop.transform, UITheme.Card, UITheme.RadiusLarge);
        card = cardImage.rectTransform;
        UITheme.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580f, 0f));
        UITheme.VStack(cardImage.gameObject, 44, 12f, TextAnchor.UpperCenter);
        UITheme.FitHeight(cardImage.gameObject);

        overline = UITheme.Overline("Overline", card, "", UITheme.Accent, TextAlignmentOptions.Center);
        title = UITheme.Heading("Title", card, "", 80f, UITheme.TextPrimary, TextAlignmentOptions.Center);
        subtitle = UITheme.Text("Subtitle", card, "", 24f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);

        UITheme.Spacer(card, 12f);

        // Istatistik satiri: Sure | Dusman | En iyi
        Image stats = UITheme.Panel("Stats", card, UITheme.Secondary, UITheme.RadiusMedium);
        HorizontalLayoutGroup row = UITheme.HStack(stats.gameObject, 0f, TextAnchor.MiddleCenter, true);
        row.padding = new RectOffset(12, 12, 18, 18);
        UITheme.Layout(stats.gameObject, -1f, 110f);

        timeValue = Stat(stats.transform, "Süre", out _);
        killsValue = Stat(stats.transform, "Düşman", out _);
        bestValue = Stat(stats.transform, "En iyi", out _);

        recordBadge = UITheme.Panel("Record", card, UITheme.WithAlpha(UITheme.Warning, 0.18f), UITheme.RadiusSmall).gameObject;
        UITheme.Layout(recordBadge, -1f, 40f);
        TextMeshProUGUI recordText = UITheme.Text("Text", recordBadge.transform, "Yeni rekor", 20f, UITheme.Warning, true,
            TextAlignmentOptions.Center);
        UITheme.Stretch(recordText.rectTransform);

        UITheme.Spacer(card, 12f);

        primaryButton = UITheme.Button(card, "Yeniden dene", true, Restart);
        primaryLabel = primaryButton.GetComponentInChildren<TextMeshProUGUI>();
        UITheme.Button(card, "Ana menü", false, GoToMainMenu);

        group.alpha = 0f;
        backdrop.gameObject.SetActive(false);
    }

    /// <summary>Ustte buyuk deger, altta kucuk etiket. Deger metnini dondurur.</summary>
    private TextMeshProUGUI Stat(Transform parent, string label, out TextMeshProUGUI labelText)
    {
        RectTransform column = UITheme.Rect(label, parent);
        UITheme.VStack(column.gameObject, 0, 4f, TextAnchor.MiddleCenter);

        TextMeshProUGUI value = UITheme.Text("Value", column, "—", 38f, UITheme.TextPrimary, UITheme.Weight.ExtraBold, TextAlignmentOptions.Center);
        labelText = UITheme.Text("Label", column, label, 18f, UITheme.TextSecondary, false, TextAlignmentOptions.Center);
        return value;
    }

    private bool Show(bool victory)
    {
        if (shown)
        {
            return true;
        }
        shown = true;

        GameSession.MarkEnded();
        float elapsed = GameSession.Elapsed;
        bool newRecord = victory && GameSession.TrySaveBestTime(elapsed);

        overline.text = victory ? "GÖREV TAMAMLANDI" : "YENİLDİN";
        overline.color = victory ? UITheme.Accent : UITheme.Danger;
        title.text = victory ? "Kazandın" : "Kaybettin";
        subtitle.text = victory
            ? "Şövalye kurtarıldı. Daha hızlı bitirebilir misin?"
            : "Bu sefer olmadı. Bir daha dene.";

        timeValue.text = GameSession.FormatTime(elapsed);
        killsValue.text = GameSession.TotalEnemies > 0
            ? $"{GameSession.Kills} / {GameSession.TotalEnemies}"
            : GameSession.Kills.ToString();
        bestValue.text = GameSession.HasBestTime ? GameSession.FormatTime(GameSession.BestTime) : "—";
        bestValue.color = newRecord ? UITheme.Warning : UITheme.TextPrimary;
        recordBadge.SetActive(newRecord);

        primaryLabel.text = victory ? "Tekrar oyna" : "Yeniden dene";

        StartCoroutine(Reveal());
        return true;
    }

    private IEnumerator Reveal()
    {
        // Oyuncu son ani gorsun diye kisa bir bekleme.
        yield return new WaitForSecondsRealtime(0.25f);
        yield return UIAnim.Show(group, card, true, 0.35f);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(primaryButton.gameObject);
        }
    }

    private void Restart()
    {
        SceneFader.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMainMenu()
    {
        SceneFader.LoadScene("Menu");
    }
}
