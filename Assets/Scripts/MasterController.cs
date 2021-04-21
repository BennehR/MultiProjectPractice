using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterController : MonoBehaviour
{
    //public bool gameIsPaused = false;

    public void ButtonSimon()
    {
        SceneManager.LoadScene(1);
    }

    public void ButtonQuitApplication()
    {
        Application.Quit();
    }

    public void ButtonMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
