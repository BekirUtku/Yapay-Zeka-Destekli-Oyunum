using UnityEngine;
using TMPro;

public class PlayerRescue : MonoBehaviour
{
    public Transform rescueTarget;
    public TextMeshProUGUI rescueMessageUI; // "F'ye basılı tutarak kurtar" mesajı
    public GameObject winPanel;             // "Kazandınız" paneli
    public float rescueDistance = 5f;
    public float holdTime = 2f;

    private float holdTimer = 0f;
    private bool gameEnded = false;

    void Start()
    {
        if (rescueMessageUI != null)
            rescueMessageUI.gameObject.SetActive(false); // Yazı başta gizli

        if (winPanel != null)
            winPanel.SetActive(false); // Kazanma paneli başta kapalı
    }

    void Update()
    {
        if (gameEnded) return;

        if (rescueTarget == null)
        {
            Debug.LogWarning($"{name}: PlayerRescue.rescueTarget atanmamis, script devre disi birakildi.", this);
            enabled = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, rescueTarget.position);

        if (distance <= rescueDistance)
        {
            // UI yazıyı göster
            if (rescueMessageUI != null && !rescueMessageUI.gameObject.activeSelf)
                rescueMessageUI.gameObject.SetActive(true);

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
            // UI yazıyı gizle
            if (rescueMessageUI != null && rescueMessageUI.gameObject.activeSelf)
                rescueMessageUI.gameObject.SetActive(false);

            holdTimer = 0f;
        }
    }

    void EndGame()
    {
        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true); // Kazandınız panelini aç

        Time.timeScale = 0f; // Oyunu durdur
        Debug.Log("Oyun Bitti!");
    }
}
