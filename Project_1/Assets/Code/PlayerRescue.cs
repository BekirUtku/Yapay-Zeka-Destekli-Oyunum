using UnityEngine;
using TMPro;

public class PlayerRescue : MonoBehaviour
{
    public Transform rescueTarget;
    public TextMeshProUGUI rescueMessageUI; // "F'ye basılı tutarak kurtar" mesajı (HUD yoksa kullanilir)
    public GameObject winPanel;             // "Kazandınız" paneli (GameEndScreen yoksa kullanilir)
    public float rescueDistance = 5f;
    public float holdTime = 2f;

    private float holdTimer = 0f;
    private bool gameEnded = false;

    private CharacterHealth health;

    /// <summary>Oyuncu kurtarma mesafesinde mi? HUD istemi buna bakar.</summary>
    public bool IsInRange { get; private set; }

    /// <summary>F'yi basili tutma ilerlemesi (0-1).</summary>
    public float HoldProgress => holdTime <= 0f ? 1f : Mathf.Clamp01(holdTimer / holdTime);

    void Start()
    {
        health = GetComponent<CharacterHealth>();

        if (rescueMessageUI != null)
            rescueMessageUI.gameObject.SetActive(false); // Yazı başta gizli

        if (winPanel != null)
            winPanel.SetActive(false); // Kazanma paneli başta kapalı
    }

    void Update()
    {
        if (gameEnded) return;

        // Olum animasyonunun 1.5 saniyesinde kurtarip kazanmak mumkun olmasin.
        if (health != null && health.IsDead)
        {
            SetInRange(false);
            holdTimer = 0f;
            return;
        }

        if (rescueTarget == null)
        {
            Debug.LogWarning($"{name}: PlayerRescue.rescueTarget atanmamis, script devre disi birakildi.", this);
            enabled = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, rescueTarget.position);

        if (distance <= rescueDistance)
        {
            SetInRange(true);

            if (Input.GetKey(KeyCode.F))
            {
                holdTimer += Time.deltaTime;

                if (holdTimer >= holdTime)
                    EndGame();
            }
            else
            {
                holdTimer = 0f;
            }
        }
        else
        {
            SetInRange(false);
            holdTimer = 0f;
        }
    }

    private void SetInRange(bool inRange)
    {
        IsInRange = inRange;

        // Yeni HUD istemi varsa eski dunya ici yazi gosterilmez.
        bool showLegacy = inRange && !GameHUD.Exists;
        if (rescueMessageUI != null && rescueMessageUI.gameObject.activeSelf != showLegacy)
            rescueMessageUI.gameObject.SetActive(showLegacy);
    }

    void EndGame()
    {
        gameEnded = true;
        SetInRange(false);

        Time.timeScale = 0f; // Oyunu durdur
        GameSession.MarkEnded();

        if (!GameEndScreen.ShowVictory() && winPanel != null)
            winPanel.SetActive(true); // Kazandınız panelini aç

        Debug.Log("Oyun Bitti!");
    }
}
