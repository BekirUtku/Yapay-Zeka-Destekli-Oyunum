using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Eðer oyun durmuþsa tekrar baþlat
        SceneManager.LoadScene("Menu"); // Sahne adýný Build Settings'e göre 
    }
}
