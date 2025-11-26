using UnityEngine;
using TMPro;

public class PlayerRescue : MonoBehaviour
{
    public Transform rescueTarget;
    public TextMeshProUGUI rescueMessageUI; // "F'ye basýlý tutarak kurtar" mesajý
    public GameObject winPanel;             // "Kazandýnýz" paneli
    public float rescueDistance = 5f;
    public float holdTime = 2f;

    private float holdTimer = 0f;
    private bool gameEnded = false;

    void Start()
    {
        if (rescueMessageUI != null)
            rescueMessageUI.gameObject.SetActive(false); // Yazý baþta gizli

        if (winPanel != null)
            winPanel.SetActive(false); // Kazanma paneli baþta kapalý
    }

    void Update()
    {
        if (gameEnded) return;

        float distance = Vector2.Distance(transform.position, rescueTarget.position);

        if (distance <= rescueDistance)
        {
            // UI yazýyý göster
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
            // UI yazýyý gizle
            if (rescueMessageUI != null && rescueMessageUI.gameObject.activeSelf)
                rescueMessageUI.gameObject.SetActive(false);

            holdTimer = 0f;
        }
    }

    void EndGame()
    {
        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true); // Kazandýnýz panelini aç

        Time.timeScale = 0f; // Oyunu durdur
        Debug.Log("Oyun Bitti!");
    }
}
