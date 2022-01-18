using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    private int levelIndex;

    // Displays high score in lobby.
    private void Start()
    {
        levelIndex = PlayerPrefs.GetInt("levelIndex", 0);
    }

    // Loads game scene.
    public void Play()
    {
        SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_01);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1 + levelIndex);
    }

    public void Levels()
    {
        SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_01);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_01);
        Application.Quit();
    }
}