using UnityEngine;

public class InfoPanelManager : MonoBehaviour
{
    public GameObject infoPanel; // Panel objesi

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Aç/kapa yap
            if (infoPanel != null)
            {
                bool isActive = infoPanel.activeSelf;
                infoPanel.SetActive(!isActive);
            }
        }
    }
}