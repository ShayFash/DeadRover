using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    public GameObject EndUI;

    public void LoadMenu()
    {
        
        Debug.Log("Loading Menu...");
        SceneManager.LoadScene("MainMenu");

    }

}
