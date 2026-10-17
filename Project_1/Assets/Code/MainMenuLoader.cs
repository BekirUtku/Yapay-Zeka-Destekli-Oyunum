using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        // SceneFader timeScale'i de 1'e dondurur.
        SceneFader.LoadScene("Menu");
    }
}
