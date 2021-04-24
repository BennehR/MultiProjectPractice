using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterController : MonoBehaviour
{
    //public bool gameIsPaused = false;
    private int sceneLoaded = 0;

    public void ButtonSimon()
    {
        SceneManager.LoadScene(1);
        sceneLoaded = 1;
    }

    public void ButtonQuitApplication()
    {
        Application.Quit();
    }

    public void ButtonMainMenu()
    {
        SceneManager.LoadScene(0);
        SceneManager.UnloadSceneAsync(sceneLoaded);
        sceneLoaded = 0;
        Time.timeScale = 1;
    }
}
