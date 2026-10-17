using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RestartGame()
    {
        SceneFader.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
