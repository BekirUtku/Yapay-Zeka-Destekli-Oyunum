using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public void PlayButton()
    {
        SceneFader.LoadScene(1);
    }

    public void QuitButton()
    {
        Application.Quit();
        Debug.Log("Oyundan Çıkıldı");
    }

}
