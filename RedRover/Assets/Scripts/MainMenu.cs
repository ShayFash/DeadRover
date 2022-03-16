using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame ()
    {
        FindObjectOfType<AudioManager>().Play("Confirm");
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void QuitGame ()
    {
        FindObjectOfType<AudioManager>().Play("Back");

        Application.Quit();
    }

    public void MuteToggle(bool muted)
    {

            FindObjectOfType<AudioManager>().MuteToggle("Theme");
    }
}
