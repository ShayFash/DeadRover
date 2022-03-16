using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausemenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject PauseUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            if (GameIsPaused)
            {
                FindObjectOfType<AudioManager>().Play("Confirm");
                Resume();
            }
            else
            {
                FindObjectOfType<AudioManager>().Play("Back");
                Pause();
            }
        }
    }

     public void Resume()
     {
         FindObjectOfType<AudioManager>().Play("Confirm");
         PauseUI.SetActive(false);
         Time.timeScale = 1f;
         GameIsPaused = false;
     }

    public void Pause()
    {
        FindObjectOfType<AudioManager>().Play("Forward");
        PauseUI.SetActive(true);
        Time.timeScale = 0f;  
        GameIsPaused = true;
    }
    
    public void LoadMenu()
    {
        FindObjectOfType<AudioManager>().Play("Back");
        Time.timeScale = 1f;
        Debug.Log("Loading Menu...");
        SceneManager.LoadScene("MainMenu");
        
    }

    public void QuitGame()
    {
        FindObjectOfType<AudioManager>().Play("Back");
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
