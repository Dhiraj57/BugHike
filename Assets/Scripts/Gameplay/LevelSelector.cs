using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    private int levelIndex;

    // Loads game scene.
    public void OpenLevel(int index)
    {
        PlayerPrefs.SetInt("levelIndex", index);
        SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_01);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + index);
    }

    public void Back()
    {
        SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_01);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Menu()
    {
        SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_01);
        SceneManager.LoadScene("Menu");
    }
}
