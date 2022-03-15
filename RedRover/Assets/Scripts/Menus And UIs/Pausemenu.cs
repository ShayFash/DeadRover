using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausemenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject PauseUI;

     public void Resume()
     {

         PauseUI.SetActive(false);
         Time.timeScale = 1f;
         GameIsPaused = false;
     }

    public void Pause()
    {
        PauseUI.SetActive(true);
        Time.timeScale = 0f;  
        GameIsPaused = true;
    }
    
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        Debug.Log("Loading Menu...");
        SceneManager.LoadScene("MainMenu");
        
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
