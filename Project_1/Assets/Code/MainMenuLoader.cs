using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Eğer oyun durmuşsa tekrar başlat
        SceneManager.LoadScene("Menu"); // Sahne adını Build Settings'e göre 
    }
}
