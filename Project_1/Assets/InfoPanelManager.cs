using System.Collections;
using UnityEngine;

/// <summary>
/// I tusuyla acilan oyun rehberini yonetir: acma/kapama, oyunu duraklatma ve
/// duraklatma menusuyle is birligi. Gorunum GuideScreen'de, kodla kurulur.
/// Ilk oynayista rehber Gorev sekmesiyle kendiliginden acilir.
/// </summary>
public class InfoPanelManager : MonoBehaviour
{
    private const string GuideSeenKey = "GuideSeen";

    [Header("Referanslar")]
    [Tooltip("Sahnedeki eski metin paneli. Yeni rehber kullanildigi icin baslangicta gizlenir.")]
    public GameObject infoPanel;

    [Header("Tuslar")]
    public KeyCode toggleKey = KeyCode.I;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Davranis")]
    [Tooltip("Rehber acikken Time.timeScale = 0 yapilir.")]
    public bool pauseWhileOpen = true;

    [Tooltip("Oyun ilk kez oynandiginda rehberi Gorev sekmesiyle otomatik ac.")]
    public bool openOnFirstPlay = true;

    private GuideScreen guide;
    private bool isOpen;

    // Duraklatma menusunden acildiysa oyun kapanista da durur, menuye donulur.
    private bool openedFromPause;
    private System.Action onClosedFromPause;

    public bool IsOpen => isOpen;

    /// <summary>
    /// Rehberin en son kapandigi kare. ESC hem rehberi kapatip hem de ayni karede
    /// duraklatma menusunu acmasin diye PauseMenu buna bakar.
    /// </summary>
    public int LastClosedFrame { get; private set; } = -1;

    void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        guide = gameObject.AddComponent<GuideScreen>();
        guide.CloseRequested = Close;

        if (openOnFirstPlay && PlayerPrefs.GetInt(GuideSeenKey, 0) == 0)
        {
            StartCoroutine(OpenOnFirstPlay());
        }
    }

    private IEnumerator OpenOnFirstPlay()
    {
        // Sahne ve kararma gecisi otursun, oyuncu dunyayi bir an gorsun.
        yield return new WaitForSecondsRealtime(0.8f);
        if (!isOpen)
        {
            TryOpen(0);
        }
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
                TryOpen(-1);
            }
        }
        else if (isOpen && Input.GetKeyDown(closeKey))
        {
            Close();
        }
    }

    /// <param name="page">Acilacak sekme; -1 en son bakilan sekme.</param>
    private void TryOpen(int page)
    {
        // Oyun baska bir sey tarafindan zaten durdurulmussa (duraklatma menusu,
        // Game Over / Kazandin) rehberi acma; kapanista timeScale'i yanlis yuklerdik.
        if (Mathf.Approximately(Time.timeScale, 0f) || GameSession.Ended)
        {
            return;
        }

        isOpen = true;
        MarkSeen();

        if (pauseWhileOpen)
        {
            Time.timeScale = 0f;
        }

        guide.Show(page);
    }

    /// <summary>
    /// Duraklatma menusunden acar. Oyun zaten durdugu icin timeScale'e dokunmaz;
    /// rehber kapaninca onClosed cagrilir.
    /// </summary>
    public void OpenFromPause(System.Action onClosed)
    {
        if (isOpen || !enabled || guide == null)
        {
            onClosed?.Invoke();
            return;
        }

        isOpen = true;
        openedFromPause = true;
        onClosedFromPause = onClosed;
        MarkSeen();

        guide.Show(-1);
    }

    private void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        LastClosedFrame = Time.frameCount;

        if (pauseWhileOpen && !openedFromPause)
        {
            // Her zaman 1'e don; rehber bir hitstop aninda acildiysa onceki deger
            // yavaslatilmis olabilir.
            Time.timeScale = 1f;
        }

        guide.Hide();

        if (openedFromPause)
        {
            openedFromPause = false;
            System.Action callback = onClosedFromPause;
            onClosedFromPause = null;
            callback?.Invoke();
        }
    }

    private static void MarkSeen()
    {
        if (PlayerPrefs.GetInt(GuideSeenKey, 0) == 0)
        {
            PlayerPrefs.SetInt(GuideSeenKey, 1);
            PlayerPrefs.Save();
        }
    }
}
